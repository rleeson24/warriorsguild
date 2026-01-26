using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.Data.Models.Rings;
using WarriorsGuild.Data.Models.Rings.Status;
using WarriorsGuild.DataAccess;
using WarriorsGuild.DataAccess.Models;
using WarriorsGuild.Email;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Models;
using WarriorsGuild.Providers;
using WarriorsGuild.Rings.Mappers;
using WarriorsGuild.Rings.Models.Status;
using WarriorsGuild.Rings.ViewModels;
using WarriorsGuild.Storage;
using WarriorsGuild.Storage.Models;

namespace WarriorsGuild.Rings
{
    public interface IRingsProvider
    {
        Task<Ring> GetAsync( Guid id );
        Task<Ring> GetPublicAsync();
        Task<Ring> DeleteRingAsync( Guid id );
        Task<FileDetail> GetImage( Guid id );
        Task<IEnumerable<Ring>> GetListAsync();
        Task<Ring> AddAsync( Ring ring );
        Task UpdateAsync( Guid id, Ring ring );
        Task UpdateRequirementsAsync( Guid id, IEnumerable<RingRequirement> requirements );
        Task<IEnumerable<Ring>> UpdateRingOrderAsync( IEnumerable<GoalIndexEntry> request );
        Task UploadImageAsync( Guid ringId, string fileExtension, string localFileName, string mediaType );

        IQueryable<RingStatus> GetRingStatus();
        Task<IEnumerable<RingStatus>> GetStatusesAsync( Guid ringId, Guid userId );
        Task<RingStatus> GetRingStatusAsync( int id );
        Task<RingStatus> PostRingStatusAsync( RingStatus ringStatus );
        Task<DeleteRingStatusResponse> DeleteRingStatusAsync( RingStatusUpdateModel ringToUpdate, Guid userIdForStatuses );
        Task<IEnumerable<PinnedRing>> GetActivePinnedRings( Guid userIdForStatuses );
        Task PinAsync( Guid id, Guid userIdForStatuses );
        Task UnpinAsync( Guid id, Guid userIdForStatuses );
        Task<IEnumerable<Ring>> GetCompletedAsync( Guid userIdForStatuses );
        Task<IEnumerable<RingRequirement>> GetRequirementsAsync( Guid id );
        Task<RingRequirement> GetRequirementAsync( Guid ringId, Guid requirementId );
        Task<IEnumerable<RingRequirementViewModel>> GetRequirementsWithStatusAsync( Guid ringId, Guid userId );
        Task<IEnumerable<PendingApprovalDetail>> GetPendingApprovalsAsync( Guid userId );
        Task<PendingApprovalDetail> GetPendingApprovalAsync( Guid userId, Guid ringId );
        Task<RecordRingCompletionResponse> RecordCompletionAsync( RingStatusUpdateModel ringToUpdate, Guid userIdForStatuses );
        Task<ApproveProgressResponse> ApproveProgressAsync( Guid approvalRecordId, Guid currentUserId );
        Task ReturnAsync( Guid approvalRecordId, Guid userId, string userReason );
        Task<RingApproval> SubmitForApprovalAsync( Guid guid, Guid ringId );
        Task<IEnumerable<UnassignedRingViewModel>> GetUnassignedPendingOrApproved( Guid userIdForStatuses );
        Task UploadGuideAsync( Guid crossId, string fileExtension, byte[] file, string mediaType );
        Task<FileDetail> GetGuideAsync( Guid id );
        Task<FileUploadResult> UploadProofOfCompletionAsync( string attachmentId, string fileExtension, byte[] file, string mediaType );
        Task<Guid> RecordProofOfCompletionDocumentAsync( Guid requirementId, Guid userId, string fileName, string extension );
        Task<FileDetail> GetProofOfCompletionAsync( Guid oneUseFileKey );
        Task<IEnumerable<MinimalGoalDetail>> GetAttachmentsForRingStatusAsync( Guid id, Guid userIdGuid );
        Task<ProofOfCompletionAttachment> GetProofOfCompletionAttachmentByIdAsync( Guid attachmentId );
        Task SaveProofOfCompletionOneUseFileKeyAsync( Guid attachmentId, Guid fileKey );
        Task<IEnumerable<PinnedRing>> GetPinnedRings( Guid userIdForStatuses );
    }
    public class RingsProvider : IRingsProvider
    {
        private readonly IEmailProvider emailProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IGuildDbContext _dbContext { get; }
        private IRingRepository Repo { get; }
        public IRingMapper _ringMapper { get; }
        public IHelpers Helpers { get; }
        public IUserProvider UserProvider { get; }
        public IBlobProvider FileProvider { get; }
        private ILogger<RingsProvider> Logger { get; }

