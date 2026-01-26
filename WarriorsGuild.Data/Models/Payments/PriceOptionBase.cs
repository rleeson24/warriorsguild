using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WarriorsGuild.DataAccess;

namespace WarriorsGuild.Data.Models.Payments
{
    public abstract class PriceOptionBase : EntityBase
    {
        public PriceOptionBase()
        {
            Id = Guid.NewGuid();
        }

        public string Key { get; set; }

        [Required]
        public string Description { get; set; }

        public Frequency Frequency { get; set; }

        [Range( 0, 999 )]
        [Column( TypeName = "decimal(18,2)" )]
        public decimal Charge { get; set; }

        public string Currency { get; set; }

        public bool Show { get; set; }

        public int? TrialPeriodLength { get; set; }

        //[Range(0, 999)]
        //public Decimal? TrialPeriodCharge { get; set; }


        public int NumberOfGuardians { get; set; }

        public int NumberOfWarriors { get; set; }

        public string StripePlanId { get; set; }

        public string StripeProductId { get; set; }

        [NotMapped]
        public StripeDetail Stripe { get; set; } = new StripeDetail();
    }
}