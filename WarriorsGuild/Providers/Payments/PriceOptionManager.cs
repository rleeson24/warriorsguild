using Microsoft.EntityFrameworkCore;
using WarriorsGuild.Areas.Payments.Mappers;
using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Models.Payments;

namespace WarriorsGuild.Providers.Payments
{
    public class PriceOptionManager : IPriceOptionManager
    {
        private IGuildDbContext db;
        private readonly ILogger<PriceOptionManager> logger;

        private IStripePlanProvider StripeProvider { get; }
        private IPriceOptionMapper PriceOptionMapper { get; }
        private IBillingPlanRequestMapper BillingPlanRequestMapper { get; }

        public PriceOptionManager( IGuildDbContext db, IStripePlanProvider stripePlanProvider, IPriceOptionMapper priceOptionMapper, IBillingPlanRequestMapper billingPlanRequestMapper, ILogger<PriceOptionManager> logger )
        {
            this.db = db;
            StripeProvider = stripePlanProvider;
            PriceOptionMapper = priceOptionMapper;
            BillingPlanRequestMapper = billingPlanRequestMapper;
            this.logger = logger;
        }

        public async Task<IEnumerable<PriceOption>> ListPriceOptions()
        {
            var options = await (from priceOption in db.PriceOptions
                                            .Where( po => !po.Voided.HasValue )
                                            .Include( p => p.AdditionalGuardianPlan )
                                            .Include( p => p.AdditionalWarriorPlan )
                                            .Include( p => p.Perks )
                                 from perks in priceOption.Perks.DefaultIfEmpty().OrderBy( rr => rr.Index )
                                 select priceOption).ToArrayAsync();

            var stripePlans = await StripeProvider.ListPlans();

            var plans = options.Select( b => SetStripeDetails( b, stripePlans ) );
            return plans;
        }

        public async Task<PriceOption?> GetPriceOption( Guid id )
        {
            PriceOption? plan = null;
            var options = from priceOption in db.PriceOptions
                                            .Where( po => !po.Voided.HasValue )
                                            .Include( p => p.AdditionalGuardianPlan )
                                            .Include( p => p.AdditionalWarriorPlan )
                                            .Include( p => p.Perks )
                          from perks in priceOption.Perks.DefaultIfEmpty().OrderBy( rr => rr.Index )
                          select priceOption;
            var option = options.SingleOrDefault();

            if ( option != null )
            {
                var stripePlan = await StripeProvider.Get( option.StripePlanId );

                plan = SetStripeDetails( option, new[] { stripePlan } );
            }
            return plan;
        }

