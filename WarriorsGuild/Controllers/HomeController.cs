using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using WarriorsGuild.Email;
using WarriorsGuild.Helpers;
using WarriorsGuild.Models;

namespace WarriorsGuild.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmailProvider emailProvider;

        public IConfiguration Configuration { get; }

        private IIdentityServerInteractionService _interaction;
        private IWebHostEnvironment _environment;
        private ILogger<HomeController> _logger;
        private readonly IIdentityServerInteractionService isis;

        public HomeController( IEmailProvider emailProvider, IConfiguration config, IIdentityServerInteractionService interaction, IWebHostEnvironment environment, ILogger<HomeController> logger, IIdentityServerInteractionService isis )
        {
            this.emailProvider = emailProvider;
            Configuration = config;
            _interaction = interaction;
            _environment = environment;
            _logger = logger;
            this.isis = isis;
        }

        public async Task<ActionResult> Index()
        {
            //var client = new HttpClient();
            //var disco = await client.GetDiscoveryDocumentAsync( "https://localhost:5000" );
            //if ( disco.IsError )
            //{
            //    Console.WriteLine( disco.Error );
            //    return Problem();
            //}
            //var tokenResponse = await client.RequestClientCredentialsTokenAsync( new ClientCredentialsTokenRequest
            //{
            //    Address = disco.TokenEndpoint,

            //    ClientId = "warriorsGuildMVC",
            //    ClientSecret = Configuration[ "Authentication:IdentityServer:SecretKey" ],
            //    Scope = "WarriorsGuildApi"
            //} );

            //if ( tokenResponse.IsError )
            //{
            //    Console.WriteLine( tokenResponse.Error );
            //    return Problem();
            //}

            //Console.WriteLine( tokenResponse.Json );
            //var apiClient = new HttpClient();
            //apiClient.SetBearerToken( tokenResponse.AccessToken );

            //var response = await apiClient.GetAsync( "https://localhost:5000/api/crosses/test" );
            //if ( !response.IsSuccessStatusCode )
            //{
            //    Console.WriteLine( response.StatusCode );
            //}
            //else
            //{
            //    var content = await response.Content.ReadAsStringAsync();
            //    Console.WriteLine( JArray.Parse( content ) );
            //}
            if ( User?.Identity != null && User.Identity.IsAuthenticated )
            {
                return RedirectToAction( "Index", "Dashboard" );
            }
            else
            {
                return View( new { PriceOptionsUrl = "/api/PriceOptions" } );
            }
        }

        public ActionResult About()
        {
            return View();
        }

        [SecurityHeaders]
        [AllowAnonymous]
        public IActionResult Test()
        {
            if ( _environment.IsDevelopment() )
            {
                // only show in development
                return View();
            }

            _logger.LogInformation( "Homepage is disabled in production. Returning 404." );
            return NotFound();
        }

        [ResponseCache( Duration = 0, Location = ResponseCacheLocation.None, NoStore = true )]
        public async Task<ActionResult> Error( string? errorId )
        {
            var model = new ErrorViewModel();
            model.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var a = errorId != null ? await isis.GetErrorContextAsync( errorId ) : null;
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if ( a != null )
            {
                model.Error = JsonSerializer.Serialize( a, new JsonSerializerOptions() { WriteIndented = true } );
            }
            else if ( exceptionHandlerPathFeature?.Error != null )
            {
                var ex = exceptionHandlerPathFeature.Error;
                var headersRaw = HttpContext.Request.Headers;
                var headers = new List<String>();
                foreach ( var key in headersRaw.Keys )
                {
                    headers.Add( $"{key}: {String.Join( ",", headersRaw[ key ] )}" );
                }
                var message = new EmailMessage();
                message.Recipients = new[] { "rleeson_2000@yahoo.com" };
                message.Subject = "Exception Occurred in WarriorsGuild - Application_Error";
                message.TextBody = String.Format( "Unhandled exception processing: {0}", ex + $@"

{String.Join( Environment.NewLine, String.Join( Environment.NewLine, headers ) )}

UserId: {User.FindFirstValue( claimType: "sub" )}
RequestId: {Activity.Current?.Id ?? HttpContext.TraceIdentifier}"
                );
                message.HtmlBody = await emailProvider.RenderEmailViewToStringAsync( EmailView.Generic, message.TextBody );

                await emailProvider.SendAsync( message );

                model.Error = message.TextBody;

            }
            return View( model );
        }
    }
}
