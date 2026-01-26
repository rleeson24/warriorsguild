using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using WarriorsGuild.Data.Models;

namespace WarriorsGuild.Helpers.Authentication
{
    public class CustomClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public CustomClaimsFactory( UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> optionsAccessor )
            : base( userManager, roleManager, optionsAccessor )
        { }
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync( ApplicationUser user )
        {
            var identity = await base.GenerateClaimsAsync( user );
            identity.AddClaim( new Claim( ClaimTypes.GivenName, user.FirstName + " " + user.LastName ) );
            if ( !identity.HasClaim( x => x.Type == JwtClaimTypes.Subject ) )
            {
                var sub = identity.Claims.FirstOrDefault( c => c.Type == ClaimTypes.NameIdentifier )?.Value;
                if ( sub != null )
                {
                    identity.AddClaim( new Claim( JwtClaimTypes.Subject, sub ) );
                }
            }
            return identity;
        }
    }
}
