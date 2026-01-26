using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WarriorsGuild.Data.Models;

namespace WarriorsGuild.Helpers.Authentication
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileService( UserManager<ApplicationUser> userManager )
        {
            this._userManager = userManager;
        }

        public async Task GetProfileDataAsync( ProfileDataRequestContext context )
        {
            context.IssuedClaims.AddRange( context.Subject.Claims );

            var user = await _userManager.GetUserAsync( context.Subject );

            var roles = await _userManager.GetRolesAsync( user );

            foreach ( var role in roles )
            {
                context.IssuedClaims.Add( new Claim( JwtClaimTypes.Role, role ) );
            }
        }

        public async Task IsActiveAsync( IsActiveContext context )
        {
            await Task.CompletedTask;
        }
    }
}
