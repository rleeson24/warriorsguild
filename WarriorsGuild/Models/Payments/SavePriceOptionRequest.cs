using System.ComponentModel.DataAnnotations;
using WarriorsGuild.Data.Models.Payments;

namespace WarriorsGuild.Models.Payments
{
    public class SavePriceOptionRequest
    {
        public Guid Id { get; set; }

        [Required]
        public string? Description { get; set; }

        public Frequency Frequency { get; set; }

        public decimal SetupFee { get; set; }

        [Range( 0, 999 )]
        public decimal Charge { get; set; }

        public bool Show { get; set; }

        public bool HasTrialPeriod { get; set; }

        public int? TrialPeriodLength { get; set; }

        //[Range(0, 999)]
        //public Decimal? TrialPeriodCharge { get; set; }

        public int NumberOfGuardians { get; set; }

        public int NumberOfWarriors { get; set; }

        [Range( 0, 999 )]
        public decimal AdditionalGuardianCharge { get; set; }

        [Range( 0, 999 )]
        public decimal AdditionalWarriorCharge { get; set; }

        public List<PriceOptionPerk> Perks { get; set; } = new List<PriceOptionPerk>();

        public string? Currency { get; internal set; }

        public string? StripePlanId { get; set; }
    }
}