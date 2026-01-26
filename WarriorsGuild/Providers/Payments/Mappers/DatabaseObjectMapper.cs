using WarriorsGuild.Data.Models.Payments;

namespace WarriorsGuild.Providers.Payments.Mappers
{
    public class DatabaseObjectMapper : IDatabaseObjectMapper
    {
        public BillingAgreement CreateBillingAgreement( Frequency frequency, PriceOption po, Int32 addlGuardians, Int32 addlWarriors, PaymentMethod method, string subscriptionId )
        {
            return new BillingAgreement()
            {
                Created = DateTime.UtcNow,
                LastPaid = DateTime.UtcNow,
                NextPaymentDue = po.Frequency == Frequency.Monthly ? DateTime.UtcNow.AddMonths( 1 ) : DateTime.UtcNow.AddYears( 1 ),
                PriceOption = po,
                AdditionalGuardians = addlGuardians,
                AdditionalWarriors = addlWarriors,
                PaymentMethod = method,
                StripeSubscriptionId = subscriptionId
            };
        }

        public UserSubscription CreateUserSubscription( string userId, Guid billingAgreementId, Boolean isPayingParty, UserRole role )
        {
            return CreateUserSubscription( new Guid( userId ), billingAgreementId, isPayingParty, role );
        }

        public UserSubscription CreateUserSubscription( Guid userId, Guid billingAgreementId, Boolean isPayingParty, UserRole role )
        {
            return new UserSubscription()
            {
                UserId = userId,
                BillingAgreementId = billingAgreementId,
                IsPayingParty = isPayingParty,
                Role = role
            };
        }

        public Dictionary<String, Int32> BuildPlanList( string stripePlanId, string? addlGuardianPlanId, string? addlWarriorPlanId, Int32 addlGuardians, Int32 addlWarriors )
        {
            var plans = new System.Collections.Generic.Dictionary<string, int>();
            plans.Add( stripePlanId, 1 );
            if ( addlGuardians > 0 && addlGuardianPlanId != null )
            {
                plans.Add( addlGuardianPlanId, addlGuardians );
            }
            if ( addlWarriors > 0 && addlWarriorPlanId != null )
            {
                plans.Add( addlWarriorPlanId, addlWarriors );
            }
            return plans;
        }

        public IEnumerable<UserSubscription> UpdateBillingAgreementId( Guid newBillingAgreementId, IEnumerable<UserSubscription> input )
        {
            return input.Where( i => !i.Revised.HasValue ).Select( i => CreateUserSubscription( i.UserId, newBillingAgreementId, i.IsPayingParty, i.Role ) );
        }
    }
}