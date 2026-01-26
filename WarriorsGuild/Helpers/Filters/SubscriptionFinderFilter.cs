using Microsoft.AspNetCore.Mvc.Filters;
using WarriorsGuild.Areas.Payments;
using WarriorsGuild.Providers;

namespace WarriorsGuild.Helpers.Filters
{
    public class SubscriptionFinderFilter : IAsyncAuthorizationFilter
    {
        private readonly ILogger<SubscriptionFinderFilter> _logger;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IValuesHolder valuesHolder;
        private readonly IUserProvider _userProvider;

        public SubscriptionFinderFilter( ILogger<SubscriptionFinderFilter> logger, ISubscriptionRepository subscriptionRepository,
                                                    IHttpContextAccessor contextAccessor, IValuesHolder valuesHolder, IUserProvider userProvider )
        {
            _logger = logger;
            _subscriptionRepository = subscriptionRepository;
            _contextAccessor = contextAccessor;
            this.valuesHolder = valuesHolder;
            _userProvider = userProvider;
        }

        public async Task OnAuthorizationAsync( AuthorizationFilterContext context )
        {
            //if ( (context.ActionDescriptor as ControllerActionDescriptor).ControllerTypeInfo.BaseType == typeof( Controller ) )
            //{
            // Log as a warning so that it's very clear in sample output which authorization policies 
            // (and requirements/handlers) are in use
            _logger.LogWarning( "Evaluating authorization requirement for Must Be Subscriber" );

            if ( context.HttpContext.User.Identity != null && context.HttpContext.User.Identity.IsAuthenticated )
            {
                if ( !context.HttpContext.User.IsInRole( "Admin" ) )
                {
                    //var userId = _userProvider.GetMyUserId( context.HttpContext.User );
                    //var userSub = await _subscriptionRepository.GetMySubscriptionAsync( userId.ToString() );
                    //if ( userSub != null )
                    //{
                    //    valuesHolder.HasActiveSubscription = !userSub.BillingAgreement.Cancelled.HasValue;
                    //    valuesHolder.IsPayingParty = userSub.UserSubscription.IsPayingParty;
                    //}
                    valuesHolder.HasActiveSubscription = true;
                    valuesHolder.IsPayingParty = context.HttpContext.User.IsInRole( "Guardian" );

                }
            }
            else
            {
                valuesHolder.HasActiveSubscription = false;
                valuesHolder.IsPayingParty = false;
            }
            //}
        }
    }
}
