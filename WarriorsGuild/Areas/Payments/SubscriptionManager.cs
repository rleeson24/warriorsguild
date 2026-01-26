using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Models.Payments;
using WarriorsGuild.Providers.Payments;
using WarriorsGuild.Providers.Payments.Mappers;

namespace WarriorsGuild.Areas.Payments
{
    public interface ISubscriptionManager
    {
        //IQueryable<BillingAgreement> GetAllBillingAgreements();
        Task<BillingAgreement?> GetBillingAgreement( Guid id );
        Task<MySubscription?> GetMySubscriptionAsync( string userId );
        Task<IEnumerable<UserSubscription>> GetUsersOnSubscriptionAsync( Guid billingAgreementId );

        Task<MySubscription> CreateSubscription( BillingViewModel request, ApplicationUser user );
        Task Unsubscribe( BillingAgreement billingAgreement, string userId );
    }

    public class SubscriptionManager : ISubscriptionManager
    {
        private ISubscriptionRepository SubscriptionRepo { get; }
        private IPriceOptionManager PriceOptionMgr { get; }

        private IStripeSubscriptionProvider StripeSubscriptions { get; }
        public IDatabaseObjectMapper DboMapper { get; }

        public SubscriptionManager( ISubscriptionRepository subscriptionRepo, IPriceOptionManager priceOptionManager, IStripeSubscriptionProvider stripeSubscriptionProvider, IDatabaseObjectMapper dboMapper )
        {
            SubscriptionRepo = subscriptionRepo;
            PriceOptionMgr = priceOptionManager;
            StripeSubscriptions = stripeSubscriptionProvider;
            DboMapper = dboMapper;
        }

        public async Task<MySubscription?> GetMySubscriptionAsync( string userId )
        {
            var dbResult = await SubscriptionRepo.GetMySubscriptionAsync( userId );
            if ( dbResult != null && !dbResult.BillingAgreement.Cancelled.HasValue )
            {
                return dbResult;
            }
            else
            {
                return null;
            }
        }

        //public IQueryable<BillingAgreement> GetAllBillingAgreements()
        //{
        //	return Db.BillingAgreements;
        //}

        public Task<BillingAgreement?> GetBillingAgreement( Guid id )
        {
            return SubscriptionRepo.GetBillingAgreement( id );
        }

        public async Task<IEnumerable<UserSubscription>> GetUsersOnSubscriptionAsync( Guid billingAgreementId )
        {
            return await SubscriptionRepo.GetUsersOnSubscriptionAsync( billingAgreementId );
        }

        public async Task<MySubscription> CreateSubscription( BillingViewModel request, ApplicationUser user )
        {
            var priceOption = await PriceOptionMgr.GetPriceOption( new Guid( request.BasePlanId ) );
            var currentSub = await SubscriptionRepo.GetMySubscriptionAsync( user.Id );
            if ( currentSub != null && !currentSub.BillingAgreement.Cancelled.HasValue )
            {
                await StripeSubscriptions.Unsubscribe( currentSub.BillingAgreement.StripeSubscriptionId );
                await SubscriptionRepo.Unsubscribe( currentSub.BillingAgreement, user.Id );
            }
            var plans = DboMapper.BuildPlanList( priceOption.StripePlanId, priceOption.AdditionalGuardianPlan.StripePlanId, priceOption.AdditionalWarriorPlan.StripePlanId, request.NumberOfAdditionalGuardians, request.NumberOfAdditionalWarriors );

            var stripeSubscriptionId = await StripeSubscriptions.Create( user, request.StripePaymentToken, plans, priceOption.SetupFee );

            var ba = DboMapper.CreateBillingAgreement( priceOption.Frequency, priceOption, request.NumberOfAdditionalGuardians, request.NumberOfAdditionalWarriors, PaymentMethod.Stripe, stripeSubscriptionId );

            var newSubscriptions = new List<UserSubscription>();
            if ( currentSub != null )
            {
                var usersOnSubscription = await SubscriptionRepo.GetUsersOnSubscriptionAsync( currentSub.BillingAgreement.Id );
                newSubscriptions.AddRange( DboMapper.UpdateBillingAgreementId( ba.Id, usersOnSubscription ) );
                foreach ( var us in usersOnSubscription )
                {
                    us.Revised = DateTime.UtcNow;
                    us.RevisedBy = new Guid( user.Id );
                }
            }
            else
            {
                newSubscriptions.Add( DboMapper.CreateUserSubscription( user.Id, ba.Id, true, UserRole.Guardian ) );
                foreach ( var u in user.ChildUsers )
                {
                    newSubscriptions.Add( DboMapper.CreateUserSubscription( u.Id, ba.Id, false, UserRole.Warrior ) );
                }
            }

            await SubscriptionRepo.CreateSubscription( ba, newSubscriptions );
            return new MySubscription()
            {
                BillingAgreement = ba,
                UserSubscription = newSubscriptions.Single( s => s.UserId == new Guid( user.Id ) )
            };
        }

        public async Task Unsubscribe( BillingAgreement billingAgreement, string userId )
        {
            await StripeSubscriptions.Unsubscribe( billingAgreement.StripeSubscriptionId );
            await SubscriptionRepo.Unsubscribe( billingAgreement, userId );
        }
    }
}