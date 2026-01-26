using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WarriorsGuild.Crosses;
using WarriorsGuild.Crosses.Crosses.Status;
using WarriorsGuild.Crosses.Models;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Providers;
using static IdentityServer4.IdentityServerConstants;

namespace WarriorsGuild.Areas.Crosses.Controllers
{
    [Route( "api/[controller]" )]
    [ApiController]
    [Authorize( LocalApi.PolicyName )]
    [IgnoreAntiforgeryToken]
    public class CrossStatusController : ControllerBase
    {
        private ICrossProvider CrossProvider { get; }
        private UserManager<ApplicationUser> _userManager;
        private readonly SessionManager sessionManager;
        private readonly ILogger<CrossStatusController> _logger;
        private readonly IUserProvider _userProvider;

        public UserManager<ApplicationUser> UserManager
        {
            get { return _userManager; }
        }

        public CrossStatusController( ICrossProvider crossProvider, UserManager<ApplicationUser> userManager,
                                    SessionManager sessionManager, ILogger<CrossStatusController> logger, IUserProvider userProvider )
        {
            CrossProvider = crossProvider;
            _userManager = userManager;
            this.sessionManager = sessionManager;
            this._logger = logger;
            _userProvider = userProvider;
        }

        // POST: api/Rings/5
        [HttpPost( "{id}/complete" )]
        public async Task<ActionResult> PostComplete( Guid id, CrossAnswerViewModel[] answers )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );

            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            if ( answers.Any( a => String.IsNullOrWhiteSpace( a.Answer ) ) )
            {
                return BadRequest( "You must answer all questions before you can complete the cross" );
            }

            var result = await CrossProvider.CompleteAsync( id, userIdForStatuses, answers );

            return Created( String.Empty, result );
        }

        // POST: api/Rings/5
        [HttpPost( "{id}/day/{dayId}/complete" )]
        public async Task<ActionResult> PostCompleteDay( Guid id, Guid dayId, CrossAnswerViewModel[] answers )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );

            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            if ( answers.Any( a => String.IsNullOrWhiteSpace( a.Answer ) ) )
            {
                return BadRequest( "You must answer all questions before you can complete the cross" );
            }

            await CrossProvider.CompleteDayAsync( id, userIdForStatuses, answers, dayId );

            return NoContent();
        }

        // POST: api/Rings/5
        [HttpPost( "{id}/return" )]
        public async Task<ActionResult> PostReturn( Guid id, string? userReason )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );

            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            await CrossProvider.ReturnAsync( id, userIdForStatuses, userReason );

            return NoContent();
        }

        // POST: api/Rings/5
        [HttpPost( "{crossId}/day/{dayId}/return" )]
        public async Task<ActionResult> PostReturn( Guid crossId, Guid dayId, string? userReason )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );

            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            await CrossProvider.ReturnDay( crossId, dayId, userIdForStatuses, userReason );

            return NoContent();
        }

        // POST: api/Rings/5
        [HttpPost( "{approvalRecordId}/confirmComplete" )]
        public async Task<ActionResult> PostConfirm( Int32 approvalRecordId )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );

            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            await CrossProvider.ConfirmCompleteAsync( approvalRecordId, userIdForStatuses );

            return NoContent();
        }

        [HttpGet( "PendingApprovals/{userId}" )]
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

            var pendingApprovals = await CrossProvider.GetPendingApprovalsAsync( userId );

            return Ok( pendingApprovals );
        }

        [HttpGet( "PendingApprovalsByCross/{crossId}" )]
        public async Task<ActionResult<IEnumerable<PendingApprovalDetail>>> GetPendingApprovalsByCross( Guid crossId )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );

            var pendingApprovals = await CrossProvider.GetPendingApprovalsByCrossAsync( crossId, userIdForStatuses );

            return Ok( pendingApprovals );
        }
    }
}