        public RingsProvider( IGuildDbContext dbContext,IRingRepository db, IRingMapper ringMapper, IHelpers helpers, IUserProvider userProvider, IBlobProvider fileProvider, IEmailProvider emailProvider, IHttpContextAccessor httpContextAccessor, ILogger<RingsProvider> logger )
        {
            _dbContext = dbContext;
            Repo = db;
            _ringMapper = ringMapper;
            Helpers = helpers;
            UserProvider = userProvider;
            FileProvider = fileProvider;
            this.emailProvider = emailProvider;
            _httpContextAccessor = httpContextAccessor;
            Logger = logger;
        }

        #region Rings

        public async Task<IEnumerable<Ring>> GetListAsync()
        {
            var rings = await Repo.GetListAsync();
            return rings;
        }

        public async Task<Ring> GetPublicAsync()
        {
            return await Repo.GetPublicAsync();
        }

        public async Task<IEnumerable<Ring>> GetCompletedAsync( Guid userIdForStatuses )
        {
            return await Repo.GetCompletedAsync( userIdForStatuses );
        }

        public async Task<Ring> GetAsync( Guid id )
        {
            return await Repo.GetAsync( id );
        }

        public async Task<IEnumerable<RingRequirement>> GetRequirementsAsync( Guid id )
        {
            return await Repo.GetRequirementsAsync( id );
        }

        public async Task<RingRequirement> GetRequirementAsync( Guid ringId, Guid requirementId )
        {
            return await _dbContext.RingRequirements.SingleOrDefaultAsync( rr => rr.RingId == ringId && rr.Id == requirementId );
        }

        public async Task UpdateAsync( Guid id, Ring ring )
        {
            var existingParent = await Repo.GetAsync( id, true );

            if ( existingParent != null )
            {
                existingParent.Name = ring.Name;
                existingParent.Description = ring.Description;
                existingParent.Type = ring.Type;

                await Repo.UpdateAsync( id, existingParent );
            }
        }

        public async Task UpdateRequirementsAsync( Guid id, IEnumerable<RingRequirement> requirements )
        {
            var existingRequirements = await Repo.GetRequirementsAsync( id );
            await Repo.UpdateRequirementsAsync( id, requirements, existingRequirements );
        }

        public async Task<IEnumerable<PinnedRing>> GetActivePinnedRings( Guid userIdForStatuses )
        {
            return await Repo.GetActivePinnedRings( userIdForStatuses );
        }

        public async Task<IEnumerable<PinnedRing>> GetPinnedRings( Guid userIdForStatuses )
        {
            return await Repo.GetPinnedRings( userIdForStatuses );
        }

        public async Task<Ring> AddAsync( Ring ring )
        {
            var maxIndexInDb = await Repo.GetMaxIndexAsync();
            ring.Index = maxIndexInDb + 1;
            return await Repo.AddAsync( ring );
        }

        public async Task<Ring> DeleteRingAsync( Guid id )
        {
            return await Repo.DeleteRingAsync( id );
        }

        public async Task<IEnumerable<Ring>> UpdateRingOrderAsync( IEnumerable<GoalIndexEntry> request )
        {
            return await Repo.UpdateRingOrderAsync( request );
        }

