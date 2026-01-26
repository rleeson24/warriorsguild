using Stripe;
using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Models.Payments;

namespace WarriorsGuild.Providers.Payments
{
    public class StripeCustomerProvider : IStripeCustomerProvider
    {
        private Stripe.CustomerService CustomerService { get; }
        private Stripe.CardService CardService { get; }
        private Stripe.SourceService SourceService { get; }

        public StripeCustomerProvider( Stripe.CustomerService stripeCustomerService, Stripe.SourceService stripeSourceService, Stripe.CardService cardService )
        {
            CardService = cardService;
            CustomerService = stripeCustomerService;
            SourceService = stripeSourceService;
        }

        public async Task<IEnumerable<PaymentMethodBase>> GetPaymentMethodsAsync( String customerId )
        {
            var customer = await CustomerService.GetAsync( customerId );
            var paymentMethods = customer.Sources;
            return paymentMethods.Select( p =>
                                {
                                    return MapToWarriorsGuildPaymentMethod( p, customer.DefaultSourceId );
                                } );
        }

        private static PaymentMethodBase MapToWarriorsGuildPaymentMethod( IPaymentSource source, String defaultSourceId )
        {
            var result = new PaymentMethodBase();
            if ( source is Stripe.Card )
            {
                var card = (Stripe.Card)source;
                result = CreateCard( card.Brand, card.Id, (int)card.ExpMonth, (int)card.ExpYear, card.Last4 );
            }
            else
            {
                throw new Exception( $"{source.GetType()} is not a supported SourceType" );
            }
            result.Id = source.Id;
            result.IsDefault = (source.Id == defaultSourceId);
            return result;
        }

        private static CardPaymentMethod CreateCard( String brand, String id, Int32 expirationMonth, Int32 expirationYear, String last4 )
        {
            return new CardPaymentMethod()
            {
                Brand = brand,
                CardId = id,
                ExpirationMonth = expirationMonth,
                ExpirationYear = expirationYear,
                Last4 = last4
            };
        }

        public async Task DeletePaymentMethod( string stripeCustomerId, string id )
        {
            var requestOptions = new Stripe.RequestOptions() { IdempotencyKey = Guid.NewGuid().ToString( "D" ) };
            await SourceService.DetachAsync( stripeCustomerId, id, requestOptions );
        }

        public async Task<CardPaymentMethod> AddPaymentMethodAsync( string stripeCustomerId, string tokenId )
        {
            var requestOptions = new Stripe.RequestOptions() { IdempotencyKey = Guid.NewGuid().ToString( "D" ) };
            var stripeCard = await CardService.CreateAsync( stripeCustomerId, new Stripe.CardCreateOptions() { Source = tokenId }, requestOptions );
            return CreateCard( stripeCard.Brand, stripeCard.Id, (int)stripeCard.ExpMonth, (int)stripeCard.ExpYear, stripeCard.Last4 );
        }

        public async Task SetDefaultPaymentMethodAsync( string stripeCustomerId, string paymentSourceId )
        {
            throw new NotImplementedException();
            //var requestOptions = new StripeRequestOptions() { IdempotencyKey = Guid.NewGuid().ToString( "D" ) };
            //var card = await CardService.GetAsync( stripeCustomerId, paymentSourceId );
            //await CustomerService.UpdateAsync( stripeCustomerId, new StripeCustomerUpdateOptions() { SourceCard = card. }, requestOptions );
        }

        public async Task<Tuple<string, PaymentMethodBase>> CreateAsync( string tokenId, string emailAddress )
        {
            var newCustomer = await CustomerService.CreateAsync( new Stripe.CustomerCreateOptions()
            {
                Email = emailAddress,
                Source = tokenId
            } );
            var stripeCard = await CardService.GetAsync( newCustomer.Id, newCustomer.DefaultSourceId );
            var card = CreateCard( stripeCard.Brand, stripeCard.Id, (int)stripeCard.ExpMonth, (int)stripeCard.ExpYear, stripeCard.Last4 );
            return new Tuple<String, PaymentMethodBase>( newCustomer.Id, card );
        }
    }
}