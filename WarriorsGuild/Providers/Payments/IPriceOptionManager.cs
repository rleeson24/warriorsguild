using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Models.Payments;

namespace WarriorsGuild.Providers.Payments
{
    public interface IPriceOptionManager
    {
        Task<IEnumerable<PriceOption>> ListPriceOptions();
        Task<PriceOption?> GetPriceOption( Guid id );
        Task Delete( PriceOption priceOption );
        Task<PriceOption> Create( SavePriceOptionRequest request, CreateBillingPlanResponse createPlanResponse );
        Task Update( SavePriceOptionRequest request );
    }
}