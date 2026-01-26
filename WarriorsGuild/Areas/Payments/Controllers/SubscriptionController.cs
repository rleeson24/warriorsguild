using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using WarriorsGuild.Areas.Payments.Mappers;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Email;
using WarriorsGuild.Models.Payments;
using WarriorsGuild.Providers;
using WarriorsGuild.Providers.Payments;
using static IdentityServer4.IdentityServerConstants;

namespace WarriorsGuild.Areas.Payments.Controllers
{
    [ApiController]
    [Route( "api/[controller]" )]
    [Authorize( LocalApi.PolicyName )]
    [IgnoreAntiforgeryToken]
    public class SubscriptionController : ControllerBase
    {

        private UserManager<ApplicationUser> _userManager;
        private readonly IUserProvider _userProvider;

        private ISubscriptionMapper SubscriptionMapper { get; }
        private IPriceOptionManager Payments { get; }
        private ISubscriptionManager SubscriptionMgr { get; }
        private IEmailProvider EmailProvider { get; }

        public ISubscriptionManager SubscriptionManager { get; }

        public SubscriptionController( UserManager<ApplicationUser> userManager, IPriceOptionManager paymentService, ISubscriptionManager subscriptionManager,
                                        ISubscriptionMapper subscriptionMapper, IEmailProvider emailProvider, IUserProvider userProvider )
        {
            _userManager = userManager;
            Payments = paymentService;
            SubscriptionMgr = subscriptionManager;
            SubscriptionMapper = subscriptionMapper;
            EmailProvider = emailProvider;
            this._userProvider = userProvider;
        }

        // GET: api/BillingAgreements/5
        [HttpGet( "{id}" )]
        public async Task<ActionResult<BillingAgreement>> GetBillingAgreement( Guid id )
        {
            var billingAgreement = await SubscriptionMgr.GetBillingAgreement( id );
            if ( billingAgreement == null )
            {
                return NotFound();
            }

            return Ok( billingAgreement );
        }

        // GET: api/BillingAgreements/5
        [HttpGet( "mine" )]
        public async Task<ActionResult<BillingAgreementViewModel>> GetMyBillingAgreement()
        {
            var myUserId = _userProvider.GetMyUserId( User );
            var bavm = new BillingAgreementViewModel();

            var subscriptionAndAgreement = await SubscriptionMgr.GetMySubscriptionAsync( myUserId.ToString() );
            if ( subscriptionAndAgreement != null && !subscriptionAndAgreement.BillingAgreement.Cancelled.HasValue )
            {
                var me = _userManager.Users.FirstOrDefault( u => u.Id == myUserId.ToString() );
                if ( me == null )
                {
                    return NotFound();
                }
                var usersOnSubscription = await SubscriptionMgr.GetUsersOnSubscriptionAsync( subscriptionAndAgreement.BillingAgreement.Id );
                var userDictionary = new Dictionary<ApplicationUser, Tuple<Boolean, Boolean>>();
                foreach ( var us in usersOnSubscription )
                {
                    var userToMap = _userManager.Users.FirstOrDefault( u => u.Id == us.UserId.ToString() );
                    var isGuardian = await _userManager.IsInRoleAsync( userToMap, "Guardian" );
                    var isWarrior = await _userManager.IsInRoleAsync( userToMap, "Warrior" );
                    userDictionary.Add( userToMap, new Tuple<Boolean, Boolean>( isGuardian, isWarrior ) );
                }
                var mappedUsers = usersOnSubscription
                    .Select( us =>
                    {
                        var userToMap = userDictionary.Keys.Single( k => Guid.Parse( k.Id ) == us.UserId );
                        return userToMap != null ? SubscriptionMapper.MapToSubscriptionUser( userToMap,
                                                                                us,
                                                                                userDictionary[ userToMap ].Item1,
                                                                                userDictionary[ userToMap ].Item2 )
                                                                                : null;
                    } )
                    .Where( us => us != null ).ToArray();
                bavm = SubscriptionMapper.CreateViewModel( subscriptionAndAgreement, mappedUsers );
            }
            else
            {
                //EmailProvider.RenderEmailViewToString( EmailView.Generic, $@"Message = Subscription was not found for user
                //																UserId = {userId}" );
                return NotFound();
            }
            return Ok( bavm );
        }

        // PUT: api/BillingAgreements/5
        [HttpPut( "{id}" )]
        public async Task<ActionResult> PutBillingAgreement( Guid id, BillingAgreement billingAgreement )
        {
            //if ( !ModelState.IsValid )
            //{
            //	return BadRequest( ModelState );
            //}

            //if ( id != billingAgreement.Id )
            //{
            //	return BadRequest();
            //}

            //Db.Entry( billingAgreement ).State = EntityState.Modified;

            //try
            //{
            //	await Db.SaveChangesAsync();
            //}
            //catch ( DbUpdateConcurrencyException )
            //{
            //	if ( !BillingAgreementExists( id ) )
            //	{
            //		return NotFound();
            //	}
            //	else
            //	{
            //		throw;
            //	}
            //}

            return NoContent();
        }

