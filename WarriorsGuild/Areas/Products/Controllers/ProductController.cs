using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarriorsGuild.DataAccess;
using WarriorsGuild.DataAccess.Models;
using WarriorsGuild.Email;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Providers;
using static IdentityServer4.IdentityServerConstants;

namespace WarriorsGuild.Areas.Products.Controllers
{
    [ApiController]
    [Route( "api/[controller]" )]
    [Authorize( LocalApi.PolicyName )]
    [IgnoreAntiforgeryToken]
    public class ProductController : ControllerBase
    {
        private readonly IGuildDbContext _dbContext;

        private readonly IHelpers Helpers;
        private readonly IEmailProvider EmailProvider;
        private IUserProvider _userProvider;

        public ProductController( IGuildDbContext dbContext, IHelpers helpers, IEmailProvider emailProvider, IUserProvider userProvider )
        {
            _dbContext = dbContext;
            Helpers = helpers;
            EmailProvider = emailProvider;
            _userProvider = userProvider;
        }

        // DELETE: api/Product/5
        [HttpDelete]
        public async Task<ActionResult> Delete( string emailAddress )
        {
            var entry = _dbContext.MailingList.Where( e => e.EmailAddress.ToLower() == emailAddress.ToLower() );
            _dbContext.Entry( entry ).CurrentValues.SetValues( new { EmailAddress = emailAddress.ToLower() } );
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPost( "Invite" )]
        public async Task<ActionResult> Invite( string emailAddress )
        {
            var myUserId = _userProvider.GetMyUserId( User );
            var entries = _dbContext.Set<InvitedEmailAddress>();
            if ( !Helpers.IsValidEmail( emailAddress ) )
            {
                return BadRequest( "Email address is not in a valid format" );
            }
            if ( entries.Any( e => e.EmailAddress.ToLower() == emailAddress.ToLower() ) )
            {
                return Conflict();
            }
            var registerUrl = String.Format( "{0}://{1}{2}", Request.Scheme, Request.Host, Url.Content( "~/Account/Register" ) );
            await EmailProvider.SendAsync( "How exciting!!!", @"Step #1  Please visit https://warriorsguildtest.azurewebsites.net/Account/Register to register your account.

Step #2  Check us out on Facebook, like our page, and join our ""beta trial"" private group https://www.facebook.com/groups/834717263852542.

Step #3  Head over to YouTube and subscribe to our channel https://www.youtube.com/channel/UCRVm4nyzYf-IZOj574pqbdg.

Questions: Reach out via Facebook Messenger or shoot us an email at beta@warriorsguild.com with any questions or issues.

Resources:
-Intro Video Link: https://youtu.be/0Xlju4kjeGw", emailAddress.ToLower(), EmailView.Invite );
            entries.Add( new InvitedEmailAddress() { EmailAddress = emailAddress, InvitedBy = myUserId, InvitedAt = DateTime.Now } );
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
