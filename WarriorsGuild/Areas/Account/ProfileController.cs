using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Account;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Helpers.Utilities.Files;
using WarriorsGuild.Models;
using WarriorsGuild.Models.Account;
using WarriorsGuild.Providers;

namespace WarriorsGuild.Areas.Account
{
    [Authorize]
    [Route( "api/Profile" )]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        public readonly UserManager<ApplicationUser> UserManager;
        public readonly IUserProvider _userProvider;

        public readonly ApplicationDbContext AppDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SessionManager sessionManager;
        private readonly IWebHostEnvironment Environment;
        private readonly ILogger<ProfileController> _logger;

        private IMultipartFormReader _multipartFormReader { get; }
        private readonly long _fileSizeLimit;

        public ProfileController( UserManager<ApplicationUser> userManager, IMultipartFormReader multipartReader, IUserProvider userProvider, ApplicationDbContext dbContext,
                            IHttpContextAccessor httpContextAccessor, SessionManager sessionManager, IWebHostEnvironment environment, IConfiguration config, ILogger<ProfileController> logger )
        {
            UserManager = userManager;
            _multipartFormReader = multipartReader;
            AppDbContext = dbContext;
            this._httpContextAccessor = httpContextAccessor;
            this.sessionManager = sessionManager;
            this.Environment = environment;
            this._logger = logger;
            this._userProvider = userProvider;
            _fileSizeLimit = config.GetValue<long>( "FileSizeLimit" );
        }

        // GET api/Profile
        [HttpGet]
        public MeViewModel Get()
        {
            var myUserId = _userProvider.GetMyUserId( User );
            var user = AppDbContext.Users.Include( u => u.Avatar ).Where( u => u.Id == myUserId.ToString() ).Include( u => u.ChildUsers ).First();
            var viewModel = new MeViewModel();
            viewModel.Me = MapUserToProfile( user );
            viewModel.ChildUsers = user.ChildUsers?.Select( MapUserToProfile ).ToArray() ?? new UserProfile[ 0 ];
            return viewModel;
        }

