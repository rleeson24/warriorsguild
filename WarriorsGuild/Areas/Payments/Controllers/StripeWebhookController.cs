using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Stripe;
using WarriorsGuild.Areas.Payments.Models.Stripe;
using WarriorsGuild.Data.Models;
using WarriorsGuild.DataAccess;

namespace WarriorsGuild.Areas.Payments.Controllers
{
    [ApiController]
    [Route( "api/[controller]" )]
    [IgnoreAntiforgeryToken]
    public class StripeWebhookController : ControllerBase
    {
        private IGuildDbContext _dbContext;
        private readonly IConfiguration _config;

        public StripeWebhookController( IGuildDbContext dbContext,IConfiguration config )
        {
            _dbContext = dbContext;
            this._config = config;
        }

        // POST: api/StripeWebhookMessages
        [HttpPost]
        public async Task<IResult> PostStripeWebhookMessage()
        {
            var json = await new StreamReader( Request.Body ).ReadToEndAsync();

            var signingSecret = _config.GetValue<string>("Stripe:WebhookKey");
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers[ "Stripe-Signature" ],
                    signingSecret
                );

                // Handle the event
                if ( stripeEvent.Type == Events.PaymentIntentSucceeded )
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    Console.WriteLine( "PaymentIntent was successful!" );
                }
                else if ( stripeEvent.Type == Events.PaymentMethodAttached )
                {
                    var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
                    Console.WriteLine( "PaymentMethod was attached to a Customer!" );
                }
                else if ( stripeEvent.Type == Stripe.Events.CustomerSubscriptionDeleted )
                {
                    var sub = stripeEvent.Data.Object as Stripe.Subscription;
                    var subscription = _dbContext.BillingAgreements.FirstOrDefault( ba => ba.StripeSubscriptionId == sub.Id );
                    if ( subscription != null && subscription.NextPaymentDue > DateTime.UtcNow )
                    {
                        subscription.NextPaymentDue = DateTime.UtcNow;
                        await _dbContext.SaveChangesAsync();
                    }
                }
                else if ( stripeEvent.Type == Stripe.Events.InvoicePaymentSucceeded )
                {
                    var invoice = stripeEvent.Data.Object as Stripe.Invoice;
                    //var customer = CustomerService.Get( invoice.CustomerId );
                    //var user = await UserManager.FindByEmailAsync( customer.Email );
                    //var subscription = invoice.Stripe.InvoiceLineItems.Data.FirstOrDefault( li => !String.IsNullOrEmpty( li.SubscriptionId ) );
                    var subscription = _dbContext.BillingAgreements.Include( ba => ba.PriceOption ).FirstOrDefault( ba => ba.StripeSubscriptionId == invoice.SubscriptionId );
                    if ( subscription != null )
                    {
                        subscription.LastPaid = DateTime.UtcNow;
                        subscription.NextPaymentDue = subscription.PriceOption.Frequency == Data.Models.Payments.Frequency.Monthly
                                                            ? subscription.NextPaymentDue.AddMonths( 1 )
                                                            : subscription.NextPaymentDue.AddYears( 1 );
                        await _dbContext.SaveChangesAsync();
                    }
                }
                else
                {
                    Console.WriteLine( "Unhandled event type: {0}", stripeEvent.Type );
                }

                var stripeWebhookMessage = new StripeWebhookMessage();
                stripeWebhookMessage.Id = stripeEvent.Id;
                stripeWebhookMessage.StripeEvent = json;
                stripeWebhookMessage.EventType = stripeEvent.Type;
                stripeWebhookMessage.Received = DateTime.UtcNow;
                stripeWebhookMessage.LiveMode = stripeEvent.Livemode;

                if ( StripeWebhookMessageExists( stripeWebhookMessage.Id ) )
                {
                    return Results.Ok();
                }

                _dbContext.StripeWebhookMessages.Add( stripeWebhookMessage );

                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch ( DbUpdateException )
                {
                    if ( StripeWebhookMessageExists( stripeWebhookMessage.Id ) )
                    {
                        return Results.Conflict();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Results.Ok( new { Message = "success" } );
            }
            catch ( StripeException e )
            {
                return Results.BadRequest( new { Error = e.Message } );
            }
            catch ( Exception e )
            {
                return Results.BadRequest( new { Error = $"{e}" } );
            }
        }

        private bool StripeWebhookMessageExists( string? id )
        {
            return _dbContext.StripeWebhookMessages.Any( e => e.Id == id );
        }
    }
}