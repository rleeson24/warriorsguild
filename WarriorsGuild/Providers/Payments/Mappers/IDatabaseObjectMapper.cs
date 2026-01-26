using WarriorsGuild.Data.Models.Payments;

namespace WarriorsGuild.Providers.Payments.Mappers
{
    public interface IDatabaseObjectMapper
    {
        BillingAgreement CreateBillingAgreement( Frequency frequency, PriceOption po, int addlGuardians, int addlWarriors, PaymentMethod method, string subscriptionId );
        UserSubscription CreateUserSubscription( string userId, Guid billingAgreementId, bool isPayingParty, UserRole role );
        UserSubscription CreateUserSubscription( Guid userId, Guid billingAgreementId, Boolean isPayingParty, UserRole role );
        Dictionary<String, Int32> BuildPlanList( string stripePlanId, string? addlGuardianPlanId, string? addlWarriorPlanId, Int32 addlGuardians, Int32 addlWarriors );
        IEnumerable<UserSubscription> UpdateBillingAgreementId( Guid newBillingAgreementId, IEnumerable<UserSubscription> input );
    }
}