        #region "File Operations"
        public async Task<FileDetail> GetImage( Guid ringId )
        {
            var fileResult = await FileProvider.DownloadFile( WarriorsGuildFileType.RingImage, ringId.ToString() );
            return new FileDetail( fileResult.FilePathToServe, null, fileResult.ContentType );
        }

        public async Task UploadImageAsync( Guid ringId, string fileExtension, string localFileName, string mediaType )
        {
            await Repo.SetHasImageAsync( ringId, fileExtension );
        }

        public async Task UploadGuideAsync( Guid ringId, string fileExtension, byte[] file, string mediaType )
        {
            var uploadedFileResult = await FileProvider.UploadFileAsync( WarriorsGuildFileType.Guide, file, ringId.ToString(), mediaType );
            await Repo.SetHasGuideAsync( ringId, fileExtension );
        }

        public async Task<FileDetail> GetGuideAsync( Guid ringId )
        {
            var fileResult = await FileProvider.DownloadFile( WarriorsGuildFileType.Guide, ringId.ToString() );
            var ring = await _dbContext.Rings.SingleOrDefaultAsync( r => r.Id == ringId && r.GuideUploaded.HasValue );
            return new FileDetail( fileResult.FilePathToServe, ring?.Name + ring?.GuideFileExtension, fileResult.ContentType );
        }

        public async Task<FileUploadResult> UploadProofOfCompletionAsync( string attachmentId, string fileExtension, byte[] file, string mediaType )
        {
            return await FileProvider.UploadFileAsync( WarriorsGuildFileType.ProofOfCompletion, file, attachmentId.ToString(), mediaType );
        }

        public async Task<FileDetail> GetProofOfCompletionAsync( Guid oneUseFileKey )
        {
            var attachment = await _dbContext.ProofOfCompletionAttachments
                                            .Where( poca => _dbContext.SingleUseFileDownloadKey.Any( a => a.Key == oneUseFileKey
                                                                                && a.AttachmentId == poca.Id ) ).SingleOrDefaultAsync();

            var filePath = await FileProvider.DownloadFile( WarriorsGuildFileType.ProofOfCompletion, attachment.StorageKey );
            var keyObject = await _dbContext.SingleUseFileDownloadKey.FindAsync( oneUseFileKey );
            _dbContext.SingleUseFileDownloadKey.Remove( keyObject );
            await _dbContext.SaveChangesAsync();
            return new FileDetail( filePath.FilePathToServe, oneUseFileKey + attachment.FileExtension, filePath.ContentType );
        }
        #endregion

        public async Task PinAsync( Guid id, Guid userIdForStatuses )
        {
            var ring = await Repo.GetAsync( id );
            await Repo.PinAsync( new PinnedRing() { Ring = ring, UserId = userIdForStatuses } );
        }

        public async Task UnpinAsync( Guid ringId, Guid userIdForStatuses )
        {
            var pinnedRing = await Repo.GetPinnedRing( userIdForStatuses, ringId );
            if ( pinnedRing != null )
            {
                await Repo.UnpinAsync( pinnedRing );
            }
        }
        #endregion

        #region Status
        public IQueryable<RingStatus> GetRingStatus()
        {
            return Repo.RingStatuses();
        }

        public async Task<IEnumerable<RingStatus>> GetStatusesAsync( Guid ringId, Guid userId )
        {
            return await Repo.GetStatusesAsync( ringId, userId );
        }

        public async Task<RingStatus> GetRingStatusAsync( int id )
        {
            return await Repo.GetRingStatusAsync( id );
        }

        public async Task<IEnumerable<UnassignedRingViewModel>> GetUnassignedPendingOrApproved( Guid userIdForStatuses )
        {
            var rings = await _dbContext.RingApprovals.Include( ra => ra.Ring )
                                                    .Where( ra => ra.UserId == userIdForStatuses && ra.ApprovedAt.HasValue )
                                                    .Where( ra => !_dbContext.RankStatusRings.Any( rsr => rsr.RingId == ra.RingId && rsr.UserId == userIdForStatuses ) )
                                                    .ToArrayAsync();
            return rings.Select( ra => new UnassignedRingViewModel()
            {
                Name = ra.Ring.Name,
                RingId = ra.RingId,
                Type = ra.Ring.Type.ToString(),
                ImageUploaded = ra.Ring.ImageUploaded,
                ImageExtension = ra.Ring.ImageExtension,
                HasImage = ra.Ring.ImageUploaded != null
            } ).ToArray();
        }