        private UserProfile MapUserToProfile( ApplicationUser user )
        {
            return new UserProfile()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                AddressLine1 = user.AddressLine1,
                AddressLine2 = user.AddressLine2,
                City = user.City,
                Email = user.Email,
                Id = user.Id,
                PhoneNumber = user.PhoneNumber,
                PostalCode = user.PostalCode,
                ShirtSize = user.ShirtSize,
                FavoriteVerse = user.FavoriteMovie,
                Hobbies = user.Hobbies,
                InterestingFact = user.InterestingFact,
                FavoriteMovie = user.FavoriteMovie,
                State = user.State,
                TwoFactorEnabled = user.TwoFactorEnabled,
                UserName = user.UserName,
                Avatar = (user.Avatar != null ? Convert.ToBase64String( user.Avatar.Data ) : null),
                AvatarContentType = user.Avatar?.ContentType,
                BirthDate = user.BirthDate
            };
        }

        [HttpPost]
        public async Task<ActionResult> PostMe( [FromForm] EditProfileViewModel model )
        {
            var myUserId = _userProvider.GetMyUserId( User );
            if ( model.User.Id != myUserId.ToString() )
            {
                return BadRequest();
            }

            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            var user = await UserManager.FindByIdAsync( model.User.Id );
            var currentEmailAddress = user.Email;
            user.FirstName = model.User.FirstName;
            user.LastName = model.User.LastName;
            user.AddressLine1 = model.User.AddressLine1;
            user.AddressLine2 = model.User.AddressLine2;
            user.City = model.User.City;
            user.State = model.User.State;
            user.PostalCode = model.User.PostalCode;
            user.PhoneNumber = model.User.PhoneNumber;
            user.ShirtSize = model.User.ShirtSize;
            user.UserName = model.User.UserName;
            user.Email = model.User.Email;
            user.FavoriteVerse = model.User.FavoriteVerse;
            user.Hobbies = model.User.Hobbies;
            user.InterestingFact = model.User.InterestingFact;
            user.FavoriteMovie = model.User.FavoriteMovie;
            user.BirthDate = model.User.BirthDate;

            if ( currentEmailAddress != model.User.Email )
            {
                if ( model.User.EmailConfirmed == model.User.Email )
                {
                    if ( model.User.Email != null && await UserManager.FindByEmailAsync( model.User.Email ) != null )
                    {
                        ModelState.AddModelError( "Email", $"An account already exists with email {model.User.Email}." );
                        return BadRequest( ModelState );
                    }
                    user.EmailConfirmed = false;
                }
                else
                {
                    return BadRequest( "The Email Address and its confirmation must match" );
                }
            }

            await UserManager.UpdateAsync( user );

            if ( user.Email != currentEmailAddress )
            {
                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                var code = await UserManager.GenerateEmailConfirmationTokenAsync( user );
                var callbackUrl = Url.Link( "Default", new { Controller = "Account", Action = "ConfirmEmail", userId = user.Id, code = code } );
                //await UserManager.SendEmailAsync( user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>" );
            }
            return Ok();
        }

        // GET api/Profile
        [HttpPost( "SetActiveWarrior" )]
        public ActionResult SetActiveWarrior( [FromBody] string id )
        {
            if ( id != null && id != "undefined" )
            {
                var currentUser = Get();
                if ( currentUser.ChildUsers.Any( c => c.Id == id ) )
                {
                    sessionManager.UserIdForStatuses = id;
                    return Ok();
                }
                else
                {
                    return BadRequest( "User not found" );
                }
            }
            else
            {
                sessionManager.UserIdForStatuses = null;
                return Ok();
            }
        }

        // GET api/Profile
        [HttpPost( "TogglePreviewMode" )]
        public ActionResult TogglePreviewMode()
        {
            sessionManager.PreviewMode = !sessionManager.PreviewMode;
            return Ok();
        }

        [HttpPost( "UploadAvatar" )]
        [Authorize]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult<Byte[]>> UploadAvatar()
        {
            var myUserId = _userProvider.GetMyUserId( User );
            var user = await UserManager.Users.Include( u => u.Avatar ).SingleAsync( u => u.Id == myUserId.ToString() );
            await _multipartFormReader.Read( Request, ModelState, new[] { ".png", ".jpg", ".gif" }, _fileSizeLimit, async requestData =>
            {
                var contentPath = this.Environment.WebRootPath;

                var updateAvatar = user.Avatar != null;
                var fileData = requestData.FileData.First();
                var ext = Path.GetExtension( fileData.ContentDisposition.FileName.Value ).ToLowerInvariant();
                user.Avatar = user.Avatar ?? new AvatarDetail();
                user.Avatar.UserId = myUserId;
                user.Avatar.Data = fileData.Content;
                user.Avatar.Extension = ext;
                user.Avatar.ContentType = MimeTypeMap.GetMimeType( fileData.Extension );
                AppDbContext.Entry<AvatarDetail>( user.Avatar ).State = updateAvatar ? EntityState.Modified : EntityState.Added;
                await AppDbContext.SaveChangesAsync();

                _logger.LogInformation(
                    "Uploaded file '{TrustedFileNameForDisplay}'", fileData.ContentDisposition.FileName.Value );
            } );
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            var fileResult = new UploadResult();
            fileResult.FileData = Convert.ToBase64String( user.Avatar.Data );
            fileResult.ContentType = user.Avatar.ContentType;
            return Ok( fileResult );
        }



        [HttpGet( "{userId}/Avatar" )]
        [Authorize]
        [IgnoreAntiforgeryToken]
        public async Task<FileContentResult> DownloadAvatar( string userId )
        {
            var user = await UserManager.Users.Include( u => u.Avatar ).SingleAsync( u => u.Id == userId );
            if ( user.Avatar != null )
            {
                return new FileContentResult( user.Avatar.Data, user.Avatar.ContentType );
            }
            else
            {
                return null;
            }
        }
    }
}