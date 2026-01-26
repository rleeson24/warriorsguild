namespace WarriorsGuild.Providers.Payments.Models
{
    public class CardPaymentMethod : PaymentMethodBase
    {
        public string? CardId { get; internal set; }
        public int ExpirationMonth { get; internal set; }
        public int ExpirationYear { get; internal set; }
        public string? Brand { get; internal set; }
        public string? Last4 { get; internal set; }
    }
}