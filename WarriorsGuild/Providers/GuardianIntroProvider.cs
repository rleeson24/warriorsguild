using Microsoft.EntityFrameworkCore;
using WarriorsGuild.Data.Models;
using WarriorsGuild.DataAccess;

namespace WarriorsGuild.Providers
{
    public interface IGuardianIntroProvider
    {
        Task<bool> GuardianHasWatchedIntroVideo( Guid guid );
        Task ConfirmVideoWatched( Guid userId );
    }

    public class GuardianIntroProvider : IGuardianIntroProvider
    {
        private readonly IGuildDbContext _dbContext;

        public GuardianIntroProvider( IGuildDbContext dbContext )
        {
            this._dbContext = dbContext;
        }

        public async Task<bool> GuardianHasWatchedIntroVideo( Guid userId )
        {
            return await _dbContext.SignedCovenants.SingleOrDefaultAsync( sc => sc.SignedBy == userId ) != null;
        }

        public async Task ConfirmVideoWatched( Guid userId )
        {
            if ( _dbContext.SignedCovenants.SingleOrDefault( s => s.SignedBy == userId ) == null )
            {
                _dbContext.SignedCovenants.Add( new SignedCovenant() { SignedAt = DateTime.UtcNow, SignedBy = userId } );
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
