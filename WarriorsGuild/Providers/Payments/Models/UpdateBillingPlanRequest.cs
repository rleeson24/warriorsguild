namespace WarriorsGuild.Providers.Payments.Models
{
    public class UpdateBillingPlanRequest
    {
        public string? ProductId { get; set; }
        public string? PlanId { get; set; }
        public BillingPlanState Status { get; set; }
    }
}