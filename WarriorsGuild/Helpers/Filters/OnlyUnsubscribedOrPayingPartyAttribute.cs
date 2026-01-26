using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WarriorsGuild.Helpers.Filters
{
    public class OnlyUnsubscribedOrPayingParty : IAuthorizationFilter
    {
        private readonly IValuesHolder valuesHolder;

        public OnlyUnsubscribedOrPayingParty( IValuesHolder valuesHolder )
        {
            this.valuesHolder = valuesHolder;
        }

        public void OnAuthorization( AuthorizationFilterContext context )
        {
            if ( !context.HttpContext.User.IsInRole( "Admin" ) )
            {
                if ( valuesHolder.HasActiveSubscription && !valuesHolder.IsPayingParty )
                {
                    context.Result = new RedirectResult( @"~\Manage\" );
                }
            }
        }
    }
}