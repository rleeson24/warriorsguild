using Microsoft.AspNetCore.Identity;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Payments;

namespace WarriorsGuild.Providers.Payments
{
    public interface ICustomerManager
    {
        Task<IEnumerable<PaymentMethodBase>> GetPaymentMethods( string stripeCustomerId );
        Task DeletePaymentMethod( string stripeCustomerId, string id );
        Task<PaymentMethodBase> AddPaymentMethod( ApplicationUser user, string tokenId, UserManager<ApplicationUser> userManager );
        Task SetDefaultPaymentMethod( string stripeCustomerId, string id );
    }
}