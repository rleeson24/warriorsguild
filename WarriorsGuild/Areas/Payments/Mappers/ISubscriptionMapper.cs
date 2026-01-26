using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Models.Payments;

namespace WarriorsGuild.Areas.Payments.Mappers
{
    public interface ISubscriptionMapper
    {
        BillingAgreementViewModel CreateViewModel( MySubscription subscriptionAndAgreement, IEnumerable<SubscriptionUser> usersOnSubscription );
        SubscriptionUser MapToSubscriptionUser( ApplicationUser user, UserSubscription subscription, bool userIsGuardian, bool userIsWarrior );
    }
}