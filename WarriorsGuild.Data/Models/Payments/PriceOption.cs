using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarriorsGuild.Data.Models.Payments
{
    public class PriceOption : PriceOptionBase
    {
        public PriceOption()
        {
            Id = Guid.NewGuid();
        }

        [Column( TypeName = "decimal(18,2)" )]
        public decimal SetupFee { get; set; }

        public bool HasTrialPeriod { get; set; }

        public List<PriceOptionPerk> Perks { get; set; }

        public DateTime? Voided { get; set; }

        public virtual AddOnPriceOption AdditionalGuardianPlan { get; set; }

        public virtual AddOnPriceOption AdditionalWarriorPlan { get; set; }

        //public String PaypalPlanId { get; set; }

        [NotMapped]
        public BillingPlanState StripeStatus { get; set; }
    }
}