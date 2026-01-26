using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Ranks.Status;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Helpers.Filters;
using WarriorsGuild.Helpers.Utilities.Files;
using WarriorsGuild.Providers;
using WarriorsGuild.Ranks;
using WarriorsGuild.Ranks.Mappers;
using WarriorsGuild.Ranks.Models.Status;
using WarriorsGuild.Ranks.ViewModels;
using static IdentityServer4.IdentityServerConstants;

namespace WarriorsGuild.Areas.Ranks.Controllers
{
    [ApiController]
    [Route( "api/[controller]" )]
    [Authorize( LocalApi.PolicyName )]
    [IgnoreAntiforgeryToken]
    public class RankStatusController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private ILogger<RankStatusController> logger;
        private readonly IWebHostEnvironment environment;
        private readonly long _fileSizeLimit;

        public UserManager<ApplicationUser> UserManager
        {
            get { return _userManager; }
        }

        public IRanksProvider RanksProvider { get; }
        public IRankApprovalsProvider _rankApprovalsProvider { get; }
        public IRankStatusProvider _rankStatusProvider { get; }
        public IUserProvider _userProvider { get; }
        public IRecordCompletion RecordCompletionProcess { get; }
        private IMultipartFormReader _multipartFormReader { get; }
        public IRankMapper RankMapper { get; }

        public RankStatusController( IRanksProvider ranksProvider, UserManager<ApplicationUser> userManager,
                                    IUserProvider userProvider, IRecordCompletion recordCompletionProcess, IRankMapper rankMapper,
                                    ILogger<RankStatusController> logger,
                                    IWebHostEnvironment environment, IConfiguration config, IMultipartFormReader multipartFormReader,
                                    IRankApprovalsProvider rankApprovalsProvider, IRankStatusProvider rankStatusProvider )
        {
            RanksProvider = ranksProvider;
            _userManager = userManager;
            _userProvider = userProvider;
            RecordCompletionProcess = recordCompletionProcess;
            RankMapper = rankMapper;
            this.logger = logger;
            this.environment = environment;
            _fileSizeLimit = config.GetValue<long>( "FileSizeLimit" );
            _multipartFormReader = multipartFormReader;
            _rankApprovalsProvider = rankApprovalsProvider;
            _rankStatusProvider = rankStatusProvider;
        }

        [HttpGet]
        [Authorize( Policy = "MustBeSubscriber" )]
        public IQueryable<RankStatus> GetRankStatus()
        {
            return _rankStatusProvider.RankStatuses();
        }

