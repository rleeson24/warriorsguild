using WarriorsGuild.Data.Models.Payments;

namespace WarriorsGuild.Models.Payments
{
    public class UpdateBillingPlanRequest
    {
        public string? ProductId { get; set; }
        public string? PlanId { get; set; }
        public BillingPlanState Status { get; set; }
    }
}