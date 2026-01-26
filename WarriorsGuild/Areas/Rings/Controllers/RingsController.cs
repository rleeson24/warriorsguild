using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarriorsGuild.Crosses;
using WarriorsGuild.Data.Models.Crosses;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.Data.Models.Rings;
using WarriorsGuild.Helpers.Filters;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Helpers.Utilities.Files;
using WarriorsGuild.Models;
using WarriorsGuild.Providers;
using WarriorsGuild.Rings;
using WarriorsGuild.Rings.Mappers;
using WarriorsGuild.Rings.ViewModels;
using WarriorsGuild.Storage;
using WarriorsGuild.Storage.Models;
using static IdentityServer4.IdentityServerConstants;

namespace WarriorsGuild.Areas.Rings.Controllers
{
    [ApiController]
    [Route( "api/Rings" )]
    [Authorize( LocalApi.PolicyName )]
    [IgnoreAntiforgeryToken]
    public class RingsController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SessionManager sessionManager;
        private readonly IWebHostEnvironment Environment;
        private readonly IMultipartFormReader _multipartFormReader;
        private readonly IFileSystemProvider _fileSystemProvider;
        private readonly IUserProvider _userProvider;
        private readonly long _fileSizeLimit;
        private readonly ILogger<RingsController> _logger;
        private readonly IConfiguration config;
        private readonly IRingValidator _validator;
        private readonly IRingMapper _ringMapper;
        private IWebHostEnvironment _webHost;

        private IRingsProvider RingsProvider { get; }

        public RingsController( ILogger<RingsController> logger, IConfiguration config, IRingsProvider ringsProvider, IRingValidator ringValidator,
                                IRingMapper ringMapper, IHttpContextAccessor httpContextAccessor, SessionManager sessionManager,
                                IWebHostEnvironment environment, IMultipartFormReader multipartFormReader, IFileSystemProvider fileSystemProvider, IUserProvider userProvider, IWebHostEnvironment webHost )
        {
            _logger = logger;
            this.config = config;
            RingsProvider = ringsProvider;
            _validator = ringValidator;
            this._ringMapper = ringMapper;
            //MultipartReader = attProvider;
            this._httpContextAccessor = httpContextAccessor;
            this.sessionManager = sessionManager;
            this.Environment = environment;
            this._multipartFormReader = multipartFormReader;
            this._fileSystemProvider = fileSystemProvider;
            this._userProvider = userProvider;
            _fileSizeLimit = config.GetValue<long>( "FileSizeLimit" );
            _webHost = webHost;
        }

        // GET: api/Rings
        [HttpGet]
        //[Authorize( Policy = "MustBeSubscriber" )]
        public async Task<IQueryable<Ring>> GetRingsAsync()
        {
            var rings = (await RingsProvider.GetListAsync()).AsQueryable();
            return rings;
        }

        // GET: api/Rings
        [HttpGet( "Public" )]
        [AllowAnonymous]
        public async Task<Ring> GetPublicRingAsync()
        {
            return await RingsProvider.GetPublicAsync();
        }

