using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Models.Payments;

namespace WarriorsGuild.Areas.Payments.Mappers
{
    public interface IPriceOptionMapper
    {
        ManageablePriceOption MapToManageablePriceOption( PriceOption arg );
        SubscribeablePriceOption MapToSubscribeablePriceOption( PriceOption arg );
        SimplePriceOption MapToSimplePriceOption( PriceOption arg );
        AddOnPriceOption CreateGuardianAddOnPriceOption( Frequency frequency, decimal additionalGuardianCharge, string currency, int v, int? trialPeriodLength, string? additionalGuardianPlanId, string? additionalGuardianProductId );
        AddOnPriceOption CreateWarriorAddOnPriceOption( Frequency frequency, decimal additionalWarriorCharge, string currency, int v, int? trialPeriodLength, string? additionalWarriorPlanId, string? additionalWarriorProductId );
    }
}