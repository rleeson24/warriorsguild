using WarriorsGuild.Data.Models.Payments;

namespace WarriorsGuild.Models.Payments
{
    public class SubscribeablePriceOption
    {
        public Guid Id { get; internal set; }

        public string? Description { get; set; }

        public Frequency Frequency { get; set; }

        public decimal Charge { get; set; }

        public int NumberOfGuardians { get; set; }

        public int NumberOfWarriors { get; set; }

        public decimal SetupFee { get; set; }

        public bool HasTrialPeriod { get; set; }

        public int? TrialPeriodLength { get; set; }

        public List<PriceOptionPerk> Perks { get; set; } = new List<PriceOptionPerk>();

        public AddOnPriceOption? AdditionalGuardianPlan { get; set; }

        public AddOnPriceOption? AdditionalWarriorPlan { get; set; }

        public string? StripePlanId { get; set; }

        public BillingPlanState StripeStatus { get; set; }
    }
}