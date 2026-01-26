using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.Data.Models.Ranks.Status;
using WarriorsGuild.Ranks.ViewModels;

namespace WarriorsGuild.Ranks
{
    public interface IRankApprovalsProvider
    {
        Task<IEnumerable<PendingApprovalDetail>> AllApprovalsForRank( Guid rankId, Guid userIdForStatuses );
        Task<RecordCompletionResponse> ApproveProgressAsync( Guid approvalRecordId, Guid currentUserId );
        Task<IEnumerable<PendingApprovalDetail>> GetPendingApprovalsAsync( Guid userId );
        Task MarkGuardianReviewedAsync( RankStatus existingChild );
        Task NotifyGuardiansOfRequestForPromotion( Rank rank, int totalCompleted, Guid userIdForStatuses );
        Task RecallAsync( Guid approvalRecordId, Guid userId );
        Task ReturnAsync( Guid approvalRecordId, Guid userId, string userReason );
        Task<RecordCompletionResponse> SubmitForApprovalAsync( Guid rankId, Guid userIdForStatuses );
    }
}