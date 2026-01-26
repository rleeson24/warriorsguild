using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Rings.Status;
using WarriorsGuild.Helpers.Filters;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Helpers.Utilities.Files;
using WarriorsGuild.Providers;
using WarriorsGuild.Rings;
using WarriorsGuild.Rings.Mappers;
using WarriorsGuild.Rings.Models.Status;
using WarriorsGuild.Rings.ViewModels;
using static IdentityServer4.IdentityServerConstants;

namespace WarriorsGuild.Areas.Rings.Controllers
{
    [ApiController]
    [Route( "api/RingStatus" )]
    [Authorize( LocalApi.PolicyName )]
    [IgnoreAntiforgeryToken]
    public class RingStatusController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private readonly IMultipartFormReader _multipartFormReader;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISessionManager sessionManager;
        private readonly long _fileSizeLimit;
        private readonly ILogger<RingStatusController> _logger;
        private readonly IWebHostEnvironment environment;
        private readonly string[] _permittedExtensions = { ".txt" };
        private readonly string? _targetFilePath;

        // Get the default form options so that we can use them to set the default 
        // limits for request body data.
        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        public UserManager<ApplicationUser> UserManager
        {
            get { return _userManager; }
        }

        public IRingsProvider RingsProvider;
        public IUserProvider _userProvider { get; }
        public IRecordRingCompletion RecordCompletionProcess { get; }
        public IRingMapper RingMapper { get; }

        public RingStatusController( IRingsProvider ringsProvider, UserManager<ApplicationUser> userManager, IUserProvider userProvider, IRecordRingCompletion recordCompletion, IConfiguration config,
                                    IRingMapper ringMapper, IHttpContextAccessor httpContextAccessor, ISessionManager sessionManager, ILogger<RingStatusController> logger, IWebHostEnvironment environment, IMultipartFormReader multipartFormReader )
        {
            RingsProvider = ringsProvider;
            _userManager = userManager;
            _userProvider = userProvider;
            RecordCompletionProcess = recordCompletion;
            RingMapper = ringMapper;
            this._httpContextAccessor = httpContextAccessor;
            this.sessionManager = sessionManager;
            _logger = logger;
            this.environment = environment;
            _fileSizeLimit = config.GetValue<long>( "FileSizeLimit" );

            // To save physical files to a path provided by configuration:
            _targetFilePath = config.GetValue<string>( "StoredFilesPath" );
            _multipartFormReader = multipartFormReader;
        }

        // GET: api/RingStatus
        [HttpGet]
        [Authorize( Policy = "MustBeSubscriber" )]
        public IQueryable<RingStatus> GetRingStatus()
        {
            return RingsProvider.GetRingStatus();
        }

        // GET: api/RingStatus/5
        [HttpGet( "{id}" )]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<RingStatus>> GetRingStatus( int id )
        {
            var ringStatus = await RingsProvider.GetRingStatusAsync( id );
            if ( ringStatus == null )
            {
                return NotFound();
            }

            return Ok( ringStatus );
        }

