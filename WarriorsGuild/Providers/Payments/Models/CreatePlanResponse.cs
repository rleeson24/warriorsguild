namespace WarriorsGuild.Providers.Payments.Models
{
    public class CreatePlanResponse
    {
        public string? PlanId { get; set; }
        public string? ProductId { get; set; }
        public bool Success { get; set; }
    }
}