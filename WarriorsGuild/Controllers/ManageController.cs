using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Email;
using WarriorsGuild.Helpers.Filters;
using WarriorsGuild.Models;
using WarriorsGuild.Models.Account;
using WarriorsGuild.Providers;
using WarriorsGuild.Providers.Payments;

namespace WarriorsGuild.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private SignInManager<ApplicationUser> signInManager;
        private readonly ICustomerManager customerMgr;
        private readonly IEmailProvider emailProvider;
        private readonly IConfiguration _configuration;
        private UserManager<ApplicationUser> userManager;
        private IUserProvider _userProvider;

        public ManageController( UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ICustomerManager customerMgr, IEmailProvider emailProvider, IConfiguration configuration, IUserProvider userProvider )
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.customerMgr = customerMgr;
            this.emailProvider = emailProvider;
            this._configuration = configuration;
            _userProvider = userProvider;
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index( ManageMessageId message )
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var myUserId = _userProvider.GetMyUserId( User );
            var user = await userManager.FindByIdAsync( myUserId.ToString() );
            var model = new IndexViewModel
            {
                HasPassword = await HasPasswordAsync(),
                PhoneNumber = await userManager.GetPhoneNumberAsync( user ),
                TwoFactor = await userManager.GetTwoFactorEnabledAsync( user ),
                Logins = await userManager.GetLoginsAsync( user ),
                //BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync( userId )
            };
            return View( model );
        }

        [Route( "EditProfile" )]
        [Route( "EditProfile/{profileId}" )]
        [HttpGet]
        public ActionResult EditProfile( Guid? profileId )
        {
            var profileUserId = _userProvider.GetMyUserId( User );
            if ( profileId.HasValue )
            {
                profileUserId = profileId.Value;
            }
            var user = userManager.Users.Include( u => u.Avatar ).First( u => u.Id == profileUserId.ToString() );

            return View( new EditProfileViewModel
            {
                User = new UserProfile()
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
                    State = user.State,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    UserName = user.UserName,
                    FavoriteVerse = user.FavoriteVerse,
                    Hobbies = user.Hobbies,
                    InterestingFact = user.InterestingFact,
                    FavoriteMovie = user.FavoriteMovie,
                    Avatar = user.Avatar != null && user.Avatar.Data.Length > 0 ? Convert.ToBase64String( user.Avatar.Data ) : String.Empty,
                    AvatarContentType = user.Avatar?.ContentType,
                    BirthDate = user.BirthDate
                },
                Urls = new EditProfileUrls
                {
                    GetProfile = "/api/Profile",
                    AvatarUploadUrl = "/api/Profile/UploadAvatar",
                    GetAvatarUrl = $"/api/Profile/{user.Id}/Avatar"
                },
                StripePublishableKey = _configuration[ "Stripe:PublishedKey" ] as String
            } );
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        public async Task<ActionResult> RemoveLogin( string loginProvider, string providerKey )
        {
            ManageMessageId? message;
            var myUserId = _userProvider.GetMyUserId( User );
            var user = await userManager.FindByIdAsync( myUserId.ToString() );
            var result = await userManager.RemoveLoginAsync( user, loginProvider, providerKey );
            if ( result.Succeeded )
            {
                if ( user != null )
                {
                    await signInManager.SignInAsync( user, isPersistent: false );
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction( "ManageLogins", new { Message = message } );
        }

        //
        // GET: /Manage/AddPhoneNumber
        [HttpPost]
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        public async Task<ActionResult> AddPhoneNumber( AddPhoneNumberViewModel model )
        {
            if ( !ModelState.IsValid )
            {
                return View( model );
            }
            // Generate the token and send it

            var myUserId = _userProvider.GetMyUserId( User );
            var user = await userManager.FindByIdAsync( myUserId.ToString() );
            var code = await userManager.GenerateChangePhoneNumberTokenAsync( user, model.Number );
            //if ( signInManager.SmsService != null )
            //{
            //    var message = new IdentityMessage
            //    {
            //        Destination = model.Number,
            //        Body = "Your security code is: " + code
            //    };
            //    await userManager.SmsService.SendAsync( message );
            //}
            return RedirectToAction( "VerifyPhoneNumber", new { PhoneNumber = model.Number } );
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            var myUserId = _userProvider.GetMyUserId( User );
            var user = await userManager.FindByIdAsync( myUserId.ToString() );
            await userManager.SetTwoFactorEnabledAsync( user, true );
            if ( user != null )
            {
                await signInManager.SignInAsync( user, isPersistent: false );
            }
            return RedirectToAction( "Index", "Manage" );
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            var myUserId = _userProvider.GetMyUserId( User );
            var user = await userManager.FindByIdAsync( myUserId.ToString() );
            await userManager.SetTwoFactorEnabledAsync( user, false );
            if ( user != null )
            {
                await signInManager.SignInAsync( user, isPersistent: false );
            }
            return RedirectToAction( "Index", "Manage" );
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber( string phoneNumber )
        {
            var myUserId = _userProvider.GetMyUserId( User );
            var user = await userManager.FindByIdAsync( myUserId.ToString() );
            var code = await userManager.GenerateChangePhoneNumberTokenAsync( user, phoneNumber );
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View( "Error" ) : View( new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber } );
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        public async Task<ActionResult> VerifyPhoneNumber( VerifyPhoneNumberViewModel model )
        {
            if ( !ModelState.IsValid )
            {
                return View( model );
            }
            var myUserId = _userProvider.GetMyUserId( User );
            var user = await userManager.FindByIdAsync( myUserId.ToString() );
            var result = await userManager.ChangePhoneNumberAsync( user, model.PhoneNumber, model.Code );
            if ( result.Succeeded )
            {
                if ( user != null )
                {
                    await signInManager.SignInAsync( user, isPersistent: false );
                }
                return RedirectToAction( "Index", new { Message = ManageMessageId.AddPhoneSuccess } );
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError( "", "Failed to verify phone" );
            return View( model );
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var myUserId = _userProvider.GetMyUserId( User );
            var user = await userManager.FindByIdAsync( myUserId.ToString() );
            var result = await userManager.SetPhoneNumberAsync( user, null );
            if ( !result.Succeeded )
            {
                return RedirectToAction( "Index", new { Message = ManageMessageId.Error } );
            }
            if ( user != null )
            {
                await signInManager.SignInAsync( user, isPersistent: false );
            }
            return RedirectToAction( "Index", new { Message = ManageMessageId.RemovePhoneSuccess } );
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        public async Task<ActionResult> ChangePassword( ChangePasswordViewModel model )
        {
            if ( !ModelState.IsValid )
            {
                return View( model );
            }
            var myUserId = _userProvider.GetMyUserId( User );
            var user = await userManager.FindByIdAsync( myUserId.ToString() );
            var result = await userManager.ChangePasswordAsync( user, model.OldPassword, model.NewPassword );
            if ( result.Succeeded )
            {
                if ( user != null )
                {
                    await signInManager.SignInAsync( user, isPersistent: false );
                }
                await emailProvider.SendAsync( "Warrior's Guild - Password Changed", null, user.Email, EmailView.PasswordResetConfirmation );
                return RedirectToAction( "Index", "Manage", new { Message = ManageMessageId.ChangePasswordSuccess } );
            }
            AddErrors( result );
            return View( model );
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        public async Task<ActionResult> SetPassword( SetPasswordViewModel model )
        {
            if ( ModelState.IsValid )
            {
                var myUserId = _userProvider.GetMyUserId( User );
                var user = await userManager.FindByIdAsync( myUserId.ToString() );
                var result = await userManager.AddPasswordAsync( user, model.NewPassword );
                if ( result.Succeeded )
                {
                    if ( user != null )
                    {
                        await signInManager.SignInAsync( user, isPersistent: false );
                    }
                    return RedirectToAction( "Index", new { Message = ManageMessageId.SetPasswordSuccess } );
                }
                AddErrors( result );
            }

            // If we got this far, something failed, redisplay form
            return View( model );
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins( ManageMessageId message )
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var myUserId = _userProvider.GetMyUserId( User );
            var user = await userManager.FindByIdAsync( myUserId.ToString() );
            if ( user == null )
            {
                return View( "Error" );
            }
            var userLogins = await userManager.GetLoginsAsync( user );
            //var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where( auth => userLogins.All( ul => auth.AuthenticationType != ul.LoginProvider ) ).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View( new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                //OtherLogins = otherLogins
            } );
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        public ActionResult LinkLogin( string provider )
        {
            var myUserId = _userProvider.GetMyUserId( User );
            // Request a redirect to the external login provider to link a login for the current user
            return Challenge( provider, Url.Action( "LinkLoginCallback", "Manage" )!, myUserId.ToString() );
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            //var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync( XsrfKey, User.FindFirstValue( claimType: "sub" ) );
            //if ( loginInfo == null )
            //{
            return RedirectToAction( "ManageLogins", new { Message = ManageMessageId.Error } );
            //}
            //var result = await userManager.AddLoginAsync( User.FindFirstValue( claimType: "sub" ), loginInfo.Login );
            //return result.Succeeded ? RedirectToAction( "ManageLogins" ) : RedirectToAction( "ManageLogins", new { Message = ManageMessageId.Error } );
        }

        //
        // GET: /Manage/LinkLoginCallback
        public ActionResult ManageProfiles()
        {
            return View( new ProfileUrls
            {
                MeUrl = "/api/Profile",
                RegistrationUrl = Url.Action( "Register", "Account" )
            } );
        }

        //
        // GET: /Manage/LinkLoginCallback
        [Authorize( Roles = "Admin, Guardian" )]
        [TypeFilter( typeof( OnlyUnsubscribedOrPayingParty ) )]
        public ActionResult Subscription( string id )
        {
            var viewModel = new ManageSubscriptionViewModel();
            viewModel.StripePublishableKey = _configuration[ "Stripe:PublishedKey" ] as String;
            viewModel.Urls = new PaymentUrls
            {
                PriceOptionsUrl = "/api/PriceOptions/ForSubscription",
                SubscriptionsUrl = "/api/Subscription"
            };
            return View( viewModel );
        }

        //public ActionResult UpdatePaypalSubscription( string? token )
        //{
        //	var transactionCancelled = !String.IsNullOrEmpty( Request.QueryString[ "cancel" ] );
        //	if ( transactionCancelled )
        //	{
        //		ViewBag.WarningMessage = "Transaction was cancelled";
        //	}
        //	else if ( token != null )
        //	{
        //		if ( string.IsNullOrEmpty( token ) )
        //		{
        //			EmailProvider.RenderEmailViewToString( EmailView.Generic, new { Message = "token was empty", Token = token, UserId = User.FindFirstValue( claimType: "sub" ) } );
        //			ViewBag.ErrorMessage = "Unable to process token";
        //		}
        //		var db = new DataAccess.GuildDbContext();
        //		var billingAgreement = db.BillingAgreements.FirstOrDefault( ba => ba.Token == token );
        //		if ( billingAgreement == null )
        //		{
        //			EmailProvider.RenderEmailViewToString( EmailView.Generic, new { Message = "Billing agreement was null", Token = token, UserId = User.FindFirstValue( claimType: "sub" ) } );
        //			ViewBag.ErrorMessage = "Sorry, the specified billing agreement was not found.";
        //		}
        //		if ( billingAgreement.UserId != new Guid( User.FindFirstValue( claimType: "sub" ) ) )
        //		{
        //			EmailProvider.RenderEmailViewToString( EmailView.Generic, new { Message = "Billing agreement does not apply to this user", BillingAgreement = billingAgreement, Token = token, UserId = User.FindFirstValue( claimType: "sub" ) } );
        //			ViewBag.ErrorMessage = "It appears the wires got crossed.  Please retry the subscription process.";
        //		}

        //		try
        //		{
        //			new PaypalPaymentProvider().ExecuteBillingAgreement( token );
        //		}
        //		catch ( Exception ex )
        //		{
        //			EmailProvider.RenderEmailViewToString( EmailView.Generic, new { Message = "Exception occurred", Exception = ex, BillingAgreement = billingAgreement, Token = token, UserId = User.FindFirstValue( claimType: "sub" ) } );
        //			ViewBag.ErrorMessage = "An unhandled error occurred.  Please try again";
        //		}

        //		billingAgreement.Status = Areas.Payments.Models.BillingAgreementStatus.Accepted;

        //		db.SaveChangesAsync();

        //		ViewBag.SuccessMessage = "Your payment was successful";
        //	}
        //	var viewModel = new ManageSubscriptionViewModel()
        //	{
        //		StripePublishableKey = ConfigurationManager.AppSettings[ "StripePublishedKey" ] as String,
        //		Urls = new PaymentUrls
        //		{
        //			PriceOptionsUrl = "/api/PriceOptions/ForSubscription",
        //			SubscriptionsUrl = "/api/Subscriptions"
        //		}
        //	};
        //	return View( viewModel );
        //}

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private void AddErrors( IdentityResult result )
        {
            foreach ( var error in result.Errors )
            {
                ModelState.AddModelError( "", error.Description );
            }
        }

        private async Task<bool> HasPasswordAsync()
        {
            var myUserId = _userProvider.GetMyUserId( User );
            var user = await userManager.FindByIdAsync( myUserId.ToString() );
            if ( user != null )
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private async Task<bool> HasPhoneNumber()
        {
            var myUserId = _userProvider.GetMyUserId( User );
            var user = await userManager.FindByIdAsync( myUserId.ToString() );
            if ( user != null )
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            None,
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        #endregion
    }
}