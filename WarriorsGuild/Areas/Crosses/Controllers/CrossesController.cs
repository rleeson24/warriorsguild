using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WarriorsGuild.Crosses;
using WarriorsGuild.Crosses.Crosses.Status;
using WarriorsGuild.Crosses.Mappers;
using WarriorsGuild.Crosses.Models;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Crosses;
using WarriorsGuild.Helpers.Filters;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Helpers.Utilities.Files;
using WarriorsGuild.Models;
using WarriorsGuild.Providers;
using WarriorsGuild.Storage;
using WarriorsGuild.Storage.Models;
using static IdentityServer4.IdentityServerConstants;

namespace WarriorsGuild.Areas.Crosses.Controllers
{
    [ApiController]
    [Route( "api/[controller]" )]
    [Authorize( LocalApi.PolicyName )]
    [IgnoreAntiforgeryToken]
    public class CrossesController : ControllerBase
    {
        private ICrossProvider CrossProvider { get; }
        private ICrossMapper CrossMapper { get; }
        private IMultipartFormReader _multipartFormReader { get; }
        private UserManager<ApplicationUser> _userManager;
        private readonly IFileSystemProvider _fileSystemProvider;
        private readonly IWebHostEnvironment _webHost;
        private readonly SessionManager sessionManager;
        private readonly IUserProvider _userProvider;
        private readonly ILogger<CrossesController> _logger;
        private IWebHostEnvironment Environment { get; }

        private readonly long _fileSizeLimit;

        public UserManager<ApplicationUser> UserManager
        {
            get { return _userManager; }
        }

        public CrossesController( ICrossProvider crossProvider, ICrossMapper crossMapper, UserManager<ApplicationUser> userManager,
                                    IMultipartFormReader attProvider, SessionManager sessionManager, IUserProvider userProvider,
                                    IConfiguration config, IWebHostEnvironment environment, ILogger<CrossesController> logger, IFileSystemProvider fileSystemProvider, IWebHostEnvironment webHost )
        {
            CrossProvider = crossProvider;
            CrossMapper = crossMapper;
            _userManager = userManager;
            _multipartFormReader = attProvider;
            this.sessionManager = sessionManager;
            _userProvider = userProvider;
            _logger = logger;
            _fileSystemProvider = fileSystemProvider;
            this._webHost = webHost;
            Environment = environment;
            _fileSizeLimit = config.GetValue<long>( "FileSizeLimit" );
        }

        // GET: api/Rings
        [HttpGet]
        //[Authorize]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<IQueryable<Cross>> GetCrosses()
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );
            var userIdGuid = userIdForStatuses != null ? userIdForStatuses : Guid.Empty;