        private PriceOption SetStripeDetails( PriceOption r, IEnumerable<Stripe.Plan> stripePlans )
        {
            var primaryPlan = stripePlans.FirstOrDefault( p => p.Id == r.StripePlanId );
            if ( primaryPlan != null )
            {
                r.StripeStatus = primaryPlan.Active ? BillingPlanState.Active : BillingPlanState.Inactive;
                r.Stripe = new StripeDetail();
                r.Stripe.Active = primaryPlan.Active;
                r.Stripe.Name = primaryPlan.Product.Name;
                r.Stripe.Amount = primaryPlan.Amount != null ? primaryPlan.Amount.Value / 100 : 0;
                r.Stripe.Frequency = MapToFrequency( primaryPlan.Interval );
                r.TrialPeriodLength = (int?)primaryPlan.TrialPeriodDays / (MapToFrequency( primaryPlan.Interval ) == Frequency.Monthly ? 30 : 365);
            }
            if ( r.AdditionalGuardianPlan != null )
            {
                var addlGuardianPlan = stripePlans.FirstOrDefault( p => p.Id == r.AdditionalGuardianPlan.StripePlanId );
                if ( addlGuardianPlan != null )
                {
                    r.AdditionalGuardianPlan.Stripe = new StripeDetail();
                    r.AdditionalGuardianPlan.Stripe.Active = addlGuardianPlan.Active;
                    r.AdditionalGuardianPlan.Stripe.Name = addlGuardianPlan.Product.Name;
                    r.AdditionalGuardianPlan.Stripe.Amount = addlGuardianPlan.Amount != null ? addlGuardianPlan.Amount.Value / 100 : 0;
                    r.AdditionalGuardianPlan.Stripe.Frequency = MapToFrequency( addlGuardianPlan.Interval );
                    r.AdditionalGuardianPlan.TrialPeriodLength = (int?)addlGuardianPlan.TrialPeriodDays / (MapToFrequency( addlGuardianPlan.Interval ) == Frequency.Monthly ? 30 : 365);
                }
            }
            if ( r.AdditionalWarriorPlan != null )
            {
                var addlWarriorPlan = stripePlans.FirstOrDefault( p => p.Id == r.AdditionalWarriorPlan.StripePlanId );
                if ( addlWarriorPlan != null )
                {
                    r.AdditionalWarriorPlan.Stripe = new StripeDetail();
                    r.AdditionalWarriorPlan.Stripe.Active = addlWarriorPlan.Active;
                    r.AdditionalWarriorPlan.Stripe.Name = addlWarriorPlan.Product.Name;
                    r.AdditionalWarriorPlan.Stripe.Amount = addlWarriorPlan.Amount != null ? addlWarriorPlan.Amount.Value / 100 : 0;
                    r.AdditionalWarriorPlan.Stripe.Frequency = MapToFrequency( addlWarriorPlan.Interval );
                    r.AdditionalWarriorPlan.TrialPeriodLength = (int?)addlWarriorPlan.TrialPeriodDays / (MapToFrequency( addlWarriorPlan.Interval ) == Frequency.Monthly ? 30 : 365);
                }
            }

            return r;
        }

