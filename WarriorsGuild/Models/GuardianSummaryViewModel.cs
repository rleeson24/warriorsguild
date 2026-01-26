namespace WarriorsGuild.Models
{
    public class GuardianSummaryViewModel
    {
        public string? Id { get; internal set; }
        public string? Name { get; internal set; }
        public string? Username { get; internal set; }
        public Boolean HasAvatar { get; internal set; }
        public string? SubscriptionDescription { get; internal set; }
        public DateTime SubscriptionExpires { get; internal set; }
    }
}