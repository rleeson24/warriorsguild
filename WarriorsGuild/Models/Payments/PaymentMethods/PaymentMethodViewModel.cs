namespace WarriorsGuild.Models.Payments.PaymentMethods
{
    public class PaymentMethodViewModel
    {

        public IEnumerable<PaymentMethodViewModelItem> PaymentMethods { get; set; } = new PaymentMethodViewModelItem[ 0 ];
    }
}