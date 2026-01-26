using Stripe;
using WarriorsGuild.Models.Payments;

namespace WarriorsGuild.Providers.Payments
{
    public interface IStripePlanProvider
    {
        Task<CreatePlanResponse> Create( SaveBillingPlanRequest request );
        Task<StripeList<Stripe.Plan>> ListPlans();
        Task<Stripe.Plan> Get( string id );
        Task<UpdatePlanResponse> Update( UpdateBillingPlanRequest request );
    }
}