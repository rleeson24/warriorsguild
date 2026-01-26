using WarriorsGuild.Data.Models;

namespace WarriorsGuild.Providers.Payments
{
    public interface IStripeSubscriptionProvider
    {
        Task<string> Create( ApplicationUser user, string token, Dictionary<string, int> plans, decimal setupFee = 0 );
        void CreateSingleCharge();
        Task Unsubscribe( string subscriptionId );
    }
}