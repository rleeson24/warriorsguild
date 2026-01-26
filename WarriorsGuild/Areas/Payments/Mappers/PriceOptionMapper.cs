using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Models.Payments;

namespace WarriorsGuild.Areas.Payments.Mappers
{
    public class PriceOptionMapper : IPriceOptionMapper
    {
        public ManageablePriceOption MapToManageablePriceOption( PriceOption r )
        {
            return new ManageablePriceOption()
            {
                Id = r.Id,
                AdditionalGuardianCharge = r.AdditionalGuardianPlan.Charge,
                AdditionalWarriorCharge = r.AdditionalWarriorPlan.Charge,
                Charge = r.Charge,
                Description = r.Description,
                Frequency = r.Frequency,
                HasTrialPeriod = r.HasTrialPeriod,
                Key = r.Key,
                NumberOfGuardians = r.NumberOfGuardians,
                NumberOfWarriors = r.NumberOfWarriors,
                Perks = r.Perks,
                SetupFee = r.SetupFee,
                Show = r.Show,
                StripePlanId = r.StripePlanId,
                TrialPeriodLength = r.TrialPeriodLength,
                StripeStatus = r.Stripe.Active ? BillingPlanState.Active : BillingPlanState.Inactive

            };
        }

        public SubscribeablePriceOption MapToSubscribeablePriceOption( PriceOption r )
        {
            return new SubscribeablePriceOption()
            {
                Id = r.Id,
                AdditionalGuardianPlan = r.AdditionalGuardianPlan,
                AdditionalWarriorPlan = r.AdditionalWarriorPlan,
                Charge = r.Charge,
                Description = r.Description,
                Frequency = r.Frequency,
                HasTrialPeriod = r.HasTrialPeriod,
                NumberOfGuardians = r.NumberOfGuardians,
                NumberOfWarriors = r.NumberOfWarriors,
                Perks = r.Perks,
                SetupFee = r.SetupFee,
                StripePlanId = r.StripePlanId,
                TrialPeriodLength = r.TrialPeriodLength,
                StripeStatus = r.Stripe.Active ? BillingPlanState.Active : BillingPlanState.Inactive

            };
        }

        public SimplePriceOption MapToSimplePriceOption( PriceOption r )
        {
            return new SimplePriceOption()
            {
                Id = r.Id,
                AdditionalGuardianCharge = r.AdditionalGuardianPlan.Charge,
                AdditionalWarriorCharge = r.AdditionalWarriorPlan.Charge,
                Charge = r.Charge,
                Name = r.Key,
                Frequency = r.Frequency,
                NumberOfGuardians = r.NumberOfGuardians,
                NumberOfWarriors = r.NumberOfWarriors,
                Perks = r.Perks
            };
        }

        public AddOnPriceOption CreateGuardianAddOnPriceOption( Frequency frequency, Decimal charge, string currency, Int32 numberOfGuardians, Int32? trialPeriodLength, string? stripePlanId, string? stripeProductId )
        {
            return new AddOnPriceOption()
            {
                Frequency = frequency,
                Charge = charge,
                Currency = currency,
                NumberOfGuardians = numberOfGuardians,
                TrialPeriodLength = trialPeriodLength,
                Description = $"Additional Guardian Plan - {frequency.ToString()}",
                StripePlanId = stripePlanId,
                StripeProductId = stripeProductId
            };
        }

        public AddOnPriceOption CreateWarriorAddOnPriceOption( Frequency frequency, Decimal charge, string currency, Int32 numberOfGuardians, Int32? trialPeriodLength, string? stripePlanId, string? stripeProductId )
        {
            return new AddOnPriceOption()
            {
                Frequency = frequency,
                Charge = charge,
                Currency = currency,
                NumberOfWarriors = numberOfGuardians,
                TrialPeriodLength = trialPeriodLength,
                Description = $"Additional Warrior Plan - {frequency.ToString()}",
                StripePlanId = stripePlanId,
                StripeProductId = stripeProductId
            };
        }
    }
}