using Microsoft.AspNetCore.Identity;
using Stripe;
using WarriorsGuild.Data.Models;

namespace WarriorsGuild.Providers.Payments
{
    public class StripeSubscriptionProvider : IStripeSubscriptionProvider
    {

        private UserManager<ApplicationUser> _userManager;
        private UserManager<ApplicationUser> UserManager
        {
            get
            {
                return _userManager;
            }
        }
        private Stripe.CardService CardService { get; }
        private Stripe.CustomerService CustomerService { get; }
        private Stripe.SubscriptionService SubscriptionService { get; }
        private Stripe.InvoiceItemService InvoiceItemService { get; }

        public StripeSubscriptionProvider( UserManager<ApplicationUser> userManager, Stripe.CustomerService stripeCustomerService, Stripe.SubscriptionService stripeSubscriptionService, Stripe.InvoiceItemService stripeInvoiceItemService, Stripe.CardService stripeCardService )
        {
            _userManager = userManager;
            CustomerService = stripeCustomerService;
            SubscriptionService = stripeSubscriptionService;
            InvoiceItemService = stripeInvoiceItemService;
            CardService = stripeCardService;
        }

        public async Task<String> Create( ApplicationUser user, string token, Dictionary<String, Int32> plans, Decimal setupFee = 0 )
        {
            try
            {
                if ( String.IsNullOrEmpty( user.StripeCustomerId ) )
                {
                    var createCustomerRequestOptions = new Stripe.RequestOptions() { IdempotencyKey = Guid.NewGuid().ToString( "N" ) };
                    var customerCreateOptions = new Stripe.CustomerCreateOptions()
                    {
                        Email = user.Email,
                        Source = token
                    };

                    var customer = CustomerService.Create( customerCreateOptions, createCustomerRequestOptions );
                    user.StripeCustomerId = customer.Id;

                    if ( setupFee > 0 )
                    {
                        createCustomerRequestOptions.IdempotencyKey = Guid.NewGuid().ToString( "N" );
                        var setupFeeLineItemCreateOptions = new Stripe.InvoiceItemCreateOptions()
                        {
                            Amount = (Int32)(setupFee * 100),
                            Currency = "usd",
                            Customer = customer.Id,
                            Description = "One-time Setup Fee"
                        };
                        InvoiceItemService.Create( setupFeeLineItemCreateOptions, createCustomerRequestOptions );
                    }
                }
                else
                {
                    var options = new CustomerGetOptions();
                    options.AddExpand( "default_source" );
                    var customer = await CustomerService.GetAsync( user.StripeCustomerId, options );
                    if ( customer.DefaultSource == null )
                    {
                        await CardService.CreateAsync( user.StripeCustomerId, new Stripe.CardCreateOptions() { Source = token } );
                    }
                }
                var requestOptions = new Stripe.RequestOptions() { IdempotencyKey = Guid.NewGuid().ToString( "N" ) };
                var subscriptionCreateOptions = new Stripe.SubscriptionCreateOptions()
                {
                    Customer = user.StripeCustomerId,
                    Items = new List<Stripe.SubscriptionItemOption>()
                };
                foreach ( var plan in plans )
                {
                    subscriptionCreateOptions.Items.Add( new Stripe.SubscriptionItemOption()
                    {
                        Plan = plan.Key,
                        Quantity = plan.Value
                    } );
                }
                var response = SubscriptionService.Create( subscriptionCreateOptions, requestOptions );

                await UserManager.UpdateAsync( user );
                return response.Id;
            }
            catch ( StripeException )
            {
                throw;
            }
        }

        public async Task Unsubscribe( string subscriptionId )
        {
            var requestOptions = new Stripe.RequestOptions() { IdempotencyKey = Guid.NewGuid().ToString( "N" ) };
            var cancelledSubscription = await SubscriptionService.CancelAsync( subscriptionId, new Stripe.SubscriptionCancelOptions() { }, requestOptions );
        }

        public void CreateSingleCharge()
        {
            //var myCharge = new StripeChargeCreateOptions();

            //// always set these properties
            //myCharge.Amount = (Int32)( chargeAmount * 100 );
            //myCharge.Currency = "usd";

            //// set this if you want to
            //myCharge.Description = "Embracing Memories - New Profile";

            //myCharge.SourceTokenOrExistingSourceId = token;

            //// set this property if using a customer - this MUST be set if you are using an existing source!
            ////myCharge.CustomerId = *customerId *;

            //// set this if you have your own application fees (you must have your application configured first within Stripe)
            ////myCharge.ApplicationFee = 25;

            //// (not required) set this to false if you don't want to capture the charge yet - requires you call capture later
            //myCharge.Capture = true;

            //var chargeService = new StripeChargeService();
            //try
            //{
            //	StripeCharge stripeCharge = chargeService.Create( myCharge );
            //}
            //catch ( Exception ex )
            //{
            //	return ex.Message;
            //}
        }
    }
}