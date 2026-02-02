using System.Threading.Tasks;

namespace WarriorsGuild.DataAccess
{
    /// <summary>
    /// Default implementation wrapping the scoped DbContext. Shares the same instance
    /// as repositories, so SaveChangesAsync persists all staged changes from any repository.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IGuildDbContext _context;

        public UnitOfWork( IGuildDbContext context )
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
