using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Data.Models.Payments
{
    public class SubscriptionBillingAgreement
    {
        [Key]
        public Guid SubscriptionId { get; set; }
        [Key]
        public Guid BillingAgreementId { get; set; }
        public virtual BillingAgreement BillingAgreement { get; set; }
    }
}