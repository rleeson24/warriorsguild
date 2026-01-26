using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Models.Payments;
using WarriorsGuild.Models.Payments.PaymentMethods;
using WarriorsGuild.Providers;
using WarriorsGuild.Providers.Payments;
using static IdentityServer4.IdentityServerConstants;

namespace WarriorsGuild.Areas.Payments.Controllers
{
    [ApiController]
    [Route( "api/[controller]" )]
    [Authorize( LocalApi.PolicyName )]
    [IgnoreAntiforgeryToken]
    public class PaymentMethodsController : ControllerBase
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IUserProvider _userProvider;
        private UserManager<ApplicationUser> _userManager;
        private UserManager<ApplicationUser> UserManager
        {
            get
            {
                return _userManager;
            }
        }

        public ICustomerManager CustomerMgr { get; }

        public PaymentMethodsController( UserManager<ApplicationUser> userManager, ICustomerManager customerMgr, IHttpContextAccessor httpContextAccessor, IUserProvider userProvider )
        {
            _userManager = userManager;
            CustomerMgr = customerMgr;
            _httpContextAccessor = httpContextAccessor;
            this._userProvider = userProvider;
        }

        [HttpGet]
        public async Task<ActionResult<PaymentMethodViewModel>> Get()
        {
            var myUserId = _userProvider.GetMyUserId( User );
            var user = await UserManager.FindByIdAsync( myUserId.ToString() );
            var viewModel = new PaymentMethodViewModel();
            if ( !String.IsNullOrEmpty( user?.StripeCustomerId ) )
            {
                var paymentMethodsForUser = await CustomerMgr.GetPaymentMethods( user.StripeCustomerId );
                viewModel.PaymentMethods = paymentMethodsForUser.Select( p => MapToViewModel( p ) ).ToArray();
            }
            return Ok( viewModel );
        }

        [HttpGet( "{id}" )]
        public async Task<ActionResult<PaymentMethodViewModelItem>> Get( string id )
        {
            var myUserId = _userProvider.GetMyUserId( User );
            var user = await UserManager.FindByIdAsync( myUserId.ToString() );
            PaymentMethodViewModelItem vm = null;
            if ( !String.IsNullOrEmpty( user?.StripeCustomerId ) )
            {
                var paymentMethodsForUser = await CustomerMgr.GetPaymentMethods( user.StripeCustomerId );
                vm = MapToViewModel( paymentMethodsForUser.Where( p => p.Id == id ).FirstOrDefault() );
            }
            return Ok( vm );
        }

        [HttpPost]
        [Authorize( Roles = "Admin, Guardian" )]
        public async Task<ActionResult<Product>> Post( string token )
        {
            try
            {
                var myUserId = _userProvider.GetMyUserId( User );
                var user = await UserManager.FindByIdAsync( myUserId.ToString() );
                var result = await CustomerMgr.AddPaymentMethod( user, token, UserManager );
                return Ok( MapToViewModel( result ) );
            }
            catch ( StripeException ex )
            {
                return BadRequest( ex.Message );
            }
        }

        [HttpDelete]
        public async Task<ActionResult<Product>> Delete( string id )
        {
            var myUserId = _userProvider.GetMyUserId( User );
            var user = await UserManager.FindByIdAsync( myUserId.ToString() );
            if ( !String.IsNullOrEmpty( user.StripeCustomerId ) )
            {
                var paymentMethodsForUser = await CustomerMgr.GetPaymentMethods( user.StripeCustomerId );
                if ( paymentMethodsForUser.Count() == 0 )
                {
                    return NotFound();
                }
                else if ( paymentMethodsForUser.Count() == 1 )
                {
                    return BadRequest( "You cannot remove your only payment method" );
                }
                else
                {
                    await CustomerMgr.DeletePaymentMethod( user.StripeCustomerId, id );
                }
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut( "{id}" )]
        public async Task<ActionResult<Product>> MakeDefault( string id )
        {
            try
            {
                var myUserId = _userProvider.GetMyUserId( User );
                var user = await UserManager.FindByIdAsync( myUserId.ToString() );
                await CustomerMgr.SetDefaultPaymentMethod( user.StripeCustomerId, id );
                return Ok();
            }
            catch ( StripeException ex )
            {
                return BadRequest( ex.Message );
            }
        }

        private PaymentMethodViewModelItem MapToViewModel( PaymentMethodBase? p )
        {
            var result = new PaymentMethodViewModelItem();
            if ( p is CardPaymentMethod )
            {
                var card = p as CardPaymentMethod;
                result.Brand = card.Brand;
                result.CardId = card.CardId;
                var twoDigitExpirationMonth = card.ExpirationMonth < 10 ? ("0" + card.ExpirationMonth.ToString()) : card.ExpirationMonth.ToString();
                result.ExpirationDate = $"{twoDigitExpirationMonth}/{card.ExpirationYear}";
                result.Last4 = card.Last4;

            }
            else if ( p is AccountPaymentMethod )
            {
            }
            result.Id = p.Id;
            result.IsDefault = p.IsDefault;
            return result;
        }
    }
}
