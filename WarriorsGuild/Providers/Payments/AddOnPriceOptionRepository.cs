using System.Linq;
using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Models.Payments;

namespace WarriorsGuild.Providers.Payments
{
    public class AddOnPriceOptionRepository : IAddOnPriceOptionRepository
    {
        private readonly IGuildDbContext _dbContext;

        public AddOnPriceOptionRepository( IGuildDbContext dbContext )
        {
            _dbContext = dbContext;
        }

        public AddOnPriceOption? FindExistingPlan( decimal charge, Frequency frequency, string? currency, int numberOfGuardians, int numberOfWarriors )
        {
            return _dbContext.AddOnPriceOptions.FirstOrDefault( o =>
                o.NumberOfGuardians == numberOfGuardians
                && o.NumberOfWarriors == numberOfWarriors
                && o.Charge == charge
                && o.Frequency == frequency
                && o.Currency == currency
                && o.Show
                && !string.IsNullOrEmpty( o.StripePlanId ) );
        }
    }
}
