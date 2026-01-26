using Microsoft.AspNetCore.Identity;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Payments;

namespace WarriorsGuild.Providers.Payments
{
    public class CustomerManager : ICustomerManager
    {
        private IStripeCustomerProvider StripeCustomers { get; }

        public CustomerManager( IStripeCustomerProvider stripeCustomers )
        {
            StripeCustomers = stripeCustomers;
        }

        public async Task<IEnumerable<PaymentMethodBase>> GetPaymentMethods( String stripeCustomerId )
        {
            return await StripeCustomers.GetPaymentMethodsAsync( stripeCustomerId );
        }

        public async Task DeletePaymentMethod( string stripeCustomerId, string id )
        {
            await StripeCustomers.DeletePaymentMethod( stripeCustomerId, id );
        }

        public async Task<PaymentMethodBase> AddPaymentMethod( ApplicationUser user, string tokenId, UserManager<ApplicationUser> userManager )
        {
            if ( String.IsNullOrEmpty( user.StripeCustomerId ) )
            {
                var newCustomer = await StripeCustomers.CreateAsync( tokenId, user.Email );
                user.StripeCustomerId = newCustomer.Item1;
                await userManager.UpdateAsync( user );
                return newCustomer.Item2;
            }
            else
            {
                return await StripeCustomers.AddPaymentMethodAsync( user.StripeCustomerId, tokenId );
            }
        }

        public async Task SetDefaultPaymentMethod( string stripeCustomerId, string id )
        {
            await StripeCustomers.SetDefaultPaymentMethodAsync( stripeCustomerId, id );
        }
    }
}