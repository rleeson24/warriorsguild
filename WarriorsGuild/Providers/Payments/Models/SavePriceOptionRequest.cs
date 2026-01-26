using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Providers.Payments.Models
{
    public class SavePriceOptionRequest
    {
        public Guid Id { get; set; }

        [Required]
        public string? Description { get; set; }

        public Frequency Frequency { get; set; }

        public Decimal SetupFee { get; set; }

        [Range( 0, 999 )]
        public Decimal Charge { get; set; }

        public Boolean Show { get; set; }

        public bool HasTrialPeriod { get; set; }

        public Int32? TrialPeriodLength { get; set; }

        //[Range(0, 999)]
        //public Decimal? TrialPeriodCharge { get; set; }

        public Int32 NumberOfGuardians { get; set; }

        public Int32 NumberOfWarriors { get; set; }

        [Range( 0, 999 )]
        public Decimal AdditionalGuardianCharge { get; set; }

        [Range( 0, 999 )]
        public Decimal AdditionalWarriorCharge { get; set; }

        public List<PriceOptionPerk> Perks { get; set; } = new List<PriceOptionPerk>();

        public string? Currency { get; internal set; }

        public string? StripePlanId { get; set; }
    }
}