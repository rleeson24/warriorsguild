namespace WarriorsGuild.Models.Payments
{
    public class CreatePlanResponse
    {
        public string PlanId { get; set; } = default!;
        public string ProductId { get; set; } = default!;
        public bool Success { get; set; }
    }
}