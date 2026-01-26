namespace WarriorsGuild.Models.Payments.PaymentMethods
{
    public class PaymentMethodViewModelItem
    {
        public PaymentMethodViewModelItem()
        {
        }

        public PaymentMethodType Type { get; set; }
        public string? CardId { get; set; }
        public string? Brand { get; set; }
        public string? Last4 { get; set; }
        public string? ExpirationDate { get; set; }
        public string Id { get; internal set; }
        public bool IsDefault { get; set; }
    }
}