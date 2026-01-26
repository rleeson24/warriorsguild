namespace WarriorsGuild.Providers.Payments.Models
{
    public class Subscription
    {
        public Guid Id { get; set; }
        public string? StripeCustomerId { get; set; }
        public string? SubscriptionId { get; set; }
        public Int32 NumberOfWarriors { get; set; }
        public Int32 NumberOfGuardians { get; set; }
    }
}