using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Models;
using WarriorsGuild.Providers;

namespace WarriorsGuild.Controllers
{
    public class ProfileController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private IUserProvider _userProvider;
        private readonly SessionManager sessionManager;

        public ProfileController( UserManager<ApplicationUser> userManager, IUserProvider userProvider, SessionManager sessionManager )
        {
            _userManager = userManager;
            _userProvider = userProvider;
            this.sessionManager = sessionManager;
        }

        // GET: Profile
        [HttpGet( "Profile/{id}" )]
        public async Task<IActionResult> Index( Guid id )
        {
            var user = await _userManager.Users.Include( u => u.Avatar ).SingleAsync( u => u.Id == id.ToString() );
            var model = new ProfileViewModel( true, id.ToString() );
            model.FullName = $"{user.FirstName.Trim()} {user.LastName.Trim()}";
            model.AvatarSrc = user.Avatar != null && user.Avatar.Data.Length > 0 ? $"data:{user.Avatar.ContentType};base64,{Convert.ToBase64String( user.Avatar.Data )}" : string.Empty;
            model.FavoriteVerse = user.FavoriteVerse;
            model.Hobbies = user.Hobbies;
            model.InterestingFact = user.InterestingFact;
            model.FavoriteMovie = user.FavoriteMovie;
            model.PhotoUploaded = null;
            var myUserId = _userProvider.GetMyUserId( User );
            var myUser = await _userManager.FindByIdAsync( myUserId.ToString() );
            if ( myUser != null && myUser.ChildUsers.Any( c => c.Id == id.ToString() ) )
            {
                sessionManager.UserIdForStatuses = id.ToString();
            }

            return View( model );
        }

        //[Route( "Avatar" )]
        //public async Task<ActionResult<Product>> GetAvatar()
        //{
        //    if ( !this.Request.Content.IsMimeMultipartContent() )
        //    {
        //        throw new HttpResponseException( HttpStatusCode.UnsupportedMediaType );
        //    }
        //    var userId = User.FindFirstValue( claimType: "sub" );
        //    var user = UserManager.Users.Include( u => u.Avatar ).Single( u => u.Id == userId );
        //    var tmpPath = Path.GetTempFileName();
        //    File.WriteAllBytes( tmpPath, user.Avatar.Data );

        //    return new WarriorsGuild.Models.FileResult( tmpPath, null, user.Avatar.ContentType );
        //}
    }
}