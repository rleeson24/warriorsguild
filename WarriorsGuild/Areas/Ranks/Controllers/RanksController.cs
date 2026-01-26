using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.Helpers.Filters;
using WarriorsGuild.Helpers.Utilities.Files;
using WarriorsGuild.Models;
using WarriorsGuild.Providers;
using WarriorsGuild.Ranks;
using WarriorsGuild.Ranks.Models;
using WarriorsGuild.Ranks.ViewModels;
using WarriorsGuild.Storage;
using WarriorsGuild.Storage.Models;
using static IdentityServer4.IdentityServerConstants;

namespace WarriorsGuild.Areas.Ranks.Controllers
{
    [ApiController]
    [Route( "api/[controller]" )]
    [Authorize( LocalApi.PolicyName )]
    [IgnoreAntiforgeryToken]
    public class RanksController : ControllerBase
    {
        private readonly ILogger<RanksController> _logger;
        private readonly IFileSystemProvider _fileSystemProvider;
        private readonly IRankValidator validator;
        private readonly long _fileSizeLimit;
        private IWebHostEnvironment _webHost;

        private IRanksProvider RanksProvider { get; }
        private IRankRequirementProvider _rankReqProvider { get; }
        private IMultipartFormReader _multipartFormReader { get; }
        private IWebHostEnvironment Environment { get; }
        public IUserProvider _userProvider { get; }

        public RanksController( IRanksProvider ranksProvider, IUserProvider userProvider, IMultipartFormReader multipartReader,
                                ILogger<RanksController> logger,
                                IWebHostEnvironment environment, IConfiguration config, IFileSystemProvider fileSystemProvider, IRankValidator validator, IWebHostEnvironment webHost, IRankRequirementProvider rankReqProvider )
        {
            RanksProvider = ranksProvider;
            _userProvider = userProvider;
            _multipartFormReader = multipartReader;
            this._logger = logger;
            Environment = environment;
            _fileSystemProvider = fileSystemProvider;
            this.validator = validator;
            _fileSizeLimit = config.GetValue<long>( "FileSizeLimit" );
            _webHost = webHost;
            _rankReqProvider = rankReqProvider;
        }

        // GET: api/Ranks
        [HttpGet]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<IQueryable<Rank>> GetRanksAsync()
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );

