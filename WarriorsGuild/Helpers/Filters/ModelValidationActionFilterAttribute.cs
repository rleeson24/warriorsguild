using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WarriorsGuild.Helpers.Filters
{
    public class ModelValidationActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting( ActionExecutingContext context )
        {
            var modelState = context.ModelState;
            if ( !modelState.IsValid )
            {
                context.Result = new BadRequestObjectResult( context.ModelState );
            }
        }
    }
}
