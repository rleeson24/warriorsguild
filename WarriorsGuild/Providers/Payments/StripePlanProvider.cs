using Stripe;
using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Models.Payments;

namespace WarriorsGuild.Providers.Payments
{
    public class StripePlanProvider : IStripePlanProvider
    {
        private Stripe.PlanService PlanService { get; }
        private Stripe.ProductService ProductService { get; }

        public StripePlanProvider( Stripe.PlanService stripePlanService, Stripe.ProductService stripeProductService )
        {
            PlanService = stripePlanService;
            ProductService = stripeProductService;
        }

        public async Task<StripeList<Stripe.Plan>> ListPlans()
        {
            var options = new PlanListOptions();
            options.AddExpand( $"data.{nameof( Stripe.Plan.Product ).ToLower()}" );
            var planList = await PlanService.ListAsync( options );
            return planList;
        }

        public async Task<Stripe.Plan> Get( string id )
        {
            var options = new PlanGetOptions();
            options.AddExpand( nameof( Stripe.Plan.Product ).ToLower() );
            var plan = await PlanService.GetAsync( id, options );
            return plan;
        }

        public async Task<CreatePlanResponse> Create( SaveBillingPlanRequest request )
        {
            var response = new CreatePlanResponse();

            var requestOptions = new Stripe.RequestOptions() { IdempotencyKey = Guid.NewGuid().ToString( "D" ) };
            try
            {
                var stripeProductCreateOptions = new Stripe.ProductCreateOptions()
                {
                    Active = request.State == BillingPlanState.Active,
                    Name = request.PlanName,
                    Type = "service",
                };
                var productResponse = await ProductService.CreateAsync( stripeProductCreateOptions, requestOptions );
                response.ProductId = productResponse.Id;
            }
            catch ( StripeException e )
            {
                HandleStripeError( e.StripeError );
                throw;
            }

            requestOptions.IdempotencyKey = Guid.NewGuid().ToString( "D" );
            try
            {
                var stripePlanCreateOptions = new Stripe.PlanCreateOptions()
                {
                    Active = request.State == BillingPlanState.Active,
                    Amount = (Int32)(request.RegularPlanPrice * 100),
                    Currency = request.Currency,
                    Interval = MapInterval( request.PlanFrequency ),
                    TrialPeriodDays = request.TrialPlanLength * (request.PlanFrequency == Frequency.Monthly ? 30 : 365),
                    Product = response.ProductId
                };

                var createPlanResponse = await PlanService.CreateAsync( stripePlanCreateOptions, requestOptions );
                response.PlanId = createPlanResponse.Id;
                response.Success = true;
            }
            catch ( StripeException e )
            {
                HandleStripeError( e.StripeError );
                throw;
            }

            return response;
        }

        public async Task<UpdatePlanResponse> Update( UpdateBillingPlanRequest request )
        {
            var response = new UpdatePlanResponse();

            var productId = String.Empty;
            var requestOptions = new Stripe.RequestOptions() { IdempotencyKey = Guid.NewGuid().ToString( "D" ) };
            try
            {
                var stripeProductCreateOptions = new Stripe.ProductUpdateOptions()
                {
                    Active = request.Status == BillingPlanState.Active,
                };
                var productResponse = await ProductService.UpdateAsync( request.ProductId, stripeProductCreateOptions, requestOptions );
                productId = productResponse.Id;
            }
            catch ( StripeException e )
            {
                HandleStripeError( e.StripeError );
                throw;
            }

            requestOptions.IdempotencyKey = Guid.NewGuid().ToString( "D" );
            try
            {
                var stripePlanUpdateOptions = new Stripe.PlanUpdateOptions()
                {
                    Active = request.Status == BillingPlanState.Active
                };

                var createPlanResponse = await PlanService.UpdateAsync( request.PlanId, stripePlanUpdateOptions, requestOptions );
                response.Success = true;
            }
            catch ( StripeException e )
            {
                HandleStripeError( e.StripeError );
                throw;
            }
            return response;
        }

        private string? MapInterval( Frequency planFrequency )
        {
            switch ( planFrequency )
            {
                case Frequency.Monthly:
                    return "month";
                case Frequency.Yearly:
                    return "year";
                default:
                    return "month";
            }
        }

        private void HandleStripeError( StripeError error )
        {
            switch ( error.ErrorType )
            {
                case "card_error":
                    Console.WriteLine( "Code: " + error.Code );
                    Console.WriteLine( "Message: " + error.Message );
                    break;
                case "api_connection_error":
                    break;
                case "api_error":
                    break;
                case "authentication_error":
                    break;
                case "invalid_request_error":
                    break;
                case "rate_limit_error":
                    break;
                case "validation_error":
                    break;
                default:
                    // Unknown Error Type
                    break;
            }
        }
    }
}