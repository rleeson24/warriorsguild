using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Models.Payments;

namespace WarriorsGuild.Areas.Payments.Mappers
{
    public interface IBillingPlanRequestMapper
    {
        SaveBillingPlanRequest CreateSaveBillingPlanRequest( Frequency frequency, string planName, Decimal charge, string currency, Decimal setupFee, Int32? trialPeriodLength );
        UpdateBillingPlanRequest CreateUpdateBillingPlanRequest( string planId, string productId, BillingPlanState state );
    }
}
