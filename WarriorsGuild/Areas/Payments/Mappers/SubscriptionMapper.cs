using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Models.Payments;

namespace WarriorsGuild.Areas.Payments.Mappers
{
    public class SubscriptionMapper : ISubscriptionMapper
    {
        public BillingAgreementViewModel CreateViewModel( MySubscription subscriptionAndAgreement, IEnumerable<SubscriptionUser> usersOnSubscription )
        {
            var bavm = new BillingAgreementViewModel();

            var billingAgreement = subscriptionAndAgreement.BillingAgreement;
            var subscription = subscriptionAndAgreement.UserSubscription;

            bavm.AdditionalCostPerGuardian = billingAgreement.PriceOption.AdditionalGuardianPlan.Charge;
            bavm.AdditionalCostPerWarrior = billingAgreement.PriceOption.AdditionalWarriorPlan.Charge;
            bavm.AdditionalGuardians = billingAgreement.AdditionalGuardians;
            bavm.AdditionalWarriors = billingAgreement.AdditionalWarriors;
            bavm.Charge = billingAgreement.PriceOption.Charge;
            bavm.Currency = billingAgreement.PriceOption.Currency;
            bavm.DateCreated = billingAgreement.Created;
            bavm.Description = billingAgreement.PriceOption.Description;
            bavm.Frequency = billingAgreement.PriceOption.Frequency;
            bavm.IsPayingParty = subscription.IsPayingParty;
            bavm.LastPaid = billingAgreement.LastPaid;
            bavm.NextPaymentDue = billingAgreement.NextPaymentDue;
            bavm.NumberOfGuardians = billingAgreement.PriceOption.NumberOfGuardians;
            bavm.NumberOfWarriors = billingAgreement.PriceOption.NumberOfWarriors;
            bavm.PaymentMethod = billingAgreement.PaymentMethod;
            bavm.Perks = billingAgreement.PriceOption.Perks;
            bavm.PriceOptionId = billingAgreement.PriceOption.Id;
            bavm.SetupFee = billingAgreement.PriceOption.SetupFee;
            bavm.HasTrialPeriod = billingAgreement.PriceOption.HasTrialPeriod;
            bavm.TrialPeriodLength = billingAgreement.PriceOption.TrialPeriodLength;
            bavm.Users = usersOnSubscription.ToList();
            return bavm;
        }

        public SubscriptionUser MapToSubscriptionUser( ApplicationUser user, UserSubscription subscription, Boolean userIsGuardian, Boolean userIsWarrior )
        {
            return new SubscriptionUser()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsWarrior = userIsWarrior,
                IsGuardian = userIsGuardian,
                IsPayingParty = subscription.IsPayingParty
            };
        }
    }
}