        public async Task<RecordRingCompletionResponse> RecordCompletionAsync( RingStatusUpdateModel ringForStatus, Guid userIdForStatuses )
        {
            var response = new RecordRingCompletionResponse();

            var ss = await Repo.GetRequirementStatusAsync( ringForStatus.RingId, ringForStatus.RingRequirementId, userIdForStatuses );

            if ( ss == null )
            {
                var approvalrecord = await _dbContext.RingApprovals.Where( ra => ra.UserId == userIdForStatuses && ra.RingId == ringForStatus.RingId && !ra.RecalledByWarriorTs.HasValue && !ra.ReturnedTs.HasValue ).OrderByDescending( r => r.ApprovedAt ).FirstOrDefaultAsync();
                if ( approvalrecord == null || approvalrecord.ApprovedAt.HasValue )
                {
                    var statusToSave = _ringMapper.CreateRingStatus( ringForStatus.RingId, ringForStatus.RingRequirementId, warriorCompletedTs: Helpers.GetCurrentDateTime(), null, userIdForStatuses );
                    _dbContext.RingStatuses.Add( statusToSave );
                    await _dbContext.SaveChangesAsync();
                    response.Success = true;
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

            return response;
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

        public async Task<IEnumerable<PendingApprovalDetail>> GetPendingApprovalsAsync( Guid userId )
        {
            var response = new List<PendingApprovalDetail>();
            var approvalRecords = await _dbContext.RingApprovals.Include( "Ring" ).Where( ra => ra.UserId == userId && !ra.RecalledByWarriorTs.HasValue && !ra.ReturnedTs.HasValue && !ra.ApprovedAt.HasValue ).ToArrayAsync();
            foreach ( var ar in approvalRecords )
            {
                var requirements = _dbContext.RingRequirements.Where( r => r.RingId == ar.RingId );
                var statusEntries = _dbContext.RingStatuses.Where( rs => rs.RingId == ar.RingId && rs.UserId == userId && !rs.GuardianCompleted.HasValue && !rs.RecalledByWarriorTs.HasValue && !rs.ReturnedTs.HasValue );
                var pendingRequirements = await requirements.Where( rr => statusEntries.Any( se => se.RingRequirementId == rr.Id ) ).ToArrayAsync();
                response.Add( new PendingApprovalDetail()
                {
                    ApprovalRecordId = ar.Id,
                    RingId = ar.RingId,
                    RingName = ar.Ring.Name,
                    RingImageUploaded = ar.Ring.ImageUploaded,
                    WarriorCompleted = ar.CompletedAt,
                    GuardianConfirmed = ar.ApprovedAt,
                    //PercentComplete = ar.PercentComplete,
                    ImageExtension = ar.Ring.ImageExtension,
                    UnconfirmedRequirements = pendingRequirements
                } );
            }
            return response;
        }

        public async Task<PendingApprovalDetail> GetPendingApprovalAsync( Guid userId, Guid ringId )
        {
            PendingApprovalDetail response = null;
            var approvalRecords = await _dbContext.RingApprovals.Include( "Ring" ).Where( ra => ra.UserId == userId && ra.RingId == ringId && !ra.RecalledByWarriorTs.HasValue && !ra.ReturnedTs.HasValue ).FirstOrDefaultAsync();
            if ( approvalRecords != null )
            {
                response = new PendingApprovalDetail()
                {
                    ApprovalRecordId = approvalRecords.Id,
                    RingId = approvalRecords.RingId,
                    RingName = approvalRecords.Ring.Name,
                    RingImageUploaded = approvalRecords.Ring.ImageUploaded,
                    WarriorCompleted = approvalRecords.CompletedAt,
                    GuardianConfirmed = approvalRecords.ApprovedAt
                };
            }
            return response;
        }

        public async Task<RingApproval> SubmitForApprovalAsync( Guid userId, Guid ringId )
        {
            var status = await _dbContext.RingApprovals.Where( c => c.RingId == ringId && c.UserId == userId && !c.RecalledByWarriorTs.HasValue && !c.ReturnedTs.HasValue && !c.ApprovedAt.HasValue ).FirstOrDefaultAsync();
            if ( status == null )
            {
                var newRingApproval = new RingApproval();
                newRingApproval.RingId = ringId;
                newRingApproval.UserId = userId;
                newRingApproval.CompletedAt = DateTime.UtcNow;
                _dbContext.RingApprovals.Add( newRingApproval );
                await _dbContext.SaveChangesAsync();
                await NotifyGuardiansOfRequestForPromotion( ringId, userId );
                return newRingApproval;
            }
            else
            {
                return null;
            }
        }

        public async Task ReturnAsync( Guid approvalRecordId, Guid userId, string userReason )
        {
            var status = await _dbContext.RingApprovals.Where( c => c.Id == approvalRecordId && c.UserId == userId && !c.RecalledByWarriorTs.HasValue && !c.ReturnedTs.HasValue && !c.ApprovedAt.HasValue ).FirstOrDefaultAsync();
            if ( status != null )
            {
                status.ReturnedTs = DateTime.UtcNow;
                status.ReturnedReason = userReason;
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task NotifyWarriorOfReturnedRequest( RingApproval status, Guid guardianUserId )
        {
            try
            {
                var warriors = await _dbContext.Set<ApplicationUser>().Where( u => u.Id == status.UserId.ToString() ).Select( g => g.Email ).ToArrayAsync();
                var user = _dbContext.Set<ApplicationUser>().Find( guardianUserId.ToString() );
                var htmlBody = $"<p><span style='font-weight: bold'>{user.FirstName} {user.LastName.Substring( 0, 1 )}.</span> has returned your <span style='font-weight: bold'>{status.Ring.Name}</span> request for completion</p><p>{status.ReturnedReason}</p>";
                await emailProvider.SendAsync( $"{user.FirstName} {user.LastName.Substring( 0, 1 )} returned request for ring completion", htmlBody, warriors, EmailView.Generic );
            }
            catch ( Exception ex )
            {
                Logger.LogError( ex, "An error occurred sending return ring email", status, guardianUserId );
            }
        }

        public async Task<ApproveProgressResponse> ApproveProgressAsync( Guid approvalRecordId, Guid currentUserId )
        {
            var response = new ApproveProgressResponse();

            var approvalRecord = await _dbContext.RingApprovals.FirstOrDefaultAsync( ra => ra.Id == approvalRecordId );
            if ( approvalRecord != null )
            {
                if ( !await UserProvider.UserIsRelatedToWarrior( currentUserId, approvalRecord.UserId ) )
                {
                    response.Success = false;
                    response.Error = "Invalid user was supplied for status update";
                }
                else if ( !approvalRecord.ApprovedAt.HasValue )
                {
                    var ss = await _dbContext.RingStatuses.Where( rs => rs.RingId == approvalRecord.RingId && rs.UserId == approvalRecord.UserId && !rs.GuardianCompleted.HasValue && !rs.RecalledByWarriorTs.HasValue && !rs.ReturnedTs.HasValue ).ToArrayAsync();
                    var currentTime = Helpers.GetCurrentDateTime();
                    foreach ( var s in ss )
                    {
                        s.GuardianCompleted = currentTime;
                    }
                    approvalRecord.ApprovedAt = currentTime;
                    await _dbContext.SaveChangesAsync();
                    response.Success = true;
                }
                else
                {
                    response.Success = false;
                    response.Error = "This ring has already been approved";
                }
            }
            else
            {
                response.Success = false;
                response.Error = "The approval record was not found";
            }
            return response;
        }

        public async Task<RingStatus> PostRingStatusAsync( RingStatus ringStatus )
        {
            return await Repo.PostRingStatusAsync( ringStatus );
        }

        public async Task<DeleteRingStatusResponse> DeleteRingStatusAsync( RingStatusUpdateModel ringFOrStatus, Guid userIdForStatuses )
        {
            var response = new DeleteRingStatusResponse();
            var ringStatus = await _dbContext.RingStatuses.SingleOrDefaultAsync( rs => rs.RingId == ringFOrStatus.RingId && rs.RingRequirementId == ringFOrStatus.RingRequirementId && rs.UserId == userIdForStatuses && !rs.RecalledByWarriorTs.HasValue && !rs.ReturnedTs.HasValue );
            if ( ringStatus == null )
            {
                response.Error = "The given requirement is not marked as complete";
            }
            else if ( ringStatus.GuardianCompleted.HasValue )
            {
                response.Error = "This requirement completion cannot be undone because it has already been approved";
            }
            else
            {
                ringStatus.RecalledByWarriorTs = DateTime.UtcNow;
                var pocToDelete = await _dbContext.ProofOfCompletionAttachments.Where( a => a.RequirementId == ringFOrStatus.RingRequirementId && a.UserId == userIdForStatuses ).ToArrayAsync();
                foreach ( var p in pocToDelete )
                {
                    _dbContext.SetDeleted( p );
                }
                await _dbContext.SaveChangesAsync();
                response.Success = true;
            }
            return response;
        }

        public async Task<IEnumerable<MinimalGoalDetail>> GetAttachmentsForRingStatusAsync( Guid requirementId, Guid userIdForStatuses )
        {
            return await Repo.GetAttachmentsForRingStatus( requirementId, userIdForStatuses );
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

        public async Task<IEnumerable<RingRequirementViewModel>> GetRequirementsWithStatusAsync( Guid ringId, Guid userId )
        {
            var requirements = await Repo.GetRequirementsAsync( ringId );
            var statuses = await Repo.GetStatusesAsync( ringId, userId );
            if ( requirements == null )
            {
                return null;
            }

            var result = new List<RingRequirementViewModel>();
            foreach ( var requirement in requirements )
            {
                var status = statuses?.FirstOrDefault( s => s.RingRequirementId == requirement.Id );
                IEnumerable<MinimalGoalDetail> attachments = new MinimalGoalDetail[ 0 ];
                if ( requirement.RequireAttachment && status != null )
                {
                    attachments = await Repo.GetAttachmentsForRingStatus( requirement.Id, userId );
                }
                result.Add( _ringMapper.CreateRequirementViewModel( requirement, status, attachments ) );
            }
            return result;
        }

        #endregion

        public async Task NotifyGuardiansOfRequestForPromotion( Guid ringId, Guid userId )
        {
            try
            {
                var ring = _dbContext.Set<Ring>().Find( ringId );
                var guardians = _dbContext.Set<ApplicationUser>().Where( u => u.ChildUsers.Any( cu => cu.Id == userId.ToString() ) ).Select( g => g.Email ).ToArray();
                var user = _dbContext.Set<ApplicationUser>().Find( userId.ToString() );
                var httpReq = _httpContextAccessor.HttpContext.Request;
                var htmlBody = $@"<h1>{user.FirstName} {user.LastName.Substring( 0, 1 )} has completed a Warrior Ring!</h1><br /><br /><p>He has completed 100% of the <span style='font-weight: bold'>{ring.Name}</span> ring and is awaiting your confirmation.</p>
                                <p><a style='color:#af111c' href='{string.Format( "{0}://{1}", httpReq.Scheme, httpReq.Host )}'>Click Here</a> to review.</p>";
                await emailProvider.SendAsync( $"{user.FirstName} {user.LastName.Substring( 0, 1 )} requesting your confirmation", htmlBody, guardians, EmailView.Generic );
            }
            catch ( Exception ex )
            {
                Logger.LogError( ex, "An error occurred sending request for review email", ringId, userId );
            }
        }
    }
}