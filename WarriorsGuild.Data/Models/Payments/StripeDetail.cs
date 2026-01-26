namespace WarriorsGuild.Data.Models.Payments
{
    public class StripeDetail
    {
        public bool Active { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public Frequency Frequency { get; set; }
    }
}