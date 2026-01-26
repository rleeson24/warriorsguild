using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.Data.Models.Ranks.Status;
using WarriorsGuild.DataAccess.Models;
using WarriorsGuild.Models;
using WarriorsGuild.Ranks.Models.Status;
using WarriorsGuild.Storage.Models;

namespace WarriorsGuild.Ranks
{
    public interface IRankStatusProvider
    {
        Task<RecordCompletionResponse> DeleteRankStatusAsync( RankStatusUpdateModel rankForStatus, Guid userIdForStatuses );
        Task<IEnumerable<MinimalGoalDetail>> GetAttachmentsForRankStatus( Guid requirementId, Guid userIdForStatuses );
        Task<IEnumerable<MinimalCrossDetail>> GetCrossesForRankStatus( Guid rankId, Guid requirementId, Guid userIdForStatuses );
        Task<FileDetail> GetProofOfCompletionAsync( Guid oneUseFileKey );
        Task<ProofOfCompletionAttachment> GetProofOfCompletionAttachmentByIdAsync( Guid attachmentId );
        Task<IEnumerable<MinimalRingDetail>> GetRingsForRankStatus( Guid rankId, Guid requirementId, Guid userIdForStatuses );
        Task<IEnumerable<RankStatus>> GetStatusesAsync( Guid rankId, Guid userId );
        Task<RankStatus> PostRankStatusAsync( RankStatus rankStatus );
        IQueryable<RankStatus> RankStatuses();
        Task<RecordCompletionResponse> RecordCompletionAsync( RankStatusUpdateModel rankForStatus, Guid userIdForStatuses );
        Task<Guid> RecordProofOfCompletionDocumentAsync( Guid reqId, Guid userId, string fileName, string extension );
        Task SaveProofOfCompletionOneUseFileKeyAsync( Guid attachmentId, Guid fileKey );
        Task<FileUploadResult> UploadProofOfCompletionAsync( string attachmentId, string fileExtension, byte[] file, string mediaType );
    }
}