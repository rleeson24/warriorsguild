using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Models;
using WarriorsGuild.Providers;

namespace WarriorsGuild.Controllers
{
    //[Authorize( Policy = "MustBeSubscriber" )]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SessionManager sessionManager;
        private readonly IUserProvider _userProvider;

        public DashboardController( SessionManager sessionManager, UserManager<ApplicationUser> userManager, IUserProvider userProvider )
        {
            this.sessionManager = sessionManager;
            _userManager = userManager;
            _userProvider = userProvider;
        }

        [Authorize( Roles = "Admin" )]
        public async Task<ActionResult> Warrior()
        {
            return await GetWarriorView();
        }

        // GET: Dashboard
        public async Task<ActionResult> Index()
        {
            if ( User.IsInRole( "Warrior" ) )
            {
                return await GetWarriorView();
            }
            else //if ( User.IsInRole( "Guardian" ) )
            {
                return View( "Guardian", new { } );
            }
        }

        private async Task<ActionResult> GetWarriorView()
        {
            var userIdForStatuses = _userProvider.GetUserIdForStatuses( User );
            var user = await _userManager.Users.Include( u => u.Avatar ).SingleAsync( u => u.Id == userIdForStatuses.ToString() );
            var model = new ProfileViewModel( false, userIdForStatuses.ToString() );
            model.FullName = $"{user.FirstName.Trim()} {user.LastName.Trim()}";
            model.AvatarSrc = user.Avatar != null && user.Avatar.Data.Length > 0 ? $"data:{user.Avatar.ContentType};base64,{Convert.ToBase64String( user.Avatar?.Data )}" : String.Empty;
            model.FavoriteVerse = user.FavoriteVerse;
            model.Hobbies = user.Hobbies;
            model.InterestingFact = user.InterestingFact;
            model.FavoriteMovie = user.FavoriteMovie;
            //model.CurrentAndWorkingRank = RanksProvider.GetMyRank( userIdForStatuses );
            model.PhotoUploaded = (DateTime?)null;
            return View( "Warrior", model );
        }

        public ActionResult IntroAndCovenant()
        {
            return View();
        }

        public ActionResult GuardianIntro()
        {
            return View();
        }
    }
}