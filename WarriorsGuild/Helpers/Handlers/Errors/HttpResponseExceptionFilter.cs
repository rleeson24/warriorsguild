using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using WarriorsGuild.Email;
using WarriorsGuild.Helpers.Handlers.Errors.Models;

namespace WarriorsGuild.Helpers.Handlers.Errors
{
    public class HttpResponseExceptionFilter : IAsyncExceptionFilter, IOrderedFilter
    {
        private readonly IEmailProvider emailProvider;
        private readonly ILogger<HttpResponseExceptionFilter> _logger;
        private readonly IWebHostEnvironment env;

        public HttpResponseExceptionFilter( IEmailProvider emailProvider, ILogger<HttpResponseExceptionFilter> logger, IWebHostEnvironment env )
        {
            this.emailProvider = emailProvider;
            _logger = logger;
            this.env = env;
        }
        public int Order { get; } = int.MaxValue - 10;

        public void OnActionExecuting( ActionExecutingContext context ) { }


        public async Task OnExceptionAsync( ExceptionContext context )
        {
            if ( context.Exception != null )
            {
                Exception ex = context.Exception;
                var headersRaw = context.HttpContext.Request.Headers;
                var headers = new List<String>();
                foreach ( var key in headersRaw.Keys )
                {
                    headers.Add( $"{key}: {String.Join( ",", headersRaw[ key ] )}" );
                }
                var message = new EmailMessage();
                message.Recipients = new[] { "tech@warriorsguild.com" };
                message.Subject = "Exception Occurred in WarriorsGuild - Application_Error";
                message.TextBody = $@"Unhandled exception processing: {ex}

{Environment.NewLine}{String.Join( Environment.NewLine, headers )}

UserId: {context.HttpContext.User.FindFirstValue( claimType: "sub" )}";
                message.HtmlBody = await emailProvider.RenderEmailViewToStringAsync( EmailView.Generic, message.TextBody );

                await emailProvider.SendAsync( message );
                _logger.LogError( message.TextBody );

                var statusCode = 500;
                if ( context.Exception is HttpResponseException exception )
                {
                    statusCode = exception.Status;
                }
                if ( env.IsDevelopment() )
                {
                    context.Result = new ObjectResult( context.Exception )
                    {
                        StatusCode = statusCode,
                    };
                }
                else
                {
                    context.Result = new ObjectResult( null )
                    {
                        StatusCode = statusCode
                    };
                }
                context.ExceptionHandled = true;
            }
        }
    }
}
