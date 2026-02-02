using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.Data.Models.Ranks.Status;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Email;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Providers;
using WarriorsGuild.Ranks.Mappers;
using WarriorsGuild.Ranks.ViewModels;
using WarriorsGuild.Storage;

namespace WarriorsGuild.Ranks
{
    public class RankApprovalsProvider : IRankApprovalsProvider
    {
        private readonly IEmailProvider emailProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRankStatusProvider rankStatusProvider;
        private IRanksProviderHelpers RpHelpers { get; }

        private IRankMapper _rankMapper { get; }
        private IDateTimeProvider _dateTimeProvider { get; }
        private ILogger<RanksProvider> Logger { get; }
        private IUserProvider _userProvider { get; }

        private IUnitOfWork _uow { get; }
        private IRankRepository _repo { get; }
        private IAccountRepository _accountRepo { get; }

        public RankApprovalsProvider( IUnitOfWork uow, IRankRepository repo, IAccountRepository accountRepo, IRankMapper rankMapper, IDateTimeProvider dateTimeProvider, IRanksProviderHelpers rpHelpers,
                            IUserProvider userProvider, IRankStatusProvider rankStatusProvider, ILogger<RanksProvider> logger, IEmailProvider emailProvider,
                            IHttpContextAccessor httpContextAccessor )
        {
            RpHelpers = rpHelpers;
            this.emailProvider = emailProvider;
            _httpContextAccessor = httpContextAccessor;
            this.rankStatusProvider = rankStatusProvider;
            _rankMapper = rankMapper;
            _dateTimeProvider = dateTimeProvider;
            Logger = logger;
            _userProvider = userProvider;
            _uow = uow;
            _repo = repo;
            _accountRepo = accountRepo;
        }

        public async Task<IEnumerable<PendingApprovalDetail>> GetPendingApprovalsAsync( Guid userId )
        {
            var response = new List<PendingApprovalDetail>();
            var approvalRecords = await _repo.GetPendingRankApprovalsWithRankAsync( userId );
            foreach ( var ar in approvalRecords )
            {
                var (pendingRequirements, statusEntries) = await _repo.GetPendingRequirementsWithStatusForApprovalAsync( ar.RankId, userId );

                var unconfirmedRequirements = new List<RankRequirementViewModel>();
                foreach ( var requirement in pendingRequirements )
                {
                    var relatedStatusEntry = statusEntries.Single( s => s.RankRequirementId == requirement.Id );
                    IEnumerable<MinimalRingDetail> rings = new MinimalRingDetail[ 0 ];
                    IEnumerable<MinimalCrossDetail> crosses = new MinimalCrossDetail[ 0 ];
                    IEnumerable<MinimalGoalDetail> attachments = new MinimalGoalDetail[ 0 ];
                    if ( requirement.RequireRing )
                    {
                        rings = await rankStatusProvider.GetRingsForRankStatus( ar.RankId, requirement.Id, userId );
                    }
                    if ( requirement.RequireCross )
                    {
                        crosses = await rankStatusProvider.GetCrossesForRankStatus( ar.RankId, requirement.Id, userId );
                    }
                    if ( requirement.RequireAttachment )
                    {
                        attachments = await rankStatusProvider.GetAttachmentsForRankStatus( requirement.Id, userId );
                    }
                    unconfirmedRequirements.Add( _rankMapper.CreateRequirementViewModel( requirement, relatedStatusEntry.WarriorCompleted, null, rings, crosses, attachments ) );
                }

                response.Add( new PendingApprovalDetail()
                {
                    ApprovalRecordId = ar.Id,
                    RankId = ar.RankId,
                    RankName = ar.Rank.Name,
                    UserId = ar.UserId,
                    RankImageUploaded = ar.Rank.ImageUploaded,
                    PercentComplete = ar.PercentComplete,
                    ImageExtension = ar.Rank.ImageExtension,
                    UnconfirmedRequirements = unconfirmedRequirements
                } );
            }
            return response;
        }