        // POST: api/BillingAgreements
        [HttpPost]
        [Authorize( Roles = "Admin, Guardian" )]
        public async Task<ActionResult<BillingAgreementViewModel>> PostBillingAgreement( BillingViewModel request )
        {
            try
            {
                if ( !ModelState.IsValid )
                {
                    return BadRequest( ModelState );
                }
                var myUserId = _userProvider.GetMyUserId( User );
                var subscription = await SubscriptionMgr.GetMySubscriptionAsync( myUserId.ToString() );
                if ( subscription != null && !subscription.UserSubscription.IsPayingParty )
                {
                    return BadRequest( "Only the paying party or an unsubscribed user can modify a subscription." );
                }
                var me = await _userManager.FindByIdAsync( myUserId.ToString() );

                var subscriptionAndAgreement = await SubscriptionMgr.CreateSubscription( request, me );
                var usersOnSubscription = await SubscriptionMgr.GetUsersOnSubscriptionAsync( subscriptionAndAgreement.BillingAgreement.Id );
                var mappedUsers = new List<SubscriptionUser>();
                foreach ( var us in usersOnSubscription )
                {
                    mappedUsers.Add( SubscriptionMapper.MapToSubscriptionUser( me,
                                                                            us,
                                                                            await _userManager.IsInRoleAsync( await _userManager.FindByIdAsync( us.UserId.ToString() ), "Guardian" ),
                                                                            await _userManager.IsInRoleAsync( await _userManager.FindByIdAsync( us.UserId.ToString() ), "Warrior" ) )
                                                                            );
                }
                var bavm = SubscriptionMapper.CreateViewModel( subscriptionAndAgreement, mappedUsers );
                return Ok( bavm );
            }
            catch ( StripeException ex )
            {
                return BadRequest( ex.Message );
            }
        }


        // DELETE: api/BillingAgreements/5
        [HttpDelete]
        public async Task<ActionResult<BillingAgreement>> DeleteBillingAgreement( Guid id )
        {
            var myUserId = _userProvider.GetMyUserId( User );
            var billingAgreement = await SubscriptionMgr.GetBillingAgreement( id );
            if ( billingAgreement == null )
            {
                return NotFound();
            }

            await SubscriptionMgr.Unsubscribe( billingAgreement, myUserId.ToString() );

            return Ok( billingAgreement );
        }


        // DELETE: api/BillingAgreements/5
        [HttpPost( "Unsubscribe" )]
        [Authorize( Policy = "MustBeSubscriber" )]
        public async Task<ActionResult<BillingAgreement>> Unsubscribe()
        {
            var myUserId = _userProvider.GetMyUserId( User );
            var subscriptionAndAgreement = await SubscriptionMgr.GetMySubscriptionAsync( myUserId.ToString() );
            var billingAgreement = await SubscriptionMgr.GetBillingAgreement( subscriptionAndAgreement.UserSubscription.BillingAgreementId );
            if ( billingAgreement == null )
            {
                return NotFound();
            }

            await SubscriptionMgr.Unsubscribe( billingAgreement, myUserId.ToString() );

            return Ok( billingAgreement );
        }

        //// POST: api/Subscriptions/Subscribe
        //[Route( "Subscribe" )]
        //[HttpPost]
        //[ResponseType( typeof( String ) )]
        //public async Task<ActionResult<Product>> Subscribe( String planId )
        //{
        //	PriceOption priceOption = await db.PriceOptions.FindAsync( new Guid( planId ) );
        //	if ( priceOption == null )
        //	{
        //		return NotFound();
        //	}

        //	var p = new PaypalPaymentProvider();
        //	var baResponse = p.CreateBillingAgreement( priceOption.PlanId, null, "My Plan", "I don't know what to put here", DateTime.UtcNow.AddDays( 1 ) );
        //	if ( baResponse != null )
        //	{
        //		//var agreementsToCancel = db.BillingAgreements.Where( ba => ba.UserId == new Guid( User.FindFirstValue( claimType: "sub" ) ) );
        //		//foreach ( var a in agreementsToCancel )
        //		//{
        //		//	a.Cancelled = DateTime.UtcNow;
        //		//}
        //		db.BillingAgreements.Add( new BillingAgreement
        //		{
        //			UserId = new Guid( User.FindFirstValue( claimType: "sub" ) ),
        //			PlanId = priceOption.PlanId,
        //			Created = DateTime.UtcNow,
        //			Token = baResponse.Token,
        //			Status = BillingAgreementStatus.Requested
        //		} );

        //		await db.SaveChangesAsync();

        //		return Ok( baResponse.ApprovalUrl );
        //	}

        //	return InternalServerError();
        //}
    }
}