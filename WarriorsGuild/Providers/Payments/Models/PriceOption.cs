using System.ComponentModel.DataAnnotations.Schema;

namespace WarriorsGuild.Providers.Payments.Models
{
    public class PriceOption : PriceOptionBase
    {

        [Column( TypeName = "decimal(18,2)" )]
        public Decimal SetupFee { get; set; }

        public Boolean HasTrialPeriod { get; set; }

        public List<PriceOptionPerk> Perks { get; set; } = new List<PriceOptionPerk>();

        public DateTime? Voided { get; set; }

        public virtual AddOnPriceOption? AdditionalGuardianPlan { get; set; }

        public virtual AddOnPriceOption? AdditionalWarriorPlan { get; set; }

        //public string? PaypalPlanId { get; set; }

        [NotMapped]
        public BillingPlanState StripeStatus { get; set; }
    }
}