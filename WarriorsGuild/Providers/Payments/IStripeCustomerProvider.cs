using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Models.Payments;

namespace WarriorsGuild.Providers.Payments
{
    public interface IStripeCustomerProvider
    {
        Task<IEnumerable<PaymentMethodBase>> GetPaymentMethodsAsync( string customerId );
        Task DeletePaymentMethod( string stripeCustomerId, string id );
        Task<CardPaymentMethod> AddPaymentMethodAsync( string stripeCustomerId, string tokenId );
        Task SetDefaultPaymentMethodAsync( string stripeCustomerId, string paymentSourceId );
        Task<Tuple<String, PaymentMethodBase>> CreateAsync( string tokenId, string emailAddress );
    }
}