        public async Task Delete( PriceOption priceOption )
        {
            if ( String.IsNullOrEmpty( priceOption.StripePlanId ) )
            {
                db.PriceOptions.Remove( priceOption );
            }
            else
            {
                await StripeProvider.Update( BillingPlanRequestMapper.CreateUpdateBillingPlanRequest( priceOption.StripePlanId, priceOption.StripeProductId, BillingPlanState.Inactive ) );
                if ( priceOption.AdditionalGuardianPlan != null )
                {
                    await StripeProvider.Update( BillingPlanRequestMapper.CreateUpdateBillingPlanRequest( priceOption.AdditionalGuardianPlan.StripePlanId, priceOption.AdditionalGuardianPlan.StripeProductId, BillingPlanState.Inactive ) );
                }
                if ( priceOption.AdditionalWarriorPlan != null )
                {
                    await StripeProvider.Update( BillingPlanRequestMapper.CreateUpdateBillingPlanRequest( priceOption.AdditionalWarriorPlan.StripePlanId, priceOption.AdditionalWarriorPlan.StripeProductId, BillingPlanState.Inactive ) );
                }
                priceOption.Voided = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
        }

        private Frequency MapToFrequency( string? interval )
        {
            switch ( interval )
            {
                case "month":
                    return Frequency.Monthly;
                case "year":
                    return Frequency.Yearly;
                default:
                    return Frequency.Unknown;
            }
        }

        public async Task<PriceOption> Create( SavePriceOptionRequest request, CreateBillingPlanResponse createPlanResponse )
        {
            var priceOption = new PriceOption();
            priceOption.Description = request.Description;
            priceOption.Charge = request.Charge;
            priceOption.Currency = request.Currency;
            priceOption.Frequency = request.Frequency;
            priceOption.HasTrialPeriod = request.HasTrialPeriod;
            priceOption.NumberOfGuardians = request.NumberOfGuardians;
            priceOption.NumberOfWarriors = request.NumberOfWarriors;
            priceOption.Perks = request.Perks;
            priceOption.SetupFee = request.SetupFee;
            priceOption.Show = request.Show;
            priceOption.TrialPeriodLength = request.TrialPeriodLength;

            var paymentResponse = !String.IsNullOrEmpty( createPlanResponse.StripeResponse.BasePlanId );
            if ( !paymentResponse )
            {
                logger.LogError( "Stripe Plan not created", "An unexpected error occurred and the Stripe plan was not created", request, createPlanResponse );
            }

            priceOption.StripePlanId = createPlanResponse.StripeResponse.BasePlanId;
            priceOption.StripeProductId = createPlanResponse.StripeResponse.BaseProductId;
            priceOption.StripeStatus = !String.IsNullOrEmpty( priceOption.StripePlanId ) ? BillingPlanState.Created : BillingPlanState.Incomplete;
            if ( !String.IsNullOrEmpty( createPlanResponse.StripeResponse.AdditionalGuardianPlanId ) )
            {
                var addlGuardianPlan = db.AddOnPriceOptions.FirstOrDefault( o => o.StripePlanId == createPlanResponse.StripeResponse.AdditionalGuardianPlanId );
                if ( addlGuardianPlan != null )
                {
                    priceOption.AdditionalGuardianPlan = addlGuardianPlan;
                }
                else
                {
                    priceOption.AdditionalGuardianPlan = PriceOptionMapper.CreateGuardianAddOnPriceOption( request.Frequency, request.AdditionalGuardianCharge, request.Currency, 1, priceOption.TrialPeriodLength, createPlanResponse.StripeResponse.AdditionalGuardianPlanId, createPlanResponse.StripeResponse.AdditionalGuardianProductId );
                }
            }
            if ( !String.IsNullOrEmpty( createPlanResponse.StripeResponse.AdditionalWarriorPlanId ) )
            {
                var addlWarriorPlan = db.AddOnPriceOptions.FirstOrDefault( o => o.StripePlanId == createPlanResponse.StripeResponse.AdditionalWarriorPlanId );
                if ( addlWarriorPlan != null )
                {
                    priceOption.AdditionalWarriorPlan = addlWarriorPlan;
                }
                else
                {
                    priceOption.AdditionalWarriorPlan = PriceOptionMapper.CreateWarriorAddOnPriceOption( request.Frequency, request.AdditionalWarriorCharge, request.Currency, 1, priceOption.TrialPeriodLength, createPlanResponse.StripeResponse.AdditionalWarriorPlanId, createPlanResponse.StripeResponse.AdditionalWarriorProductId );
                }
            }
            priceOption.StripeStatus = BillingPlanState.Created;

            await db.PriceOptions.AddAsync( priceOption );
            await db.SaveChangesAsync();

            return priceOption;
        }

        public async Task Update( SavePriceOptionRequest request )
        {
            var existingParent = await db.PriceOptions
                  .Where( p => p.Id == request.Id )
                  .Include( p => p.Perks )
                  .SingleOrDefaultAsync();
            if ( existingParent != null )
            {
                #region Perks
                // Delete children
                foreach ( var existingChild in existingParent.Perks.ToArray() )
                {
                    db.PriceOptionPerks.Remove( existingChild );
                }
                existingParent.Perks.Clear();

                // Update and Insert children
                foreach ( var childModel in request.Perks )
                {
                    await db.PriceOptionPerks.AddAsync( childModel );
                }
                #endregion


                //if ( String.IsNullOrWhiteSpace( existingParent.StripePlanId ) )
                //{
                //	var createPlanResponse = await _paymentService.CreateStripeBillingPlan( request );
                //	existingParent.StripePlanId = createPlanResponse.StripePlanId;
                //	existingParent.AdditionalGuardianPlan = db.AddOnPriceOptions.FirstOrDefault( p => p.StripePlanId == createPlanResponse.StripePlanId );
                //	existingParent.AdditionalWarriorPlan = db.AddOnPriceOptions.FirstOrDefault( p => p.StripePlanId == createPlanResponse.StripePlanId );
                //	if ( String.IsNullOrEmpty( existingParent.StripePlanId ) )
                //	{
                //		return InternalServerError();
                //	}
                //}

                // Update parent
                existingParent.ModifiedDate = DateTime.UtcNow;
                db.Entry( existingParent ).CurrentValues.SetValues( request );

                await db.SaveChangesAsync();
            }
        }

        private bool PriceOptionExists( string? id )
        {
            return db.PriceOptions.Count( e => e.Key == id ) > 0;
        }
    }
}