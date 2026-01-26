namespace WarriorsGuild.Providers.Payments.Models
{
    public class StripeDetail
    {
        public Boolean Active { get; set; }
        public string? Name { get; set; }
        public Decimal Amount { get; set; }
        public Frequency Frequency { get; set; }
    }
}