namespace WarriorsGuild.Models
{
    public class ManageSubscriptionViewModel
    {
        public string? StripePublishableKey { get; set; }
        public PaymentUrls? Urls { get; internal set; }
    }
}