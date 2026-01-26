using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarriorsGuild.Areas.Payments.Mappers;
using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Models.Payments;
using WarriorsGuild.Providers.Payments;
using static IdentityServer4.IdentityServerConstants;

namespace WarriorsGuild.Areas.Payments.Controllers
{
    [ApiController]
    [Route( "api/[controller]" )]
    [Authorize( LocalApi.PolicyName )]
    [IgnoreAntiforgeryToken]
    public class PriceOptionsController : ControllerBase
    {
        private IPriceOptionManager PriceOptionMgr { get; }

        private IStripePlanProvider StripeProvider { get; }

        private IPriceOptionMapper PriceOptionMapper { get; }

        private IBillingPlanManager BillingPlanManager { get; }

        private IBillingPlanRequestMapper BillingPlanRequestMapper { get; }

        public PriceOptionsController( IPriceOptionManager priceOptionMgr, IStripePlanProvider stripeProvider, IPriceOptionMapper priceOptionMapper, IBillingPlanRequestMapper billingPlanRequestMapper, IBillingPlanManager billingPlanManager )
        {
            PriceOptionMgr = priceOptionMgr;
            StripeProvider = stripeProvider;
            PriceOptionMapper = priceOptionMapper;
            BillingPlanRequestMapper = billingPlanRequestMapper;
            BillingPlanManager = billingPlanManager;
        }

        // GET: api/PriceOptions
        [HttpGet]
        [Authorize( Policy = "MustBeAdminApi" )]
        public async Task<ActionResult<IEnumerable<ManageablePriceOption>>> GetManageablePriceOptions()
        {
            var priceOptions = await PriceOptionMgr.ListPriceOptions();
            var mPriceOptions = priceOptions.Select( PriceOptionMapper.MapToManageablePriceOption );
            return Ok( mPriceOptions );
        }

        // GET: api/PriceOptions/ForSubscription

        [HttpGet( "ForSubscription" )]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SubscribeablePriceOption>>> GetSuscribeablePriceOptions()
        {
            var priceOptions = await PriceOptionMgr.ListPriceOptions();
            var mPriceOptions = priceOptions.Where( po => po.Stripe.Active ).Select( PriceOptionMapper.MapToSubscribeablePriceOption );
            return Ok( mPriceOptions );
        }

        // GET: api/PriceOptions/ForSubscription

        [HttpGet( "Simple" )]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SimplePriceOption>>> GetSimplePriceOptions()
        {
            var priceOptions = await PriceOptionMgr.ListPriceOptions();
            var mPriceOptions = priceOptions.Where( po => po.Stripe.Active ).Select( PriceOptionMapper.MapToSimplePriceOption );
            return Ok( mPriceOptions );
        }

        // GET: api/PriceOptions/5
        [Authorize]
        [HttpGet( "{id}" )]
        public async Task<ActionResult<PriceOption>> GetPriceOption( string id )
        {
            if ( !Guid.TryParse( id, out var guid ) )
            {
                //new Logger().LogMessage( "Invalid Parameter", "GetPriceOption was called with an invalid id parameter", id );
                return NotFound();
            }
            var priceOption = await PriceOptionMgr.GetPriceOption( guid );
            if ( priceOption == null )
            {
                return NotFound();
            }

            return Ok( priceOption );
        }

        // PUT: api/PriceOptions/5
        [HttpPut( "{id}" )]
        [Authorize( Policy = "MustBeAdminApi" )]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> PutPriceOption( string id, SavePriceOptionRequest request )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }
            if ( id != request.Id.ToString() )
            {
                return BadRequest();
            }

            await PriceOptionMgr.Update( request );

