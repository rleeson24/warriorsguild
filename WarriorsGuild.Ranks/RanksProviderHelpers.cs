using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.Data.Models.Ranks.Status;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Ranks.Models.Status;

namespace WarriorsGuild.Ranks
{
    public interface IRanksProviderHelpers
    {
        Task<int> GetTotalCompletedPercent( Guid rankId, Guid userIdForStatuses );
        Task<bool> AllAssociatedRingsAndCrossesAreCompletedAndApprovedAsync( Guid rankId, IEnumerable<Guid> requirementIds, Guid approvalRecordUserId );
        Task<bool> AllPreviousRanksComplete( Guid rankId, Guid userId );
    }
    public class RanksProviderHelpers : IRanksProviderHelpers
    {
        private IGuildDbContext _dbContext { get; }

        public RanksProviderHelpers( IGuildDbContext dbContext )
        {
            _dbContext = dbContext;
        }

        public async Task<int> GetTotalCompletedPercent( Guid rankId, Guid userIdForStatuses )
        {
            //var requirements = Database.RankRequirements.Where( r => r.RankId == rankId ).Select( r => new { Id = r.Id.ToString(), r.Weight } );
            //var statusEntries = Database.RankStatuses.Where( rs => rs.RankId == rankId && rs.UserId == userIdForStatuses.ToString() && !rs.RecalledByWarriorTs.HasValue && !rs.ReturnedTs.HasValue );

            //var totalCompleted = await requirements.Join( statusEntries.Select( r => r.RankRequirementId.ToString() ), r => r.Id, s => s, ( r, s ) => r.Weight ).SumAsync();
            var requirements = await _dbContext.RankRequirements.Where( r => r.RankId == rankId ).Select( r => new { Id = r.Id.ToString(), r.Weight } ).ToArrayAsync();
            var statusEntries = await _dbContext.RankStatuses.Where( rs => rs.RankId == rankId && rs.UserId == userIdForStatuses && !rs.RecalledByWarriorTs.HasValue && !rs.ReturnedTs.HasValue ).ToArrayAsync();

            var fullQuery = requirements.Join( statusEntries.Select( r => r.RankRequirementId.ToString() ), r => r.Id.ToLower(), s => s, ( r, s ) => r.Weight );
            var totalCompleted = fullQuery.Sum();
            return totalCompleted;
        }

        public async Task<bool> AllAssociatedRingsAndCrossesAreCompletedAndApprovedAsync( Guid rankId, IEnumerable<Guid> requirementIds, Guid approvalRecordUserId )
        {
            var reqs = await _dbContext.RankRequirements.Where( rr => rr.RankId == rankId && requirementIds.Contains( rr.Id ) ).ToArrayAsync();
            var selectedRingsAreApproved = true;
            var selectedCrossesAreApproved = true;
            foreach ( var req in reqs )
            {
                if ( req.RequireRing )
                {
                    int approvedAssociatedRingsCount = await _dbContext.RankStatusRings.CountAsync( r => r.RankId == req.RankId
                                                           && r.RankRequirementId == req.Id
                                                           && r.UserId == approvalRecordUserId
                                                           && _dbContext.RingApprovals.Any( ra => ra.RingId == r.RingId && ra.ApprovedAt.HasValue ) );
                    selectedRingsAreApproved = approvedAssociatedRingsCount == req.RequiredRingCount.Value;
                }
                if ( req.RequireCross )
                {
                    var approvedAssociatedCrossesCount = await _dbContext.RankStatusCrosses.CountAsync( r => r.RankId == req.RankId
                                                            && r.RankRequirementId == req.Id
                                                            && r.UserId == approvalRecordUserId
                                                            && _dbContext.CrossApprovals.Any( ra => ra.CrossId == r.CrossId && ra.ApprovedAt.HasValue ) );
                    selectedCrossesAreApproved = approvedAssociatedCrossesCount == req.RequiredCrossCount.Value;
                }
                if ( !selectedCrossesAreApproved || !selectedRingsAreApproved )
                {
                    break;
                }
            }
            return selectedCrossesAreApproved && selectedRingsAreApproved;
        }

        public async Task<bool> AllPreviousRanksComplete( Guid rankId, Guid userId )
        {
            var rankForStatus = _dbContext.Set<Rank>().First( r => r.Id == rankId ).Index;
            var previousRankIds = await _dbContext.Set<Rank>().Where( r => r.Index < rankForStatus ).Select( r => r.Id ).ToArrayAsync();
            var approvals = await _dbContext.Set<RankApproval>().Where( r => previousRankIds.Contains( r.RankId ) && r.UserId == userId && r.ApprovedAt.HasValue && r.PercentComplete == 100 ).Select( r => r.RankId ).Distinct().CountAsync();
            return approvals == previousRankIds.Count();
        }
    }
}