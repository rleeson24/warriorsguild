using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Models;

namespace WarriorsGuild.Areas.Profile.Controllers
{
    [Route( "api/[controller]" )]
    public class ProfileController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;

        public ProfileController( UserManager<ApplicationUser> userManager )
        {
            _userManager = userManager;
        }

        // GET: api/Rings
        [HttpGet( "{id}" )]
        public async Task<ProfileViewModel> Get( string id )
        {
            var user = await _userManager.Users.Include( u => u.Avatar ).SingleAsync( u => u.Id == id.ToString() );
            var model = new ProfileViewModel( true, id.ToString() );
            model.FullName = $"{user.FirstName.Trim()} {user.LastName.Trim()}";
            model.AvatarSrc = user.Avatar != null && user.Avatar.Data.Length > 0 ? $"data:{user.Avatar.ContentType};base64,{Convert.ToBase64String( user.Avatar.Data )}" : String.Empty;
            model.FavoriteVerse = user.FavoriteVerse;
            model.Hobbies = user.Hobbies;
            model.InterestingFact = user.InterestingFact;
            model.FavoriteMovie = user.FavoriteMovie;
            model.PhotoUploaded = (DateTime?)null;
            return model;
        }
    }
}
