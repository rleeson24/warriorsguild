using Microsoft.AspNetCore.Authorization;
using WarriorsGuild.Areas.Payments;
using WarriorsGuild.Helpers;
using WarriorsGuild.Providers;

namespace WarriorsGuild.Authorization.MustBeSubscriber
{
    // This class contains logic for determining whether MinimumAgeRequirements in authorization
    // policies are satisfied or not
    internal class MustBeSubscriberAuthorizationHandler : AuthorizationHandler<MustBeSubscriberRequirement>
    {
        private readonly ILogger<MustBeSubscriberAuthorizationHandler> _logger;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IValuesHolder valuesHolder;
        private readonly IUserProvider _userProvider;

        public MustBeSubscriberAuthorizationHandler( ILogger<MustBeSubscriberAuthorizationHandler> logger, ISubscriptionRepository subscriptionRepository,
                                                    IHttpContextAccessor contextAccessor, IValuesHolder valuesHolder, IUserProvider userProvider )
        {
            _logger = logger;
            _subscriptionRepository = subscriptionRepository;
            _contextAccessor = contextAccessor;
            this.valuesHolder = valuesHolder;
            _userProvider = userProvider;
        }

        // Check whether a given MinimumAgeRequirement is satisfied or not for a particular context
        protected async override Task HandleRequirementAsync( AuthorizationHandlerContext context, MustBeSubscriberRequirement requirement )
        {
            // Log as a warning so that it's very clear in sample output which authorization policies 
            // (and requirements/handlers) are in use
            _logger.LogWarning( "Evaluating authorization requirement for Must Be Subscriber" );

            if ( context.User?.Identity != null && context.User.Identity.IsAuthenticated )
            {
                if ( !context.User.IsInRole( "Admin" ) )
                {
                    valuesHolder.HasActiveSubscription = true;
                    valuesHolder.IsPayingParty = context.User.IsInRole( "Guardian" );
                    //var myUserId = _userProvider.GetMyUserId( context.User );
                    //var userSub = await _subscriptionRepository.GetMySubscriptionAsync( myUserId.ToString() );
                    //if ( userSub != null )
                    //{
                    //    valuesHolder.HasActiveSubscription = !userSub.BillingAgreement.Cancelled.HasValue;
                    //    valuesHolder.IsPayingParty = userSub.UserSubscription.IsPayingParty;
                    //}
                    //if ( (userSub == null || userSub.BillingAgreement.Cancelled.HasValue) )
                    //{
                    //    await _contextAccessor.HttpContext.ExecuteResultAsync( new RedirectResult( @"~\Manage\" ) );
                    //}
                    //else
                    //{
                    context.Succeed( requirement );
                    //}
                }
                else
                {
                    context.Succeed( requirement );
                }
            }
            else
            {
                context.Fail();
            }
        }
        //_logger.LogInformation( "No DateOfBirth claim present" );
    }
}