            var ranks = (await RanksProvider.GetListAsync( userIdForStatuses )).AsQueryable();
            return ranks;
        }

        // GET: api/Ranks
        [HttpGet( "Public" )]
        [AllowAnonymous]
        public async Task<Rank> GetPublicRank()
        {
            return await RanksProvider.GetPublicAsync();
        }

        // GET: api/Ranks/5
        [HttpGet( "{id}" )]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<Rank>> GetRank( Guid id )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );

            var result = await RanksProvider.GetAsync( id );

            if ( result == null )
            {
                return NotFound();
            }
            return Ok( result );
        }

        // GET: api/Ranks/5
        [AllowAnonymous]
        [HttpGet( "{rankId}/requirements" )]
        public async Task<ActionResult<IEnumerable<RankRequirementViewModel>>> GetRankRequirements( Guid rankId )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );

            //only allow getting requirements if the user is logged in or if the ring is public
            if ( User.Identity.IsAuthenticated ) // || (await RanksProvider.GetPublicAsync()).Id == rankId )
            {
                var requirements = await _rankReqProvider.GetRequirementsWithStatus( rankId, userIdForStatuses );
                if ( requirements == null )
                {
                    return NotFound();
                }
                return Ok( requirements );
            }
            else
            {
                var result = new List<RankRequirementViewModel>();
                return Ok( result );
            }
        }

        // GET: api/Ranks/5
        [Authorize( Policy = "MustBeSubscriber" )]
        [HttpGet( "Mine" )]
        public async Task<ActionResult<MyRankViewModel>> GetMyRank()
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );

            var result = await RanksProvider.GetCurrentRankAsync( userIdForStatuses );

            if ( result == null )
            {
                return NotFound();
            }
            return Ok( result );
        }

        // GET: api/Ranks/5
        [HttpGet( "ByUser/{userId}" )]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<MyRankViewModel>> GetCompletedAndWorkingRanks( string userId )
        {
            Guid idGuid;
            if ( !Guid.TryParse( userId, out idGuid ) )
            {
                return BadRequest( "The id is not in a valid format." );
            }

            var result = await RanksProvider.GetCurrentRankAsync( idGuid );

            if ( result == null )
            {
                return NotFound();
            }
            return Ok( result );
        }

        // PUT: api/Ranks/5
        [Authorize( Policy = "MustBeAdminApi" )]
        [HttpPut( "{id}" )]
        public async Task<ActionResult> PutRank( Guid id, Rank rank )
        {
            await RanksProvider.UpdateAsync( id, rank );

            return NoContent();
        }

        [Authorize( Policy = "MustBeAdminApi" )]
        [HttpPut( "{id}/requirements" )]
        public async Task<ActionResult> PutRequirements( Guid id, IEnumerable<RankRequirementViewModel> requirements )
        {
            var validationErrors = validator.ValidateRequirements( requirements );
            if ( validationErrors.Any() )
            {
                return BadRequest( validationErrors.First() );
            }

            await _rankReqProvider.UpdateRequirementsAsync( id, requirements );

            return NoContent();
        }

        // POST: api/Ranks
        [HttpPost]
        [Authorize( Policy = "MustBeAdminApi" )]
        public async Task<ActionResult<Rank>> PostRank( CreateRankModel input )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            var rank = await RanksProvider.AddAsync( input );

            return CreatedAtRoute( "DefaultApi", new { id = rank.Id }, rank );
        }

        // DELETE: api/Ranks/5
        [HttpDelete]
        [Authorize( Policy = "MustBeAdminApi" )]
        public async Task<ActionResult<Rank>> DeleteRank( Guid id )
        {
            var rank = await RanksProvider.GetAsync( id );
            if ( rank == null )
            {
                return NotFound();
            }

            return Ok( rank );
        }


        [HttpPost( "Order" )]
        [Authorize( Policy = "MustBeAdminApi" )]
        public async Task<ActionResult<IEnumerable<Rank>>> UpdateOrder( IEnumerable<GoalIndexEntry> request )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest();
            }
            var updatedRankOrder = await RanksProvider.UpdateOrderAsync( request );
            return Ok( updatedRankOrder );
        }


        [HttpGet( "Image/{id}" )]
        public async Task<ActionResult<Byte[]>> GetRankImage( Guid id )
        {
            try
            {
                var imageDetail = await RanksProvider.GetImage( id );
                return new PhysicalFileResult( imageDetail.FilePath, imageDetail.ContentType ) { FileDownloadName = imageDetail.FileDownloadName };
            }
            catch ( Exception ex )
            {
                if ( !Environment.IsDevelopment() )
                    _logger.LogError( ex, "An unexpected error occurred and the Rank image was not retrieved" );
                return NotFound();
            }
        }

        [Authorize( Policy = "MustBeAdminApi" )]
        [HttpPost( "UploadImage/{rankId}" )]
        [DisableFormValueModelBinding]
        [ProducesResponseType( StatusCodes.Status200OK )]
        [ProducesResponseType( typeof( string ), StatusCodes.Status400BadRequest )]
        public async Task<IActionResult> UploadImage( Guid rankId )
        {
            string? relativePath = null;
            await _multipartFormReader.Read( Request, ModelState, new[] { ".png", ".jpg", ".gif" }, _fileSizeLimit, async requestData =>
            {
                var fileData = requestData.FileData.First();
                var trustedFileNameForFileStorage = rankId.ToString() + fileData.Extension;
                var fileUploadResult = await _fileSystemProvider.SaveFileToContentImages( _webHost.WebRootPath, WarriorsGuildFileType.RankImage, fileData, trustedFileNameForFileStorage );
                relativePath = fileUploadResult.Path;
                await RanksProvider.UploadImageAsync( rankId, fileData.Extension, null, MimeTypeMap.GetMimeType( fileData.Extension ) );
            } );
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            return Created( $@"/{relativePath}", null );
        }


        [HttpPost( "UploadGuide/{rankId}" )]
        [Authorize( Policy = "MustBeAdminApi" )]
        [DisableFormValueModelBinding]
        [ProducesResponseType( StatusCodes.Status200OK )]
        [ProducesResponseType( typeof( string ), StatusCodes.Status400BadRequest )]
        public async Task<ActionResult> UploadGuide( Guid rankId )
        {
            await _multipartFormReader.Read( Request, ModelState, new[] { ".pdf" }, _fileSizeLimit, async requestData =>
            {
                var contentPath = this.Environment.WebRootPath;

                var fileData = requestData.FileData.First();
                var ext = Path.GetExtension( fileData.ContentDisposition.FileName.Value ).ToLowerInvariant();

                await RanksProvider.UploadGuideAsync( rankId, ext, fileData.Content, MimeTypeMap.GetMimeType( ext ) );
                _logger.LogInformation(
                    "Uploaded file '{TrustedFileNameForDisplay}'", fileData.ContentDisposition.FileName.Value );
            } );
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            return Ok();
        }

        [HttpGet( "Guide/{rankId}" )]
        [AllowAnonymous]
        public async Task<IActionResult> GetGuide( Guid rankId )
        {
            try
            {
                var guideDetail = await RanksProvider.GetGuideAsync( rankId );
                var result = new PhysicalFileResult( guideDetail.FilePath, guideDetail.ContentType );
                result.FileDownloadName = rankId.ToString() + ".pdf";
                return result;
            }
            catch ( Exception ex )
            {
                if ( !Environment.IsDevelopment() )
                    _logger.LogError( ex, "An unexpected error occurred and the Rank guide was not retrieved" );
                return NotFound();
            }
        }
    }
}