        [Authorize( Policy = "MustBeSubscriber" )]
        [HttpGet( "{rankId}" )]
        public async Task<ActionResult<RankStatus>> GetRankStatuses( Guid rankId )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );
            var rankStatus = await _rankStatusProvider.GetStatusesAsync( rankId, userIdForStatuses );
            if ( rankStatus == null )
            {
                return NotFound();
            }

            return Ok( rankStatus );
        }

        [HttpGet( "approvalsForRank/{rankId}/" )]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<IEnumerable<PendingApprovalDetail>>> GetApprovalsForRank( Guid rankId )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );
            var rankStatus = await _rankApprovalsProvider.AllApprovalsForRank( rankId, userIdForStatuses );
            return Ok( rankStatus );
        }

        [Authorize( Policy = "MustBeSubscriber" )]
        [Authorize( Roles = "Warrior" )]
        [HttpPost( "RecordCompletion" )]
        public async Task<IActionResult> RecordCompletion( RankStatusUpdateModel rankToUpdate )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            var userIdForStatuses = _userProvider.GetMyUserId( User );

            //var userValidationResult = await _userProvider.ValidateUserId( userIdForStatuses );

            //if ( !String.IsNullOrEmpty( userValidationResult ) )
            //{
            //    return BadRequest( userValidationResult );
            //}

            var status = await RecordCompletionProcess.RecordCompletionAsync( rankToUpdate, userIdForStatuses );

            if ( status.Success )
            {
                return Ok();
            }
            else
            {
                return BadRequest( status.Error );
            }
        }

        [Authorize( Policy = "MustBeSubscriber" )]
        [Authorize( Roles = "Guardian" )]
        [HttpPost( "RecordCompletionAsGuardian" )]
        public async Task<IActionResult> RecordCompletionAsGuardian( RankStatusUpdateModel rankToUpdate )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            var userIdForStatuses = _userProvider.GetMyUserId( User );

            //var userValidationResult = await _userProvider.ValidateUserId( userIdForStatuses );

            //if ( !String.IsNullOrEmpty( userValidationResult ) )
            //{
            //    return BadRequest( userValidationResult );
            //}

            var status = await RecordCompletionProcess.RecordCompletionAsync( rankToUpdate, userIdForStatuses );
            await _rankApprovalsProvider.MarkGuardianReviewedAsync( status.Status );
            
            if ( status.Success )
            {
                return Ok();
            }
            else
            {
                return BadRequest( status.Error );
            }
        }

        [Authorize( Policy = "MustBeSubscriber" )]
        [Authorize( Roles = "Warrior" )]
        [HttpPost( "ProofOfCompletion" )]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadProofOfCompletion()
        {
            var userIdForStatuses = _userProvider.GetMyUserId( User );
            string? relativePath = null;
            var attachmentIds = new List<Guid>();
            var error = String.Empty;
            await _multipartFormReader.Read( Request, ModelState, new[] { ".png", ".jpg", ".gif", ".pdf" }, _fileSizeLimit, async requestData =>
            {
                var reqIdKey = requestData.FormData.Keys.Cast<String>().FirstOrDefault( s => s.StartsWith( "reqId" ) );
                var rankIdKey = requestData.FormData.Keys.Cast<String>().FirstOrDefault( s => s.StartsWith( "rankId" ) );
                var rankId = Guid.Parse( requestData.FormData[ rankIdKey ]! );
                var reqId = Guid.Parse( requestData.FormData[ reqIdKey ]! );
                attachmentIds = await RecordCompletionProcess.UploadAttachmentsForRankReq( rankId, reqId, requestData.FileData, userIdForStatuses );
                var rankToUpdate = RankMapper.CreateRankStatusUpdateModel( rankId, reqId, new Guid[ 0 ], new Guid[ 0 ] );

                var status = await _rankStatusProvider.RecordCompletionAsync( rankToUpdate, userIdForStatuses );

                if ( status.Success )
                {
                    logger.LogInformation( "Proof of Completion upload successful" );
                }
                else
                {
                    logger.LogInformation( "Proof of Completion upload failed" );
                    error = status.Error;
                }
            } );
            //if ( !ModelState.IsValid )
            //{
            //    return BadRequest( ModelState );
            //}
            if ( error != String.Empty )
            {
                return BadRequest( error );
            }
            return Ok( attachmentIds );

        }


        // DELETE: api/Photos/5
        [HttpGet( "ProofOfCompletion/OneUseFileKey/{attachmentId}" )]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<Guid>> GetProofOfCompletionAttachmentOneUseFileKey( Guid attachmentId )
        {
            try
            {
                var requestorUserId = _userProvider.GetMyUserId( User );
                var attachment = await _rankStatusProvider.GetProofOfCompletionAttachmentByIdAsync( attachmentId );
                if ( attachment != null )
                {
                    if ( User.IsInRole( "Admin" ) || requestorUserId == attachment.UserId || await _userProvider.UserIsRelatedToWarrior( requestorUserId, attachment.UserId ) )
                    {
                        var fileKey = Guid.NewGuid();
                        await _rankStatusProvider.SaveProofOfCompletionOneUseFileKeyAsync( attachmentId, fileKey );
                        return Ok( fileKey );
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch ( Exception ex )
            {
                if ( !environment.IsDevelopment() )
                    logger.LogError( ex, "An unexpected error occurred and the Proof of Completion attachment was not retrieved" );
                return NotFound();
            }
        }


        // DELETE: api/Photos/5
        [AllowAnonymous]
        [HttpGet( "ProofOfCompletion/{oneUseFileKey}" )]
        public async Task<IActionResult> GetProofOfCompletionAttachment( Guid oneUseFileKey )
        {
            try
            {
                var contentResult = await _rankStatusProvider.GetProofOfCompletionAsync( oneUseFileKey );
                var fileResult = new PhysicalFileResult( contentResult.FilePath, contentResult.ContentType ) { FileDownloadName = contentResult.FileDownloadName };
                fileResult.FileDownloadName = contentResult.FileDownloadName;
                return fileResult;
            }
            catch ( Exception ex )
            {
                if ( !environment.IsDevelopment() )
                    logger.LogError( ex, "An unexpected error occurred and the Proof of Completion attachment was not retrieved", oneUseFileKey );
                return NotFound();
            }
        }

        [Authorize( Policy = "MustBeSubscriber" )]
        [Authorize( Roles = "Warrior" )]
        [HttpPost( "SubmitForApproval/{rankId}" )]
        public async Task<ActionResult> PostSubmitForPromotion( Guid rankId )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            var userIdForStatuses = _userProvider.GetMyUserId( User );

            //var userValidationResult = await _userProvider.ValidateUserId( userIdForStatuses );

            //if ( !String.IsNullOrEmpty( userValidationResult ) )
            //{
            //    return BadRequest( userValidationResult );
            //}

            RecordCompletionResponse status = default!;
            status = await _rankApprovalsProvider.SubmitForApprovalAsync( rankId, userIdForStatuses );

            if ( status.Success )
            {
                return Ok( status );
            }
            else
            {
                return BadRequest( status.Error );
            }
        }

        // POST: api/Rank/5
        [Authorize( Policy = "MustBeSubscriber" )]
        [HttpPost( "{approvalRecordId}/return" )]
        public async Task<ActionResult> PostReturn( Guid approvalRecordId, string? reason )
        {
            var userIdForStatuses = _userProvider.GetMyUserId( User );

            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            if ( User.IsInRole( "Warrior" ) )
            {
                await _rankApprovalsProvider.RecallAsync( approvalRecordId, userIdForStatuses );
            }
            else
            {
                await _rankApprovalsProvider.ReturnAsync( approvalRecordId, userIdForStatuses, reason );
            }

            return NoContent();
        }

        [Authorize( Policy = "MustBeSubscriber" )]
        [HttpGet( "PendingApproval/{userId}" )]
        public async Task<ActionResult<PendingApprovalDetail>> GetPendingApproval( Guid userId )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            var myUserId = _userProvider.GetMyUserId( User );
            if ( User.IsInRole( "Guardian" ) && !(await _userProvider.UserIsRelatedToWarrior( myUserId, userId )) )
            {
                return BadRequest( "Invalid user id." );
            }
            else if ( User.IsInRole( "Warrior" ) && myUserId != userId )
            {
                return BadRequest( "Invalid user id." );
            }

            var pendingApprovals = await _rankApprovalsProvider.GetPendingApprovalsAsync( userId );

            return Ok( pendingApprovals.FirstOrDefault() );
        }

        [HttpPost( "{approvalRecordId}/ApproveProgress" )]
        [Authorize( Policy = "MustBeSubscriber" )]
        [Authorize( Roles = "Guardian" )]
        public async Task<ActionResult> ApproveProgress( Guid approvalRecordId )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            var myUserId = _userProvider.GetMyUserId( User );
            //var userValidationResult = await _userProvider.ValidateUserId( myUserId );

            //if ( !String.IsNullOrEmpty( userValidationResult ) )
            //{
            //    return BadRequest( userValidationResult );
            //}

            var status = await _rankApprovalsProvider.ApproveProgressAsync( approvalRecordId, myUserId );

            if ( status.Success )
            {
                return Ok();
            }
            else
            {
                return BadRequest( status.Error );
            }
        }

        // POST: api/RankStatus
        [HttpPost]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<RankStatus>> PostRankStatus( RankStatus rankStatus )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            var updatedStatus = await _rankStatusProvider.PostRankStatusAsync( rankStatus );

            return CreatedAtRoute( "DefaultApi", new { id = updatedStatus.Id }, updatedStatus );
        }

        // DELETE: api/RankStatus/5
        [HttpDelete]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult> DeleteRankStatus( RankStatusUpdateModel rankToUpdate )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            var userIdForStatuses = _userProvider.GetMyUserId( User );

            //var userValidationResult = await _userProvider.ValidateUserId( userIdForStatuses );

            //if ( !String.IsNullOrEmpty( userValidationResult ) )
            //{
            //    return BadRequest( userValidationResult );
            //}

            var status = await _rankStatusProvider.DeleteRankStatusAsync( rankToUpdate, userIdForStatuses );

            if ( status.Success )
            {
                return Ok();
            }
            else
            {
                return BadRequest( status.Error );
            }
        }
    }
}