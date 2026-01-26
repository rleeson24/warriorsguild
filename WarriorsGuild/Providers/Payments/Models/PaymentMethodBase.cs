namespace WarriorsGuild.Providers.Payments.Models
{
    public class PaymentMethodBase
    {
        public string? Id { get; internal set; }
        public bool IsDefault { get; internal set; }
    }
}