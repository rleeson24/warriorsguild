namespace WarriorsGuild.Models.Payments
{
    public class Subscription
    {
        public Guid Id { get; set; }
        public string? StripeCustomerId { get; set; }
        public string? SubscriptionId { get; set; }
        public int NumberOfWarriors { get; set; }
        public int NumberOfGuardians { get; set; }
    }
}