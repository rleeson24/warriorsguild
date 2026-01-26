using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WarriorsGuild.Controllers
{
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [HttpPost( "/error-local-development" )]
        public IActionResult ErrorLocalDevelopment( [FromServices] IWebHostEnvironment webHostEnvironment )
        {
            if ( webHostEnvironment.EnvironmentName != "Development" )
            {
                throw new InvalidOperationException(
                    "This shouldn't be invoked in non-development environments." );
            }

            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();

            return Problem(
                detail: context.Error.StackTrace,
                title: context.Error.Message );
        }

        [HttpPost( "/error" )]
        public IActionResult Error() => Problem();
    }
}