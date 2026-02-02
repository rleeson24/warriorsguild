using WarriorsGuild.Areas.Payments.Mappers;
using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Models.Payments;

namespace WarriorsGuild.Providers.Payments
{
    public class BillingPlanManager : IBillingPlanManager
    {
        private readonly IAddOnPriceOptionRepository _addOnPriceOptionRepository;
        private IStripePlanProvider StripeProvider { get; }
        private IPriceOptionMapper PriceOptionMapper { get; }
        private IBillingPlanRequestMapper BillingPlanRequestMapper { get; }

        public BillingPlanManager( IAddOnPriceOptionRepository addOnPriceOptionRepository, IStripePlanProvider stripePlanProvider, IPriceOptionMapper priceOptionMapper, IBillingPlanRequestMapper billingPlanRequestMapper )
        {
            _addOnPriceOptionRepository = addOnPriceOptionRepository;
            StripeProvider = stripePlanProvider;
            PriceOptionMapper = priceOptionMapper;
            BillingPlanRequestMapper = billingPlanRequestMapper;
        }

        public async Task<CreateBillingPlanResponse> CreateBillingPlan( SavePriceOptionRequest request )
        {
            var response = new CreateBillingPlanResponse();
            if ( String.IsNullOrWhiteSpace( request.StripePlanId ) )
            {
                response.StripeResponse = await CreateStripeBillingPlan( request );
            }
            return response;
        }


        public async Task<CreateStripeBillingPlanResponse> CreateStripeBillingPlan( SavePriceOptionRequest request )
        {
            request.Currency = "usd";

            var savePlanRequest = BillingPlanRequestMapper.CreateSaveBillingPlanRequest( request.Frequency, request.Description, request.Charge, request.Currency, request.SetupFee, request.HasTrialPeriod ? request.TrialPeriodLength : (Int32?)null );
            var mainResponse = await StripeProvider.Create( savePlanRequest );
            if ( mainResponse.Success )
            {
                var addlGuardianProductId = String.Empty;
                var addlGuardianPlanId = String.Empty;
                var addlWarriorProductId = String.Empty;
                var addlWarriorPlanId = String.Empty;
                var existingGuardianPlan = _addOnPriceOptionRepository.FindExistingPlan( request.AdditionalGuardianCharge, request.Frequency, request.Currency, 1, 0 );
                var existingWarriorPlan = _addOnPriceOptionRepository.FindExistingPlan( request.AdditionalWarriorCharge, request.Frequency, request.Currency, 0, 1 );
                if ( existingGuardianPlan != null )
                {
                    addlGuardianProductId = existingGuardianPlan.StripeProductId;
                    addlGuardianPlanId = existingGuardianPlan.StripePlanId;
                }
                else
                {
                    savePlanRequest = BillingPlanRequestMapper.CreateSaveBillingPlanRequest( request.Frequency, $"Additional Guardian - {request.Frequency.ToString()}", request.AdditionalGuardianCharge, request.Currency, 0, request.HasTrialPeriod ? request.TrialPeriodLength : (Int32?)null );
                    var addlGuardianResponse = await StripeProvider.Create( savePlanRequest );
                    addlGuardianProductId = addlGuardianResponse.ProductId;
                    addlGuardianPlanId = addlGuardianResponse.PlanId;
                }
                if ( existingWarriorPlan != null )
                {
                    addlWarriorProductId = existingWarriorPlan.StripeProductId;
                    addlWarriorPlanId = existingWarriorPlan.StripePlanId;
                }
                else
                {
                    savePlanRequest = BillingPlanRequestMapper.CreateSaveBillingPlanRequest( request.Frequency, $"Additional Warrior - {request.Frequency.ToString()}", request.AdditionalWarriorCharge, request.Currency, 0, request.HasTrialPeriod ? request.TrialPeriodLength : (Int32?)null );
                    var addlWarriorResponse = await StripeProvider.Create( savePlanRequest );
                    addlWarriorProductId = addlWarriorResponse.ProductId;
                    addlWarriorPlanId = addlWarriorResponse.PlanId;
                }

                return new CreateStripeBillingPlanResponse()
                {
                    BasePlanId = mainResponse.PlanId,
                    BaseProductId = mainResponse.ProductId,
                    AdditionalGuardianProductId = addlGuardianProductId,
                    AdditionalGuardianPlanId = addlGuardianPlanId,
                    AdditionalWarriorProductId = addlWarriorProductId,
                    AdditionalWarriorPlanId = addlWarriorPlanId
                };
            }
            else
            {
                return new CreateStripeBillingPlanResponse();
            }
        }
    }
}