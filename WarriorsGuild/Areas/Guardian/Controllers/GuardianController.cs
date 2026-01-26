using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarriorsGuild.Areas.Payments;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Models;
using WarriorsGuild.Models.Guardian;
using WarriorsGuild.Providers;
using static IdentityServer4.IdentityServerConstants;

namespace WarriorsGuild.Areas.Guardian.Controllers
{
    [Authorize]
    [ApiController]
    [Route( "api/[controller]" )]
    [Authorize( LocalApi.PolicyName )]
    [IgnoreAntiforgeryToken]
    public class GuardianController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private readonly IUserProvider _userProvider;
        private readonly IGuardianIntroProvider guardianIntroProvider;

        private ISubscriptionManager SubscriptionManager { get; }

        public GuardianController( IHttpContextAccessor _httpContextAccessor, UserManager<ApplicationUser> userMgr, IUserProvider userProvider, ISubscriptionManager subscriptionManager, IGuardianIntroProvider guardianIntroProvider )
        {
            _userManager = userMgr;
            this._userProvider = userProvider;
            SubscriptionManager = subscriptionManager;
            this.guardianIntroProvider = guardianIntroProvider;
        }

        [HttpGet( "Warriors" )]
        public async Task<ActionResult<IEnumerable<WarriorViewModel>>> GetWarriors()
        {
            var vm = new List<WarriorViewModel>();
            var myUserId = _userProvider.GetMyUserId( User );
            var childUsers = await _userProvider.GetChildUsers( myUserId.ToString() );
            foreach ( var w in childUsers )
            {
                var avatar = (await _userManager.Users.Include( u => u.Avatar ).FirstAsync( u => u.Id == w.Id )).Avatar;
                if ( await _userManager.IsInRoleAsync( w, "Warrior" ) )
                {
                    vm.Add( new WarriorViewModel()
                    {
                        Id = w.Id,
                        Name = $"{w.FirstName} {w.LastName}",
                        Username = w.UserName,
                        AvatarSrc = avatar != null && avatar.Data.Length > 0 ? $"data:{avatar.ContentType};base64,{Convert.ToBase64String( avatar?.Data )}" : String.Empty
                    } );
                }
            };
            return Ok( vm );
        }

        [Authorize( Policy = "MustBeSubscriber" )]
        [HttpGet( "CanAddWarrior" )]
        public async Task<ActionResult<Boolean>> GetCanAddWarrior()
        {
            var canAdd = true;
            var myUserId = _userProvider.GetMyUserId( User );
            //var mySub = await SubscriptionManager.GetMySubscriptionAsync( myUserId.ToString() );
            //if ( mySub == null )
            //{
            //    return BadRequest( "No Subscription found" );
            //}
            var myUser = await _userManager.Users.Include( u => u.ChildUsers ).FirstAsync( u => u.Id == myUserId.ToString() );
            var warriorsInSession = myUser.ChildUsers;
            //var warriorsInSession = (await SubscriptionManager.GetUsersOnSubscriptionAsync( mySub.BillingAgreement.Id )).Where( u => u.Role == Payments.Models.UserRole.Warrior ).ToArray();
            var maxWarriors = 5; // mySub.BillingAgreement.AdditionalWarriors + mySub.BillingAgreement.PriceOption.NumberOfWarriors;
            if ( warriorsInSession.Count() == maxWarriors )
            {
                canAdd = false;
            }
            return Ok( canAdd );
        }

        [HttpGet( "summary" )]
        public async Task<ActionResult<GuardianSummaryViewModel>> GetSummary()
        {
            var myUserId = _userProvider.GetMyUserId( User );
            var viewModel = new GuardianSummaryViewModel();
            var myUser = await _userManager.Users.Include( u => u.Avatar ).FirstAsync( u => u.Id == myUserId.ToString() );
            viewModel.Id = myUser.Id;
            viewModel.Name = $"{myUser.FirstName} {myUser.LastName}";
            viewModel.Username = myUser.UserName;
            viewModel.HasAvatar = myUser.Avatar != null;
            var sub = await SubscriptionManager.GetMySubscriptionAsync( myUserId.ToString() ); ;
            viewModel.SubscriptionDescription = sub?.BillingAgreement?.PriceOption?.Description;
            viewModel.SubscriptionExpires = sub?.BillingAgreement?.NextPaymentDue ?? DateTime.Now;
            return viewModel;
        }

        [HttpPost( "confirmVideoWatched" )]
        public async Task<IActionResult> ConfirmVideoWatched()
        {
            var myUserId = _userProvider.GetMyUserId( User );

            await guardianIntroProvider.ConfirmVideoWatched( myUserId );
            return Ok();
        }
    }
}
