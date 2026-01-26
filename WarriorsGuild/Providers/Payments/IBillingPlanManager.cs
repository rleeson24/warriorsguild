using WarriorsGuild.Models.Payments;

namespace WarriorsGuild.Providers.Payments
{
    public interface IBillingPlanManager
    {
        Task<CreateBillingPlanResponse> CreateBillingPlan( SavePriceOptionRequest request );
        Task<CreateStripeBillingPlanResponse> CreateStripeBillingPlan( SavePriceOptionRequest request );
    }
}