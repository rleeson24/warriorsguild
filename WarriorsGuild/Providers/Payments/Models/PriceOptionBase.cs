using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WarriorsGuild.DataAccess;

namespace WarriorsGuild.Providers.Payments.Models
{
    public abstract class PriceOptionBase : EntityBase
    {
        public string? Key { get; set; }

        [Required]
        public string? Description { get; set; }

        public Frequency Frequency { get; set; }

        [Range( 0, 999 )]
        [Column( TypeName = "decimal(18,2)" )]
        public Decimal Charge { get; set; }

        public string? Currency { get; set; }

        public Boolean Show { get; set; }

        public Int32? TrialPeriodLength { get; set; }

        //[Range(0, 999)]
        //public Decimal? TrialPeriodCharge { get; set; }


        public Int32 NumberOfGuardians { get; set; }

        public Int32 NumberOfWarriors { get; set; }

        public string? StripePlanId { get; set; }

        public string? StripeProductId { get; set; }

        [NotMapped]
        public StripeDetail Stripe { get; set; } = new StripeDetail();
    }
}