        public async Task<IEnumerable<PendingApprovalDetail>> AllApprovalsForRank( Guid rankId, Guid userIdForStatuses )
        {
            var ar = await _repo.GetApprovalsForRankAsync( rankId, userIdForStatuses );
            IEnumerable<PendingApprovalDetail> response = null;
            response = ar.Select( a => new PendingApprovalDetail()
            {
                ApprovalRecordId = a.Id,
                RankId = a.RankId,
                UserId = a.UserId,
                PercentComplete = a.PercentComplete,
                GuardianApprovedTs = a.ApprovedAt,
                WarriorCompletedTs = a.CompletedAt,
                ReturnedTs = a.RecalledByWarriorTs ?? a.ReturnedTs,
                ReturnReason = a.ReturnedReason
            } ).ToArray();
            return response;
        }

        public async Task<RecordCompletionResponse> SubmitForApprovalAsync( Guid rankId, Guid userIdForStatuses )
        {
            var response = new RecordCompletionResponse();
            var approvalrecord = await _repo.GetLatestPendingOrApprovedRankApprovalAsync( rankId, userIdForStatuses );
            var percentApproved = approvalrecord?.PercentComplete ?? 0;
            if ( approvalrecord == null || approvalrecord.ApprovedAt.HasValue )
            {
                var totalCompleted = await RpHelpers.GetTotalCompletedPercent( rankId, userIdForStatuses );
                var lastApprovedMilestone = 0;
                if ( percentApproved == 100 ) lastApprovedMilestone = 100;
                else if ( percentApproved >= 66 ) lastApprovedMilestone = 66;
                else if ( percentApproved >= 33 ) lastApprovedMilestone = 33;
                if ( totalCompleted == 100 || totalCompleted - lastApprovedMilestone >= 33 )
                {
                    var approvalEntry = _rankMapper.CreateRankApproval( rankId: rankId, userIdForStatuses, totalCompleted, _dateTimeProvider.GetCurrentDateTime() );
                    _repo.AddApprovalEntry( approvalEntry );
                    await _uow.SaveChangesAsync();
                    var rank = _repo.Get( rankId );
                    await NotifyGuardiansOfRequestForPromotion( rank, totalCompleted, userIdForStatuses );
                    response.Success = true;
                    response.ApprovalRecordId = approvalEntry.Id;
                }
            }
            else
            {
                response.Success = false;
                response.Error = "There is currently a pending approval record.  Your Guardian must approve it before you can continue.";
            }
            return response;
        }

        public async Task ReturnAsync( Guid approvalRecordId, Guid userId, string userReason )
        {
            var status = await _repo.GetRankApprovalByIdWithRankAsync( approvalRecordId );
            if ( status != null )
            {
                status.ReturnedTs = DateTime.UtcNow;
                status.ReturnedReason = userReason;
                status.ReturnedBy = userId;
                await _uow.SaveChangesAsync();
                await NotifyWarriorOfReturnedRequest( status, userId );
            }
        }

        public async Task RecallAsync( Guid approvalRecordId, Guid userId )
        {
            var status = await _repo.GetRankApprovalByIdAsync( approvalRecordId );
            if ( status != null && !status.RecalledByWarriorTs.HasValue && !status.ReturnedTs.HasValue && !status.ApprovedAt.HasValue )
            {
                status.RecalledByWarriorTs = DateTime.UtcNow;
                await _uow.SaveChangesAsync();
            }
        }

