using WarriorsGuild.Data.Models.Payments;

namespace WarriorsGuild.Models.Payments
{
    public class MySubscription
    {
        public UserSubscription UserSubscription { get; set; }
        public BillingAgreement BillingAgreement { get; set; }
    }
}