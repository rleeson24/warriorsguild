using Microsoft.EntityFrameworkCore;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Ranks.Status;
using WarriorsGuild.DataAccess;

namespace WarriorsGuild.Providers
{
    public interface ICovenantProvider
    {
        Task<bool> ContractHasBeenSigned( Guid userId );
        Task SignCovenant( Guid userId, string name );
    }

    public class CovenantProvider : ICovenantProvider
    {
        private readonly IGuildDbContext _dbContext;

        public CovenantProvider( IGuildDbContext dbContext )
        {
            this._dbContext = dbContext;
        }

        public async Task<bool> ContractHasBeenSigned( Guid userId )
        {
            return await _dbContext.SignedCovenants.SingleOrDefaultAsync( sc => sc.SignedBy == userId ) != null;
        }

        public async Task SignCovenant( Guid userId, string name )
        {
            var covenantReq = await _dbContext.RankRequirements.SingleOrDefaultAsync( rr => rr.ActionToComplete == "Read the Warrior's Guild Introduction and sign the Warriors Guild Covenant." );
            if ( covenantReq != null )
            {
                _dbContext.RankStatuses.Add( new RankStatus() { UserId = userId, WarriorCompleted = DateTime.UtcNow, RankId = covenantReq.RankId, RankRequirementId = covenantReq.Id } );
            }
            if ( _dbContext.SignedCovenants.SingleOrDefault( s => s.SignedBy == userId ) == null )
            {
                _dbContext.SignedCovenants.Add( new SignedCovenant() { SignedAt = DateTime.UtcNow, SignedBy = userId } );
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}
