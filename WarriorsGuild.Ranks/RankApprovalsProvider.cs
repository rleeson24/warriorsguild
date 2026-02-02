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

        private IGuildDbContext _dbContext { get; }
        private IRankRepository _repo { get; }

        public RankApprovalsProvider( IGuildDbContext dbContext, IRankRepository repo, IRankMapper rankMapper, IDateTimeProvider dateTimeProvider, IRanksProviderHelpers rpHelpers,
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
            _dbContext = dbContext;
            _repo = repo;
        }

        public async Task<IEnumerable<PendingApprovalDetail>> GetPendingApprovalsAsync( Guid userId )
        {
            var response = new List<PendingApprovalDetail>();
            var approvalRecords = await _dbContext.RankApprovals.Include( "Rank" ).Where( ra => ra.UserId == userId && !ra.RecalledByWarriorTs.HasValue && !ra.ReturnedTs.HasValue && !ra.ApprovedAt.HasValue ).ToArrayAsync();
            foreach ( var ar in approvalRecords )
            {
                var requirements = _dbContext.RankRequirements.Where( r => r.RankId == ar.RankId );
                var statusEntries = _dbContext.RankStatuses.Where( rs => rs.RankId == ar.RankId && rs.UserId == userId && !rs.GuardianCompleted.HasValue && !rs.RecalledByWarriorTs.HasValue && !rs.ReturnedTs.HasValue );
                var pendingRequirements = await requirements.Where( rr => statusEntries.Any( se => se.RankRequirementId == rr.Id && !se.GuardianCompleted.HasValue ) ).ToArrayAsync();

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
            var ar = await _dbContext.RankApprovals.Where( ra => ra.RankId == rankId && ra.UserId == userIdForStatuses ).OrderByDescending( r => r.CompletedAt ).ToArrayAsync();
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
            var approvalrecord = await _dbContext.RankApprovals.Where( ra => ra.UserId == userIdForStatuses && ra.RankId == rankId && !ra.RecalledByWarriorTs.HasValue && !ra.ReturnedTs.HasValue ).OrderByDescending( r => r.CompletedAt ).FirstOrDefaultAsync();
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
                    _dbContext.RankApprovals.Add( approvalEntry );
                    await _dbContext.SaveChangesAsync();
                    var rank = _dbContext.Set<Rank>().Find( rankId );
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
            var status = await _dbContext.RankApprovals.Include( s => s.Rank ).Where( c => c.Id == approvalRecordId && !c.RecalledByWarriorTs.HasValue && !c.ReturnedTs.HasValue && !c.ApprovedAt.HasValue ).FirstOrDefaultAsync();
            if ( status != null )
            {
                status.ReturnedTs = DateTime.UtcNow;
                status.ReturnedReason = userReason;
                status.ReturnedBy = userId;
                await _dbContext.SaveChangesAsync();
                //var rank = Database.Set<RankApproval>().Find( approvalRecordId ).Rank;
                await NotifyWarriorOfReturnedRequest( status, userId );
            }
        }

        public async Task RecallAsync( Guid approvalRecordId, Guid userId )
        {
            var status = await _dbContext.RankApprovals.Where( c => c.Id == approvalRecordId && !c.RecalledByWarriorTs.HasValue && !c.ReturnedTs.HasValue && !c.ApprovedAt.HasValue ).FirstOrDefaultAsync();
            if ( status != null )
            {
                status.RecalledByWarriorTs = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<RecordCompletionResponse> ApproveProgressAsync( Guid approvalRecordId, Guid currentUserId )
        {
            var response = new RecordCompletionResponse();

            var approvalRecord = await _dbContext.RankApprovals.FirstOrDefaultAsync( ra => ra.Id == approvalRecordId );
            if ( approvalRecord != null )
            {
                if ( !await _userProvider.UserIsRelatedToWarrior( currentUserId, approvalRecord.UserId ) )
                {
                    response.Success = false;
                    response.Error = "Invalid user was supplied for status update";
                }
                else if ( !approvalRecord.ApprovedAt.HasValue )
                {
                    var ss = await _dbContext.RankStatuses.Where( rs => rs.RankId == approvalRecord.RankId && rs.UserId == approvalRecord.UserId && !rs.GuardianCompleted.HasValue && !rs.RecalledByWarriorTs.HasValue && !rs.ReturnedTs.HasValue ).ToArrayAsync();
                    var relatedRingsAndCrossesAreApproved = await RpHelpers.AllAssociatedRingsAndCrossesAreCompletedAndApprovedAsync( approvalRecord.RankId, ss.Select( s => s.RankRequirementId ), approvalRecord.UserId );
                    if ( relatedRingsAndCrossesAreApproved )
                    {
                        var currentTime = _dateTimeProvider.GetCurrentDateTime();
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
            await _dbContext.SaveChangesAsync();
        }



        public async Task NotifyGuardiansOfRequestForPromotion( Rank rank, int totalCompleted, Guid userIdForStatuses )
        {
            try
            {
                var guardians = _dbContext.Set<ApplicationUser>().Where( u => u.ChildUsers.Any( cu => cu.Id == userIdForStatuses.ToString() ) ).Select( g => g.Email ).ToArray();
                var user = _dbContext.Set<ApplicationUser>().Find( userIdForStatuses.ToString() );
                var requesting = totalCompleted == 100 ? "Round Table" : "Promotion";
                var httpReq = _httpContextAccessor.HttpContext.Request;
                var htmlBody = $@"<h1>{user.FirstName} {user.LastName.Substring( 0, 1 )}. is requesting a {requesting}</h1><br /><br /><p>He has completed <span style='font-weight: bold'>{totalCompleted}%</span> of <span style='font-weight: bold'>{rank.Name}</span> and is requesting a <span style='font-weight: bold'>{requesting}</span></p>
                                <a style='color:#af111c' href='{string.Format( "{0}://{1}", httpReq.Scheme, httpReq.Host )}'>Click Here</a> to review.";
                await emailProvider.SendAsync( $"{user.FirstName} {user.LastName.Substring( 0, 1 )} requesting {requesting}", htmlBody, guardians, EmailView.Generic );
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
                var warriors = await _dbContext.Set<ApplicationUser>().Where( u => u.Id == status.UserId.ToString() ).Select( g => g.Email ).ToArrayAsync();
                var user = _dbContext.Set<ApplicationUser>().Find( guardianUserId.ToString() );
                var htmlBody = $"<p><span style='font-weight: bold'>{user.FirstName} {user.LastName.Substring( 0, 1 )}.</span> has returned your <span style='font-weight: bold'>{status.Rank.Name}</span> request for promotion</p><p>{status.ReturnedReason}</p>";
                await emailProvider.SendAsync( $"{user.FirstName} {user.LastName.Substring( 0, 1 )} returned request for promotion", htmlBody, warriors, EmailView.Generic );
            }
            catch ( Exception ex )
            {
                Logger.LogError( ex, "An error occurred sending return rank email", status, guardianUserId );
            }
        }
    }
}
