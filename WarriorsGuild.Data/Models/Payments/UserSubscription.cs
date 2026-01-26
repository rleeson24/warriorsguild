using System;

namespace WarriorsGuild.Data.Models.Payments
{
    public class UserSubscription
    {
        public Guid UserId { get; set; }
        public Guid SubscriptionId { get; set; }
        public Guid BillingAgreementId { get; set; }
        public UserRole Role { get; set; }
        public bool IsPayingParty { get; set; }

        public DateTime? Revised { get; set; }
        public Guid? RevisedBy { get; set; }
    }
}