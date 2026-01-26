using System;

namespace WarriorsGuild.Data.Models.Payments
{
    public class BillingAgreement
    {
        public BillingAgreement()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastPaid { get; set; }
        public DateTime NextPaymentDue { get; set; }
        public DateTime? Cancelled { get; set; }
        public BillingAgreementStatus Status { get; set; }
        public int AdditionalGuardians { get; set; }
        public int AdditionalWarriors { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string StripeSubscriptionId { get; set; }
        public virtual PriceOption PriceOption { get; set; }
    }
}