using Microsoft.AspNetCore.Http;
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
        private IHelpers Helpers { get; }

        private IGuildDbContext _dbContext { get; }
        private IRankRepository _repo { get; }

        public RankStatusProvider( IGuildDbContext dbContext, IRankRepository repo, IRankMapper rankMapper, IHelpers helpers, IRanksProviderHelpers rpHelpers,
                            IBlobProvider fileProvider )
        {
            _dbContext = dbContext;
            _rankMapper = rankMapper;
            _fileProvider = fileProvider;
            RpHelpers = rpHelpers;
            Helpers = helpers;
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
                    var approvalrecord = await _dbContext.RankApprovals.Where( ra => ra.UserId == userIdForStatuses && ra.RankId == rankForStatus.RankId && !ra.RecalledByWarriorTs.HasValue && !ra.ReturnedTs.HasValue ).OrderByDescending( r => r.CompletedAt ).FirstOrDefaultAsync();
                    var percentApproved = approvalrecord?.PercentComplete ?? 0;
                    if ( approvalrecord == null || approvalrecord.ApprovedAt.HasValue )
                    {
                        int totalCompleted = await RpHelpers.GetTotalCompletedPercent( rankForStatus.RankId, userIdForStatuses );
                        if ( totalCompleted - percentApproved < 33 )
                        {
                            var statusToSave = _rankMapper.CreateRankStatus( rankForStatus.RankId, rankForStatus.RankRequirementId, warriorCompletedTs: Helpers.GetCurrentDateTime(), null, userIdForStatuses );
                            response.Status = statusToSave;
                            _dbContext.RankStatuses.Add( statusToSave );

                            await _dbContext.SaveChangesAsync();
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

        public async Task<RecordCompletionResponse> DeleteRankStatusAsync( RankStatusUpdateModel rankForStatus, Guid userIdForStatuses )
        {
            var response = new RecordCompletionResponse();
            var rankStatus = await _dbContext.RankStatuses.FirstOrDefaultAsync( rs => rs.RankId == rankForStatus.RankId && rs.RankRequirementId == rankForStatus.RankRequirementId && rs.UserId == userIdForStatuses && !rs.RecalledByWarriorTs.HasValue && !rs.ReturnedTs.HasValue );
            if ( rankStatus == null )
            {
                response.Error = "The given requirement is not marked as complete";
            }
            else if ( rankStatus.GuardianCompleted.HasValue )
            {
                response.Error = "This requirement completion cannot be undone because it has already been approved";
            }
            else
            {
                rankStatus.RecalledByWarriorTs = DateTime.UtcNow;
                var pocToDelete = await _dbContext.ProofOfCompletionAttachments.Where( a => a.RequirementId == rankForStatus.RankRequirementId && a.UserId == userIdForStatuses ).ToArrayAsync();
                foreach ( var p in pocToDelete )
                {
                    _dbContext.SetDeleted( p );
                }
                var crossesToDelete = await _dbContext.RankStatusCrosses.Where( c => c.RankId == rankForStatus.RankId
                                                                            && c.RankRequirementId == rankForStatus.RankRequirementId
                                                                            && c.UserId == userIdForStatuses ).ToArrayAsync();
                foreach ( var c in crossesToDelete )
                {
                    _dbContext.RankStatusCrosses.Remove( c );
                }
                var ringsToDelete = await _dbContext.RankStatusRings.Where( c => c.RankId == rankForStatus.RankId
                                                                            && c.RankRequirementId == rankForStatus.RankRequirementId
                                                                            && c.UserId == userIdForStatuses ).ToArrayAsync();
                foreach ( var c in ringsToDelete )
                {
                    _dbContext.RankStatusRings.Remove( c );
                }
                await _dbContext.SaveChangesAsync();
                response.Success = true;
            }
            return response;
        }

        public async Task<RankStatus> PostRankStatusAsync( RankStatus rankStatus )
        {
            _repo.AddStatusEntry( rankStatus );
            await _dbContext.SaveChangesAsync();
            return rankStatus;
        }

        public async Task<FileUploadResult> UploadProofOfCompletionAsync( string attachmentId, string fileExtension, byte[] file, string mediaType )
        {
            return await _fileProvider.UploadFileAsync( WarriorsGuildFileType.ProofOfCompletion, file, attachmentId.ToString(), mediaType );
        }

        public async Task<ProofOfCompletionAttachment> GetProofOfCompletionAttachmentByIdAsync( Guid attachmentId )
        {
            return await _dbContext.ProofOfCompletionAttachments.SingleOrDefaultAsync( a => a.Id == attachmentId );
        }

        public async Task SaveProofOfCompletionOneUseFileKeyAsync( Guid attachmentId, Guid fileKey )
        {
            _dbContext.SingleUseFileDownloadKey.Add( new SingleUseFileDownloadKey() { AttachmentId = attachmentId, Key = fileKey } );
            await _dbContext.SaveChangesAsync();
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
            _dbContext.ProofOfCompletionAttachments.Add( entity );
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<FileDetail> GetProofOfCompletionAsync( Guid oneUseFileKey )
        {
            var attachment = await _dbContext.ProofOfCompletionAttachments
                                    .Where( poca => _dbContext.SingleUseFileDownloadKey.Any( a => a.Key == oneUseFileKey
                                                                                                && a.AttachmentId == poca.Id ) ).SingleOrDefaultAsync();

            var filePath = await _fileProvider.DownloadFile( WarriorsGuildFileType.ProofOfCompletion, attachment.StorageKey );
            var keyObject = await _dbContext.SingleUseFileDownloadKey.FindAsync( oneUseFileKey );
            _dbContext.SingleUseFileDownloadKey.Remove( keyObject );
            await _dbContext.SaveChangesAsync();
            return new FileDetail( filePath.FilePathToServe, oneUseFileKey + attachment.FileExtension, filePath.ContentType );
        }

        public async Task<IEnumerable<MinimalCrossDetail>> GetCrossesForRankStatus( Guid rankId, Guid requirementId, Guid userIdForStatuses )
        {
            return await _dbContext.RankCrosses.Include( r => r.Cross ).Where( r => r.RankId == rankId && r.RankRequirementId == requirementId )
                                                .Select( r => new MinimalCrossDetail()
                                                {
                                                    Id = r.CrossId,
                                                    Name = r.Cross.Name,
                                                    ImageUploaded = r.Cross.ImageUploaded,
                                                    ImageExtension = r.Cross.ImageExtension
                                                } ).ToArrayAsync();
        }

        public async Task<IEnumerable<MinimalGoalDetail>> GetAttachmentsForRankStatus( Guid requirementId, Guid userIdForStatuses )
        {
            return await _dbContext.ProofOfCompletionAttachments.Where( r => r.RequirementId == requirementId && r.UserId == userIdForStatuses )
                                                .Select( r => new MinimalAttachmentDetail() { Id = r.Id } ).ToArrayAsync();
        }

        public async Task<IEnumerable<MinimalRingDetail>> GetRingsForRankStatus( Guid rankId, Guid requirementId, Guid userIdForStatuses )
        {
            return await _dbContext.RankStatusRings.Include( r => r.Ring )
                                                .Where( r => r.RankId == rankId && r.RankRequirementId == requirementId && r.UserId == userIdForStatuses )
                                                .Select( r => new MinimalRingDetail()
                                                {
                                                    Id = r.RingId,
                                                    Name = r.Ring.Name,
                                                    ImageUploaded = r.Ring.ImageUploaded,
                                                    ImageExtension = r.Ring.ImageExtension
                                                } ).ToArrayAsync();
        }
    }
}