        // GET: api/Rings
        [HttpGet( "Completed" )]
        public async Task<IEnumerable<Ring>> GetCompletedRings()
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );
            return await RingsProvider.GetCompletedAsync( userIdForStatuses );
        }

        // GET: api/Rings
        [HttpGet( "ByUser/{id}/Completed" )]
        public async Task<IEnumerable<Ring>> GetCompletedRings( Guid id )
        {
            return await RingsProvider.GetCompletedAsync( id );
        }

        // GET: api/Rings/5
        [HttpGet( "{id}" )]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<Ring>> GetRing( Guid id )
        {
            var result = await RingsProvider.GetAsync( id );

            if ( result == null )
            {
                return NotFound();
            }

            return Ok( result );
        }

        // GET: api/Ranks/5
        [AllowAnonymous]
        [HttpGet( "{ringId}/requirements" )]
        public async Task<ActionResult<IEnumerable<RingRequirementViewModel>>> GetRingRequirements( Guid ringId )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );

            //only allow getting requirements if the user is logged in or if the ring is public
            if ( User.Identity != null && User.Identity.IsAuthenticated ) //|| (await RingsProvider.GetPublicAsync()).Id == id )
            {
                var requirements = await RingsProvider.GetRequirementsAsync( ringId );
                var statuses = await RingsProvider.GetStatusesAsync( ringId, userIdForStatuses );
                if ( requirements == null )
                {
                    return NotFound();
                }

                var result = new List<RingRequirementViewModel>();
                foreach ( var requirement in requirements )
                {
                    var status = statuses?.FirstOrDefault( s => s.RingRequirementId == requirement.Id );
                    IEnumerable<MinimalGoalDetail> attachments = new MinimalGoalDetail[ 0 ];
                    if ( requirement.RequireAttachment && status != null )
                    {
                        attachments = await RingsProvider.GetAttachmentsForRingStatusAsync( requirement.Id, userIdForStatuses );
                    }
                    result.Add( _ringMapper.CreateRequirementViewModel( requirement, status, attachments ) );
                }
                return Ok( result );
            }
            else
            {
                var result = new List<RingRequirementViewModel>();
                return Ok( result );
            }
        }

        // GET: api/Rings/5
        [HttpGet( "Pinned/active" )]
        //[Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<IEnumerable<PinnedRing>>> GetActivePinnedRings()
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );

            var result = (await RingsProvider.GetActivePinnedRings( userIdForStatuses ));

            if ( result == null )
            {
                return NotFound();
            }

            return Ok( result );
        }

        // GET: api/Rings/5
        [HttpGet( "Pinned" )]
        //[Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<IEnumerable<PinnedRing>>> GetPinnedRings()
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );

            var result = (await RingsProvider.GetPinnedRings( userIdForStatuses ));

            if ( result == null )
            {
                return NotFound();
            }

            return Ok( result );
        }

        // GET: api/Rings/5
        [HttpGet( "Byuser/{userId}/Pinned" )]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<IEnumerable<Ring>>> GetPinnedRings( Guid userId )
        {
            var result = (await RingsProvider.GetActivePinnedRings( userId )).Select( r => r.Ring );

            if ( result == null )
            {
                return NotFound();
            }

            return Ok( result );
        }

        // PUT: api/Rings/5
        [HttpPut( "{id}" )]
        [Authorize( Policy = "MustBeAdminApi" )]
        public async Task<ActionResult> PutRing( Guid id, Ring ring )
        {
            await RingsProvider.UpdateAsync( id, ring );

            return NoContent();
        }

        // PUT: api/Rings/5
        [HttpPut( "{id}/requirements" )]
        [Authorize( Policy = "MustBeAdminApi" )]
        public async Task<ActionResult> PutRequirements( Guid id, IEnumerable<RingRequirement> requirements )
        {
            var validationErrors = _validator.ValidateRequirements( requirements );
            if ( validationErrors.Any() )
            {
                return BadRequest( validationErrors.First() );
            }

            await RingsProvider.UpdateRequirementsAsync( id, requirements );

            return NoContent();
        }

        // POST: api/Rings
        [HttpPost]
        [Authorize( Policy = "MustBeAdminApi" )]
        public async Task<ActionResult<Ring>> PostRing( Ring input )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            var ring = await RingsProvider.AddAsync( input );

            return CreatedAtRoute( "DefaultApi", new { id = ring.Id }, ring );
        }

        // DELETE: api/Rings/5
        [HttpDelete]
        [Authorize( Policy = "MustBeAdminApi" )]
        public async Task<ActionResult<Ring>> DeleteRing( Guid id )
        {
            var ring = await RingsProvider.DeleteRingAsync( id );
            if ( ring == null )
            {
                return NotFound();
            }

            return Ok( ring );
        }


        [HttpPost( "Order" )]
        [Authorize( Policy = "MustBeAdminApi" )]
        public async Task<ActionResult<IEnumerable<Ring>>> UpdateRingOrder( IEnumerable<GoalIndexEntry> request )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest();
            }
            var updatedRankOrder = await RingsProvider.UpdateRingOrderAsync( request );
            return Ok( updatedRankOrder );
        }

        [HttpGet( "Image/{id}" )]
        public async Task<ActionResult<Byte[]>> GetRingImage( Guid id )
        {
            try
            {
                var imageDetail = await RingsProvider.GetImage( id );
                return new PhysicalFileResult( imageDetail.FilePath, imageDetail.ContentType ) { FileDownloadName = imageDetail.FileDownloadName };
            }
            catch ( Exception ex )
            {
                if ( !Environment.IsDevelopment() )
                    _logger.LogError( ex, "An unexpected error occurred and the Ring image was not retrieved" );
                return NotFound();
            }
        }

        [HttpPost( "UploadImage/{ringId}" )]
        [Authorize( Policy = "MustBeAdminApi" )]
        [DisableFormValueModelBinding]
        [ProducesResponseType( StatusCodes.Status200OK )]
        [ProducesResponseType( typeof( string ), StatusCodes.Status400BadRequest )]
        public async Task<IActionResult> UploadImage( Guid ringId )
        {
            string? relativePath = null;
            await _multipartFormReader.Read( Request, ModelState, new[] { ".png", ".jpg", ".gif" }, _fileSizeLimit, async requestData =>
            {
                var fileData = requestData.FileData.First();
                var trustedFileNameForFileStorage = ringId.ToString() + fileData.Extension;
                var fileUploadResult = await _fileSystemProvider.SaveFileToContentImages( _webHost.WebRootPath, WarriorsGuildFileType.RingImage, fileData, trustedFileNameForFileStorage );
                relativePath = fileUploadResult.Path;
                await RingsProvider.UploadImageAsync( ringId, fileData.Extension, null, MimeTypeMap.GetMimeType( fileData.Extension ) );
            } );
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            return Created( $@"/{relativePath}", null );
        }

        [HttpPost( "UploadGuide/{ringId}" )]
        [Authorize( Policy = "MustBeAdminApi" )]
        [DisableFormValueModelBinding]
        public async Task<ActionResult> UploadGuide( Guid ringId )  //IFormFileCollection files )??????
        {
            await _multipartFormReader.Read( Request, ModelState, new[] { ".pdf" }, _fileSizeLimit, async requestData =>
            {
                var contentPath = this.Environment.WebRootPath;
                var fileData = requestData.FileData.First();

                var ext = Path.GetExtension( fileData.ContentDisposition.FileName.Value ).ToLowerInvariant();

                await RingsProvider.UploadGuideAsync( ringId, ext, fileData.Content, MimeTypeMap.GetMimeType( ext ) );
                _logger.LogInformation(
                    "Uploaded file '{TrustedFileNameForDisplay}'", fileData.ContentDisposition.FileName.Value );
            } );
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            return Ok();
        }

        //private async Task SaveFileToContentImages( Guid ringId, IFormFile file )
        //{
        //    var contentPath = this.Environment.ContentRootPath;

        //    var path = Path.Combine( contentPath, @"\Images\RingImages" );
        //    if ( !Directory.Exists( path ) )
        //    {
        //        Directory.CreateDirectory( path );
        //    }

        //    var extension = Path.GetExtension( file.FileName );
        //    using ( var stream = new FileStream( Path.Combine( path, ringId + extension ), FileMode.Create ) )
        //    {
        //        await file.CopyToAsync( stream );
        //    }
        //}

        [HttpGet( "Guide/{ringId}" )]
        [AllowAnonymous]
        public async Task<IActionResult> GetGuide( Guid ringId )
        {
            try
            {
                var guideDetail = await RingsProvider.GetGuideAsync( ringId );
                var result = new PhysicalFileResult( guideDetail.FilePath, guideDetail.ContentType );
                result.FileDownloadName = ringId.ToString() + ".pdf";
                return result;
            }
            catch ( Exception ex )
            {
                if ( !Environment.IsDevelopment() )
                    _logger.LogError( ex, "An unexpected error occurred and the Ring guide was not retrieved" );
                return NotFound();
            }
        }

        // PUT: api/Rings/5
        [HttpPost( "Pin/{id}" )]
        public async Task<ActionResult> PinRing( Guid id )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );
            await RingsProvider.PinAsync( id, userIdForStatuses );

            return NoContent();
        }

        // PUT: api/Rings/5
        [HttpPost( "Unpin/{id}" )]
        public async Task<ActionResult> UnpinRing( Guid id )
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );
            await RingsProvider.UnpinAsync( id, userIdForStatuses );

            return NoContent();
        }
    }
}