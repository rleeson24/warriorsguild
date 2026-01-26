using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Models.Payments;

namespace WarriorsGuild.Areas.Payments.Mappers
{
    public class BillingPlanRequestMapper : IBillingPlanRequestMapper
    {
        public SaveBillingPlanRequest CreateSaveBillingPlanRequest( Frequency frequency, string planName, Decimal charge, string currency, Decimal setupFee, Int32? trialPeriodLength )
        {
            var result = new SaveBillingPlanRequest();
            //result.MaxFailAttempts = 10;
            //result.PlanDescription = priceOption.Description;
            result.PlanFrequency = frequency;
            //result.PlanId = planId;
            result.PlanName = planName;
            result.RegularPlanPrice = charge;
            result.Currency = currency;
            //result.RequestUrl = $"{Request.RequestUri.Scheme}://{Request.RequestUri.Authority}/Manage/ManageSubscription";
            result.SetupFee = setupFee;
            result.TrialPlanLength = trialPeriodLength;
            //result.TrialPlanPrice = priceOption.HasTrialPeriod ? priceOption.TrialPeriodCharge : (Decimal?)null;
            return result;
        }

        public UpdateBillingPlanRequest CreateUpdateBillingPlanRequest( string planId, string productId, BillingPlanState state )
        {
            return new UpdateBillingPlanRequest() { PlanId = planId, ProductId = productId, Status = state };
        }
    }
}