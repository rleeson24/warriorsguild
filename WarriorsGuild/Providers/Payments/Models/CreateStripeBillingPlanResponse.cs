namespace WarriorsGuild.Providers.Payments.Models
{
    public class CreateStripeBillingPlanResponse
    {
        public string? BasePlanId { get; internal set; }
        public string? BaseProductId { get; internal set; }
        public string? AdditionalGuardianPlanId { get; internal set; }
        public string? AdditionalGuardianProductId { get; internal set; }
        public string? AdditionalWarriorPlanId { get; internal set; }
        public string? AdditionalWarriorProductId { get; internal set; }
        public Boolean Success { get; set; }
    }
}