        // GET: api/RingStatus/5
        [HttpGet( "Unassigned" )]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<IEnumerable<UnassignedRingViewModel>>> GetUnassignedPendingOrApproved()
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );
            var ringStatus = await RingsProvider.GetUnassignedPendingOrApproved( userIdForStatuses );
            if ( ringStatus == null )
            {
                return NotFound();
            }

            return Ok( ringStatus );
        }

        // PUT: api/RingStatus/5
        [HttpPost( "RecordCompletion" )]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult> RecordCompletion( RingStatusUpdateModel ringToUpdate )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            var userIdForStatuses = _userProvider.GetMyUserId( User );

            var status = await RingsProvider.RecordCompletionAsync( ringToUpdate, userIdForStatuses );

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
        [HttpPost( "ProofOfCompletion" )]
        [DisableFormValueModelBinding]
        public async Task<ActionResult<IEnumerable<Guid>>> UploadProofOfCompletion()
        {
            var myUserId = _userProvider.GetMyUserId( User );
            var attachmentIds = new List<Guid>();
            var error = String.Empty;
            await _multipartFormReader.Read( Request, ModelState, new[] { ".png", ".jpg", ".gif", ".pdf" }, _fileSizeLimit, async requestData =>
            {
                var reqIdKey = requestData.FormData.Keys.Cast<String>().FirstOrDefault( s => s.StartsWith( "reqId" ) );
                var rankIdKey = requestData.FormData.Keys.Cast<String>().FirstOrDefault( s => s.StartsWith( "ringId" ) );
                var ringId = Guid.Parse( requestData.FormData[ rankIdKey ]! );
                var reqId = Guid.Parse( requestData.FormData[ reqIdKey ]! );
                attachmentIds = await RecordCompletionProcess.UploadAttachmentsForRingReq( ringId, reqId, requestData.FileData, myUserId );
                var rankToUpdate = RingMapper.CreateRingStatusUpdateModel( ringId, reqId );

                var status = await RingsProvider.RecordCompletionAsync( rankToUpdate, myUserId );

                if ( status.Success )
                {
                    _logger.LogInformation( "Proof of Completion upload successful" );
                }
                else
                {
                    _logger.LogInformation( "Proof of Completion upload failed" );
                    error = status.Error;
                }
            } );
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            if ( error != String.Empty )
            {
                return BadRequest( error );
            }
            return Ok( attachmentIds );

        }

        [HttpGet( "PendingApprovals/{userId}" )]
        [Authorize( Policy = "MustBeSubscriber" )]
        [Authorize( Roles = "Guardian" )]
        public async Task<ActionResult<IEnumerable<PendingApprovalDetail>>> GetPendingApprovals( Guid userId )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            var myUserId = _userProvider.GetMyUserId( User );

            if ( !(await _userProvider.UserIsRelatedToWarrior( myUserId, userId )) )
            {
                return BadRequest( "Invalid user id." );
            }

            var pendingApprovals = await RingsProvider.GetPendingApprovalsAsync( userId );

            return Ok( pendingApprovals );
        }

        [HttpGet( "{ringId}/PendingApproval" )]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<PendingApprovalDetail>> GetPendingApproval( Guid ringId )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );

            var pendingApproval = await RingsProvider.GetPendingApprovalAsync( userIdForStatuses, ringId );

            return Ok( pendingApproval );
        }


        // DELETE: api/Photos/5
        [HttpGet( "ProofOfCompletion/OneUseFileKey/{attachmentId}" )]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<Guid>> GetProofOfCompletionAttachmentOneUseFileKey( Guid attachmentId )
        {
            try
            {
                var requestorUserId = _userProvider.GetMyUserId( User );
                var attachment = await RingsProvider.GetProofOfCompletionAttachmentByIdAsync( attachmentId );
                if ( attachment != null )
                {
                    if ( User.IsInRole( "Admin" ) || requestorUserId == attachment.UserId || await _userProvider.UserIsRelatedToWarrior( requestorUserId, attachment.UserId ) )
                    {
                        var fileKey = Guid.NewGuid();
                        await RingsProvider.SaveProofOfCompletionOneUseFileKeyAsync( attachmentId, fileKey );
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
                    _logger.LogError( ex, "An unexpected error occurred and the Proof of Completion attachment was not retrieved" );
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
                var contentResult = await RingsProvider.GetProofOfCompletionAsync( oneUseFileKey );
                var fileResult = new PhysicalFileResult( contentResult.FilePath, contentResult.ContentType ) { FileDownloadName = contentResult.FileDownloadName };
                fileResult.FileDownloadName = contentResult.FileDownloadName;
                return fileResult;
            }
            catch ( Exception ex )
            {
                if ( !environment.IsDevelopment() )
                    _logger.LogError( ex, "An unexpected error occurred and the Proof of Completion attachment was not retrieved", oneUseFileKey );
                return NotFound();
            }
        }

        [HttpPost( "{ringId}/SubmitForApproval" )]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<RingApproval>> PostSubmitForApproval( Guid ringId )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );

            var pendingApproval = await RingsProvider.SubmitForApprovalAsync( userIdForStatuses, ringId );
            if ( pendingApproval != null )
            {
                return Ok( pendingApproval );
            }
            else
            {
                return BadRequest( "A pending approval record already exists" );
            }
        }

        // POST: api/Rings/5
        [HttpPost( "{approvalRecordId}/return" )]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult> PostReturn( Guid approvalRecordId, string? userReason )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );

            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            await RingsProvider.ReturnAsync( approvalRecordId, userIdForStatuses, userReason );

            return NoContent();
        }

        // PUT: api/RingStatus/5
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

            var status = await RingsProvider.ApproveProgressAsync( approvalRecordId, myUserId );

            if ( status.Success )
            {
                return Ok();
            }
            else
            {
                return BadRequest( status.Error );
            }
        }

        // POST: api/RingStatus
        [HttpPost]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<RingStatus>> PostRingStatus( RingStatus ringStatus )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            var updatedStatus = await RingsProvider.PostRingStatusAsync( ringStatus );

            return CreatedAtRoute( "DefaultApi", new { id = updatedStatus.Id }, updatedStatus );
        }


        // DELETE: api/RankStatus/5
        [HttpDelete]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult> DeleteRingStatus( RingStatusUpdateModel ringToUpdate )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            var myUserId = _userProvider.GetMyUserId( User );

            if ( await UserManager.FindByIdAsync( myUserId.ToString() ) == null )
            {
                return BadRequest( "Invalid user was supplied." );
            }

            var status = await RingsProvider.DeleteRingStatusAsync( ringToUpdate, myUserId );

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