            var crosses = (await CrossProvider.ListAsync( userIdGuid )).AsQueryable();
            return crosses;
        }

        // GET: api/Rings
        [HttpGet( "Public" )]
        [AllowAnonymous]
        public async Task<CrossViewModel> GetPublicCross()
        {
            return await CrossProvider.GetPublicAsync();
        }

        // GET: api/Rings
        [HttpGet( "Completed" )]
        public async Task<IEnumerable<CrossViewModel>> GetCompletedCrosses()
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );
            return (await CrossProvider.GetCompletedAsync( userIdForStatuses )).Select( CrossMapper.MapToViewModel );
        }

        // GET: api/Rings
        [HttpGet( "ByUser/{userId}/Completed" )]
        public async Task<IEnumerable<CrossViewModel>> GetCompletedCrosses( Guid userId )
        {
            return (await CrossProvider.GetCompletedAsync( userId )).Select( CrossMapper.MapToViewModel );
        }

        // GET: api/Rings/5
        [HttpGet( "{crossId}" )]
        [Authorize]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<CrossViewModel>> GetCross( Guid crossId )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );
            var userIdGuid = userIdForStatuses != null ? userIdForStatuses : Guid.Empty;

            var result = await CrossProvider.GetAsync( crossId, userIdGuid );

            if ( result == null )
            {
                return NotFound();
            }

            return Ok( result );
        }

        // GET: api/Rings/5
        [AllowAnonymous]
        [HttpGet( "{crossId}/questions" )]
        public async Task<ActionResult<IEnumerable<CrossQuestion>>> GetCrossQuestions( Guid crossId )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );
            var userIdGuid = userIdForStatuses != null ? userIdForStatuses : Guid.Empty;

            var result = User.Identity.IsAuthenticated ? await CrossProvider.GetQuestionsAsync( crossId, userIdGuid ) : new CrossQuestionViewModel[ 0 ];

            if ( result == null )
            {
                return NotFound();
            }

            return Ok( result );
        }

        // GET: api/Rings/5
        [HttpGet( "Pinned/active" )]
        //[Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<IEnumerable<PinnedCross>>> GetActivePinnedCrosses()
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );
            var pinnedCrosses = await CrossProvider.GetActivePinnedCrosses( userIdForStatuses );
            var result = pinnedCrosses;

            if ( result == null )
            {
                return NotFound();
            }

            return Ok( result );
        }

        // GET: api/Rings/5
        [HttpGet( "Pinned" )]
        //[Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<IEnumerable<PinnedCross>>> GetPinnedCrosses()
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );
            var pinnedCrosses = await CrossProvider.GetPinnedCrosses( userIdForStatuses );
            var result = pinnedCrosses;

            if ( result == null )
            {
                return NotFound();
            }

            return Ok( result );
        }

        // GET: api/Rings/5
        [HttpGet( "Byuser/{userId}/Pinned" )]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<IEnumerable<Cross>>> GetPinnedCrosses( Guid userId )
        {
            var result = (await CrossProvider.GetActivePinnedCrosses( userId )).Select( r => r.Cross );

            if ( result == null )
            {
                return NotFound();
            }

            return Ok( result );
        }

        // PUT: api/Rings/5
        [HttpPut( "{crossId}" )]
        [Authorize( Policy = "MustBeAdminApi" )]
        public async Task<ActionResult> PutCross( Guid crossId, Cross cross )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            await CrossProvider.UpdateAsync( crossId, cross );

            return NoContent();
        }

        // PUT: api/Rings/5
        //[HttpPut( "{id}/questions" )]
        //[Authorize( Policy = "MustBeAdminApi" )]
        //public async Task<ActionResult> PutQuestions( Guid id, CrossQuestion[] questions )
        //{
        //    if ( !ModelState.IsValid )
        //    {
        //        return BadRequest( ModelState );
        //    }

        //    await CrossProvider.SaveQuestionsAsync( id, questions );

        //    return NoContent();
        //}

        // POST: api/Rings
        [HttpPost]
        [Authorize( Policy = "MustBeAdminApi" )]
        public async Task<ActionResult<Cross>> PostCross( CreateCrossModel input )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            var cross = await CrossProvider.AddAsync( input );

            return CreatedAtRoute( "DefaultApi", new { id = cross.Id }, cross );
        }


        // DELETE: api/Rings/5
        [HttpDelete]
        [Authorize( Policy = "MustBeAdminApi" )]
        public async Task<ActionResult<Cross>> DeleteCross( Guid crossId )
        {
            var cross = await CrossProvider.DeleteAsync( crossId );
            if ( cross == null )
            {
                return NotFound();
            }

            return Ok( cross );
        }

        [HttpPost( "Order" )]
        [Authorize( Policy = "MustBeAdminApi" )]
        public async Task<ActionResult<IEnumerable<Cross>>> UpdateCrossOrder( IEnumerable<GoalIndexEntry> request )
        {
            var updatedCrossOrder = await CrossProvider.UpdateOrderAsync( request );
            return Ok( updatedCrossOrder );
        }


        [HttpGet( "Image/{crossId}" )]
        public async Task<ActionResult<byte[]>> GetCrossImage( Guid crossId )
        {
            try
            {
                var imageDetail = await CrossProvider.GetImageAsync( crossId );
                return new PhysicalFileResult( imageDetail.FilePath, imageDetail.ContentType ) { FileDownloadName = imageDetail.FileDownloadName };
            }
            catch ( Exception ex )
            {
                if ( !Environment.IsDevelopment() )
                    _logger.LogError( ex, "An unexpected error occurred and the Cross image was not retrieved" );
                return NotFound();
            }
        }

        [Authorize( Policy = "MustBeAdminApi" )]
        [HttpPost( "UploadImage/{crossId}" )]
        [DisableFormValueModelBinding]
        [ProducesResponseType( StatusCodes.Status200OK )]
        [ProducesResponseType( typeof( string ), StatusCodes.Status400BadRequest )]
        public async Task<IActionResult> UploadImage( Guid crossId )
        {
            string? relativePath = null;
            await _multipartFormReader.Read( Request, ModelState, new[] { ".png", ".jpg", ".gif" }, _fileSizeLimit, async requestData =>
            {
                var fileData = requestData.FileData.First();
                var trustedFileNameForFileStorage = crossId.ToString() + fileData.Extension;
                var fileUploadResult = await _fileSystemProvider.SaveFileToContentImages( _webHost.WebRootPath, WarriorsGuildFileType.CrossImage, fileData, trustedFileNameForFileStorage );
                relativePath = fileUploadResult.Path;
                await CrossProvider.UploadImageAsync( crossId, fileData.Extension, null, MimeTypeMap.GetMimeType( fileData.Extension ) );
            } );
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            return Created( $@"/{relativePath}", null );
        }

        [HttpPost( "UploadGuide/{crossId}" )]
        [Authorize( Policy = "MustBeAdminApi" )]
        [DisableFormValueModelBinding]
        [ProducesResponseType( StatusCodes.Status200OK )]
        [ProducesResponseType( typeof( string ), StatusCodes.Status400BadRequest )]
        public async Task<ActionResult> UploadGuide( Guid crossId )
        {
            await _multipartFormReader.Read( Request, ModelState, new[] { ".pdf" }, _fileSizeLimit, async requestData =>
            {
                var contentPath = Environment.WebRootPath;
                var fileData = requestData.FileData.First();

                var ext = Path.GetExtension( fileData.ContentDisposition.FileName.Value ).ToLowerInvariant();

                await CrossProvider.UploadGuideAsync( crossId, ext, fileData.Content, MimeTypeMap.GetMimeType( ext ) );
                _logger.LogInformation(
                    "Uploaded file '{TrustedFileNameForDisplay}'", fileData.ContentDisposition.FileName.Value );
            } );
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            return Ok();
        }

        [HttpGet( "Guide/{crossId}" )]
        [AllowAnonymous]
        public async Task<IActionResult> GetGuide( Guid crossId )
        {
            try
            {
                var guideDetail = await CrossProvider.GetGuideAsync( crossId );
                var result = new PhysicalFileResult( guideDetail.FilePath, guideDetail.ContentType );
                result.FileDownloadName = crossId.ToString() + ".pdf";
                return result;
            }
            catch ( Exception ex )
            {
                if ( !Environment.IsDevelopment() )
                    _logger.LogError( ex, "An unexpected error occurred and the Cross guide was not retrieved" );
                return NotFound();
            }
        }


        // PUT: api/Rings/5
        [HttpPut( "{crossId}/answers" )]
        public async Task<ActionResult> PutAnswers( Guid crossId, CrossAnswerViewModel[] answers )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );

            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            await CrossProvider.SaveAnswersAsync( crossId, userIdForStatuses, answers );

            return NoContent();
        }


        // PUT: api/Rings/5
        [HttpPut( "{crossId}/day/{dayId}/answers" )]
        public async Task<ActionResult> PutDayAnswers( Guid crossId, Guid dayId, CrossAnswerViewModel[] answers )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );

            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            await CrossProvider.SaveAnswersAsync( crossId, dayId, userIdForStatuses, answers );

            return NoContent();
        }


        // PUT: api/Rings/5
        [HttpGet( "{crossId}/days" )]
        public async Task<ActionResult> GetDays( Guid crossId )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );
            var myUserId = _userProvider.GetMyUserId( User );
            var childUsers = await _userProvider.GetChildUsers( myUserId.ToString() );

            var days = await CrossProvider.GetDaysAsync( crossId, userIdForStatuses );
            //remove answers if the warrior or guardian is not the one requesting the data
            if ( userIdForStatuses != myUserId && !childUsers.Any( u => u.Id == userIdForStatuses.ToString( "D" ) ) )
            {
                days.ToList().ForEach( d => d.Questions.ToList().ForEach( q => q.Answer = null ) );
            }

            return Ok( days );
        }


        // PUT: api/Rings/5
        [HttpPut( "{crossId}/days" )]
        public async Task<ActionResult> PutDays( Guid crossId, IEnumerable<CrossDayViewModel> days )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            var totalWeight = days.Sum( r => r.Weight );
            if ( totalWeight != 0 && totalWeight != 100 )
            {
                return BadRequest( $"The day weights must add up to 0 or 100.  Current sum is {totalWeight}" );
            }

            days = await CrossProvider.SaveDaysAsync( crossId, days );

            return Ok( days );
        }

        // GET: api/RingStatus/5
        [HttpGet( "Unassigned" )]
        public async Task<ActionResult<UnassignedCrossViewModel>> GetUnassigned()
        {
            var crossStatus = await CrossProvider.GetUnassigned();
            return Ok( crossStatus );
        }

        [HttpGet( "questionsByTemplate/{templateName}" )]
        public async Task<ActionResult<IEnumerable<CrossQuestionViewModel>>> GetTemplateQuestions( string templateName )
        {
            IEnumerable<CrossQuestionViewModel> questions = await CrossProvider.GetTemplateQuestions( templateName );
            return Ok( questions );
        }

        // PUT: api/Rings/5
        [HttpPost( "Pin/{id}" )]
        public async Task<ActionResult> PinCross( Guid id )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );
            await CrossProvider.PinAsync( id, userIdForStatuses );

            return NoContent();
        }

        // PUT: api/Rings/5
        [HttpPost( "Unpin/{id}" )]
        public async Task<ActionResult> UnpinCross( Guid id )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );
            await CrossProvider.UnpinAsync( id, userIdForStatuses );

            return NoContent();
        }
    }
}