        public async Task<RecordCompletionResponse> ApproveProgressAsync( Guid approvalRecordId, Guid currentUserId )
        {
            var response = new RecordCompletionResponse();

            var approvalRecord = await _repo.GetRankApprovalByIdAsync( approvalRecordId );
            if ( approvalRecord != null )
            {
                if ( !await _userProvider.UserIsRelatedToWarrior( currentUserId, approvalRecord.UserId ) )
                {
                    response.Success = false;
                    response.Error = "Invalid user was supplied for status update";
                }
                else if ( !approvalRecord.ApprovedAt.HasValue )
                {
                    var ss = await _repo.GetUnapprovedRankStatusesAsync( approvalRecord.RankId, approvalRecord.UserId );
                    var relatedRingsAndCrossesAreApproved = await RpHelpers.AllAssociatedRingsAndCrossesAreCompletedAndApprovedAsync( approvalRecord.RankId, ss.Select( s => s.RankRequirementId ), approvalRecord.UserId );
                    if ( relatedRingsAndCrossesAreApproved )
                    {
                        var currentTime = _dateTimeProvider.GetCurrentDateTime();
                        foreach ( var s in ss )
                        {
                            s.GuardianCompleted = currentTime;
                        }
                        approvalRecord.ApprovedAt = currentTime;
                        await _uow.SaveChangesAsync();
                        response.Success = true;
                    }
                    else
                    {
                        response.Error = "All Crosses and Rings must be approved before you can approve the promotion";
                        response.Success = false;
                    }
                }
                else
                {
                    response.Success = false;
                    response.Error = "This stage has already been approved";
                }
            }
            else
            {
                response.Success = false;
                response.Error = "The approval record was not found";
            }
            return response;
        }

        public async Task MarkGuardianReviewedAsync( RankStatus existingChild )
        {
            existingChild.GuardianCompleted = DateTime.UtcNow;
            _repo.Update( existingChild );
            await _uow.SaveChangesAsync();
        }



        public async Task NotifyGuardiansOfRequestForPromotion( Rank rank, int totalCompleted, Guid userIdForStatuses )
        {
            try
            {
                var guardians = await _accountRepo.GetGuardianEmailsForWarriorAsync( userIdForStatuses );
                var user = await _accountRepo.GetUserByIdAsync( userIdForStatuses );
                if ( guardians.Length == 0 || user == null )
                    return;
                var requesting = totalCompleted == 100 ? "Round Table" : "Promotion";
                var httpReq = _httpContextAccessor.HttpContext.Request;
                var displayName = $"{user.FirstName} {user.LastName?.Substring( 0, 1 ) ?? ""}.";
                var htmlBody = $@"<h1>{displayName} is requesting a {requesting}</h1><br /><br /><p>He has completed <span style='font-weight: bold'>{totalCompleted}%</span> of <span style='font-weight: bold'>{rank.Name}</span> and is requesting a <span style='font-weight: bold'>{requesting}</span></p>
                                <a style='color:#af111c' href='{string.Format( "{0}://{1}", httpReq.Scheme, httpReq.Host )}'>Click Here</a> to review.";
                await emailProvider.SendAsync( $"{displayName} requesting {requesting}", htmlBody, guardians, EmailView.Generic );
            }
            catch ( Exception ex )
            {
                Logger.LogError( ex, "An error occurred sending request for promotion email", rank, userIdForStatuses );
            }
        }

        private async Task NotifyWarriorOfReturnedRequest( RankApproval status, Guid guardianUserId )
        {
            try
            {
                var warriorUser = await _accountRepo.GetUserByIdAsync( status.UserId );
                var warriors = warriorUser != null ? new[] { warriorUser.Email } : Array.Empty<string>();
                var user = await _accountRepo.GetUserByIdAsync( guardianUserId );
                var htmlBody = user != null
                    ? $"<p><span style='font-weight: bold'>{user.FirstName} {user.LastName.Substring( 0, 1 )}.</span> has returned your <span style='font-weight: bold'>{status.Rank.Name}</span> request for promotion</p><p>{status.ReturnedReason}</p>"
                    : $"<p>Your <span style='font-weight: bold'>{status.Rank.Name}</span> request for promotion has been returned</p><p>{status.ReturnedReason}</p>";
                await emailProvider.SendAsync( user != null ? $"{user.FirstName} {user.LastName.Substring( 0, 1 )} returned request for promotion" : "Request for promotion returned", htmlBody, warriors, EmailView.Generic );
            }
            catch ( Exception ex )
            {
                Logger.LogError( ex, "An error occurred sending return rank email", status, guardianUserId );
            }
        }
    }
}
