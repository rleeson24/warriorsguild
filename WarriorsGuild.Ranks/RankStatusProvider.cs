using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.Data.Models.Ranks.Status;
using WarriorsGuild.DataAccess;
using WarriorsGuild.DataAccess.Models;
using WarriorsGuild.Email;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Models;
using WarriorsGuild.Providers;
using WarriorsGuild.Ranks.Mappers;
using WarriorsGuild.Ranks.Models.Status;
using WarriorsGuild.Storage;
using WarriorsGuild.Storage.Models;

namespace WarriorsGuild.Ranks
{
    public class RankStatusProvider : IRankStatusProvider
    {
        private IRankMapper _rankMapper { get; }
        private IBlobProvider _fileProvider { get; }
        private IRanksProviderHelpers RpHelpers { get; }
        private IDateTimeProvider _dateTimeProvider { get; }
        private IUnitOfWork _uow { get; }
        private IRankRepository _repo { get; }

        public RankStatusProvider( IUnitOfWork uow, IRankRepository repo, IRankMapper rankMapper, IDateTimeProvider dateTimeProvider, IRanksProviderHelpers rpHelpers,
                            IBlobProvider fileProvider )
        {
            _uow = uow;
            _rankMapper = rankMapper;
            _fileProvider = fileProvider;
            RpHelpers = rpHelpers;
            _dateTimeProvider = dateTimeProvider;
            _repo = repo;
        }
        public IQueryable<RankStatus> RankStatuses()
        {
            return _repo.RankStatuses();
        }

        public async Task<IEnumerable<RankStatus>> GetStatusesAsync( Guid rankId, Guid userId )
        {
            return await _repo.RankStatuses().Where( rs => rs.RankId == rankId && rs.UserId == userId && !rs.RecalledByWarriorTs.HasValue && !rs.ReturnedTs.HasValue ).ToArrayAsync();
        }

        public async Task<RecordCompletionResponse> RecordCompletionAsync( RankStatusUpdateModel rankForStatus, Guid userIdForStatuses )
        {
            var response = new RecordCompletionResponse();

            if ( await RpHelpers.AllPreviousRanksComplete( rankForStatus.RankId, userIdForStatuses ) )
            {
                var ss = await _repo.GetRequirementStatusAsync( rankForStatus.RankId, rankForStatus.RankRequirementId, userIdForStatuses );

                if ( ss == null )
                {
                    var approvalrecord = await _repo.GetLatestPendingOrApprovedRankApprovalAsync( rankForStatus.RankId, userIdForStatuses );
                    var percentApproved = approvalrecord?.PercentComplete ?? 0;
                    if ( approvalrecord == null || approvalrecord.ApprovedAt.HasValue )
                    {
                        int totalCompleted = await RpHelpers.GetTotalCompletedPercent( rankForStatus.RankId, userIdForStatuses );
                        if ( totalCompleted - percentApproved < 33 )
                        {
                            var statusToSave = _rankMapper.CreateRankStatus( rankForStatus.RankId, rankForStatus.RankRequirementId, warriorCompletedTs: _dateTimeProvider.GetCurrentDateTime(), null, userIdForStatuses );
                            response.Status = statusToSave;
                            await _repo.PostRankStatusAsync( statusToSave );
                            response.Success = true;
                        }
                        else
                        {
                            response.Success = false;
                            response.Error = "You cannot complete any more requirements until you are promoted.  Request promotion to continue on your path.";
                        }
                    }
                    else
                    {
                        response.Success = false;
                        response.Error = "There is currently a pending approval record.  Your Guardian must approve it before you can continue.";
                    }
                }
                else
                {
                    response.Success = false;
                    response.Error = "You have already completed this requirement.";
                }
            }
            else
            {
                response.Success = false;
                response.Error = "You must complete the Ranks in order.";
            }

            return response;
        }

        public Task<RecordCompletionResponse> DeleteRankStatusAsync( RankStatusUpdateModel rankForStatus, Guid userIdForStatuses )
        {
            return _repo.DeleteRankStatusWithRelatedAsync( rankForStatus, userIdForStatuses );
        }

        public async Task<RankStatus> PostRankStatusAsync( RankStatus rankStatus )
        {
            _repo.AddStatusEntry( rankStatus );
            await _uow.SaveChangesAsync();
            return rankStatus;
        }

        public async Task<FileUploadResult> UploadProofOfCompletionAsync( string attachmentId, string fileExtension, byte[] file, string mediaType )
        {
            return await _fileProvider.UploadFileAsync( WarriorsGuildFileType.ProofOfCompletion, file, attachmentId.ToString(), mediaType );
        }

        public Task<ProofOfCompletionAttachment> GetProofOfCompletionAttachmentByIdAsync( Guid attachmentId )
        {
            return _repo.GetProofOfCompletionAttachmentByIdAsync( attachmentId );
        }

        public Task SaveProofOfCompletionOneUseFileKeyAsync( Guid attachmentId, Guid fileKey )
        {
            return _repo.SaveProofOfCompletionOneUseFileKeyAsync( attachmentId, fileKey );
        }

        public async Task<Guid> RecordProofOfCompletionDocumentAsync( Guid reqId, Guid userId, string fileName, string extension )
        {
            var entity = new ProofOfCompletionAttachment()
            {
                RequirementId = reqId,
                StorageKey = fileName,
                FileExtension = extension,
                UserId = userId
            };
            return await _repo.AddProofOfCompletionAttachmentAsync( entity );
        }

        public async Task<FileDetail> GetProofOfCompletionAsync( Guid oneUseFileKey )
        {
            var attachment = await _repo.GetAndConsumeProofOfCompletionByFileKeyAsync( oneUseFileKey );
            if ( attachment == null )
                return null;
            var filePath = await _fileProvider.DownloadFile( WarriorsGuildFileType.ProofOfCompletion, attachment.StorageKey );
            return new FileDetail( filePath.FilePathToServe, oneUseFileKey + attachment.FileExtension, filePath.ContentType );
        }

        public Task<IEnumerable<MinimalCrossDetail>> GetCrossesForRankStatus( Guid rankId, Guid requirementId, Guid userIdForStatuses )
        {
            return _repo.GetCrossesForRankStatusAsync( rankId, requirementId );
        }

        public Task<IEnumerable<MinimalGoalDetail>> GetAttachmentsForRankStatus( Guid requirementId, Guid userIdForStatuses )
        {
            return _repo.GetAttachmentsForRankStatusAsync( requirementId, userIdForStatuses );
        }

        public Task<IEnumerable<MinimalRingDetail>> GetRingsForRankStatus( Guid rankId, Guid requirementId, Guid userIdForStatuses )
        {
            return _repo.GetRingsForRankStatusAsync( rankId, requirementId, userIdForStatuses );
        }
    }
}
