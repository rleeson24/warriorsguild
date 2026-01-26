using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarriorsGuild.Providers;
using static IdentityServer4.IdentityServerConstants;

namespace WarriorsGuild.Areas.Warriors.Controllers
{
    [ApiController]
    [Route( "api/[controller]" )]
    [Authorize( LocalApi.PolicyName )]
    [IgnoreAntiforgeryToken]
    public class WarriorController : ControllerBase
    {
        private readonly ICovenantProvider covenantProvider;
        private IUserProvider _userProvider;

        public WarriorController( ICovenantProvider covenantProvider, IUserProvider userProvider )
        {
            this.covenantProvider = covenantProvider;
            _userProvider = userProvider;
        }

        [HttpPost( "signcovenant" )]
        public async Task<IActionResult> SignCovenant( [FromBody] string name )
        {
            var myUserId = _userProvider.GetMyUserId( User );

            await covenantProvider.SignCovenant( myUserId, name );
            return Ok();
        }
    }
}
