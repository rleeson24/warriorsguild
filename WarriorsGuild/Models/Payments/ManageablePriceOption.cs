using System.ComponentModel.DataAnnotations;
using WarriorsGuild.Data.Models.Payments;

namespace WarriorsGuild.Models.Payments
{
    public class ManageablePriceOption
    {
        public Guid Id { get; internal set; }

        public string? Key { get; set; }

        [Required]
        public string? Description { get; set; }

        public Frequency Frequency { get; set; }

        public decimal Charge { get; set; }

        public bool Show { get; set; }

        [Range( 1, 500 )]
        public int NumberOfGuardians { get; set; }

        [Range( 1, 500 )]
        public int NumberOfWarriors { get; set; }

        public decimal SetupFee { get; set; }

        public bool HasTrialPeriod { get; set; }

        public int? TrialPeriodLength { get; set; }

        //[Range(0, 999)]
        //public Decimal? TrialPeriodCharge { get; set; }

        public List<PriceOptionPerk> Perks { get; set; } = new List<PriceOptionPerk>();

        public decimal AdditionalGuardianCharge { get; set; }

        public decimal AdditionalWarriorCharge { get; set; }

        public string? StripePlanId { get; set; }

        public BillingPlanState StripeStatus { get; set; }
    }
}