using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        private IRankRepository _repo { get; }

        public RanksProviderHelpers( IRankRepository repo )
        {
            _repo = repo;
        }

        public Task<int> GetTotalCompletedPercent( Guid rankId, Guid userIdForStatuses )
            => _repo.GetTotalCompletedPercentAsync( rankId, userIdForStatuses );

        public Task<bool> AllAssociatedRingsAndCrossesAreCompletedAndApprovedAsync( Guid rankId, IEnumerable<Guid> requirementIds, Guid approvalRecordUserId )
            => _repo.AllAssociatedRingsAndCrossesAreCompletedAndApprovedAsync( rankId, requirementIds, approvalRecordUserId );

        public Task<bool> AllPreviousRanksComplete( Guid rankId, Guid userId )
            => _repo.AllPreviousRanksCompleteAsync( rankId, userId );
    }
}