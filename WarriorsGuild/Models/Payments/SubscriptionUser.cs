namespace WarriorsGuild.Models.Payments
{
    public class SubscriptionUser
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsWarrior { get; set; }
        public bool IsGuardian { get; set; }
        public bool IsPayingParty { get; set; }
    }
}