            return Ok();
        }


        // POST: api/PriceOptions
        [HttpPost]
        [Authorize( Policy = "MustBeAdminApi" )]
        //[IgnoreAntiforgeryToken]
        public async Task<ActionResult<PriceOption>> PostPriceOption( SavePriceOptionRequest request )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            var createPlanResponse = await BillingPlanManager.CreateBillingPlan( request );
            var priceOption = await PriceOptionMgr.Create( request, createPlanResponse );

            return CreatedAtAction( nameof( GetPriceOption ), nameof( PriceOptionsController ), new { id = priceOption.Id }, PriceOptionMapper.MapToManageablePriceOption( priceOption ) );
        }

        // DELETE: api/PriceOptions/5
        [HttpDelete( "{id}" )]
        [Authorize( Policy = "MustBeAdminApi" )]
        public async Task<ActionResult<PriceOption>> DeletePriceOption( Guid id )
        {
            var priceOption = await PriceOptionMgr.GetPriceOption( id );
            if ( priceOption == null )
            {
                return NotFound();
            }

            await PriceOptionMgr.Delete( priceOption );

            return Ok( priceOption );
        }

        // PUT: api/PriceOptions/5/Activate
        [HttpPut( "{id}/Activate" )]
        [Authorize( Policy = "MustBeAdminApi" )]
        public async Task<ActionResult<PriceOption>> Activate( Guid id )
        {
            var priceOption = await PriceOptionMgr.GetPriceOption( id );
            if ( priceOption == null )
            {
                return NotFound();
            }

            await StripeProvider.Update( BillingPlanRequestMapper.CreateUpdateBillingPlanRequest( priceOption.StripePlanId, priceOption.StripeProductId, BillingPlanState.Active ) );
            if ( priceOption.AdditionalGuardianPlan != null )
            {
                await StripeProvider.Update( BillingPlanRequestMapper.CreateUpdateBillingPlanRequest( priceOption.AdditionalGuardianPlan.StripePlanId, priceOption.AdditionalGuardianPlan.StripeProductId, BillingPlanState.Active ) );
            }
            if ( priceOption.AdditionalWarriorPlan != null )
            {
                await StripeProvider.Update( BillingPlanRequestMapper.CreateUpdateBillingPlanRequest( priceOption.AdditionalWarriorPlan.StripePlanId, priceOption.AdditionalWarriorPlan.StripeProductId, BillingPlanState.Active ) );
            }
            //var p = new PaypalPaymentProvider();
            //if ( !p.ActivateBillingPlan( priceOption.StripePlanId ) )
            //{
            //	return InternalServerError();
            //}
            //priceOption.State = BillingPlanState.Active;

            //try
            //{
            //	await db.SaveChangesAsync();
            //}
            //catch ( DbUpdateException )
            //{
            //	if ( PriceOptionExists( priceOption.Key ) )
            //	{
            //		return Conflict();
            //	}
            //	else
            //	{
            //		throw;
            //	}
            //}

            return Ok( priceOption );
        }

        // PUT: api/PriceOptions/5/Activate
        [HttpPut( "{id}/Deactivate" )]
        [Authorize( Policy = "MustBeAdminApi" )]
        public async Task<ActionResult<PriceOption>> Deactivate( Guid id )
        {
            var priceOption = await PriceOptionMgr.GetPriceOption( id );
            if ( priceOption == null )
            {
                return NotFound();
            }

            await StripeProvider.Update( BillingPlanRequestMapper.CreateUpdateBillingPlanRequest( priceOption.StripePlanId, priceOption.StripeProductId, BillingPlanState.Inactive ) );
            if ( priceOption.AdditionalGuardianPlan != null )
            {
                await StripeProvider.Update( BillingPlanRequestMapper.CreateUpdateBillingPlanRequest( priceOption.AdditionalGuardianPlan.StripePlanId, priceOption.AdditionalGuardianPlan.StripeProductId, BillingPlanState.Inactive ) );
            }
            if ( priceOption.AdditionalWarriorPlan != null )
            {
                await StripeProvider.Update( BillingPlanRequestMapper.CreateUpdateBillingPlanRequest( priceOption.AdditionalWarriorPlan.StripePlanId, priceOption.AdditionalWarriorPlan.StripeProductId, BillingPlanState.Inactive ) );
            }

            //var p = new PaypalPaymentProvider();
            //if ( !p.DeactivateBillingPlan( priceOption.StripePlanId ) )
            //{
            //	return InternalServerError();
            //}
            //priceOption.State = BillingPlanState.Inactive;

            //try
            //{
            //	await db.SaveChangesAsync();
            //}
            //catch ( DbUpdateException )
            //{
            //	if ( PriceOptionExists( priceOption.Key ) )
            //	{
            //		return Conflict();
            //	}
            //	else
            //	{
            //		throw;
            //	}
            //}

            return Ok( priceOption );
        }
    }
}