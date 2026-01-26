using WarriorsGuild.Data.Models.Payments;

namespace WarriorsGuild.Models.Payments
{
    public class CardPaymentMethod : PaymentMethodBase
    {
        public string CardId { get; internal set; } = default!;
        public int ExpirationMonth { get; internal set; }
        public int ExpirationYear { get; internal set; }
        public string Brand { get; internal set; } = default!;
        public string Last4 { get; internal set; } = default!;
    }
}