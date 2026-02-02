using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Models.Payments;

namespace WarriorsGuild.Providers.Payments
{
    /// <summary>
    /// Repository for AddOnPriceOption lookups. Encapsulates data access per DIP.
    /// </summary>
    public interface IAddOnPriceOptionRepository
    {
        AddOnPriceOption? FindExistingPlan( decimal charge, Frequency frequency, string? currency, int numberOfGuardians, int numberOfWarriors );
    }
}
