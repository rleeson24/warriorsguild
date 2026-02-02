using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using System.Text.Json;

using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Security.KeyVault.Secrets;

using IdentityServer4;

using Lamar;
using Lamar.Microsoft.DependencyInjection;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using NLog.Web;

using Stripe;

using WarriorsGuild;
using WarriorsGuild.Authorization.CustomTokenProviders;
using WarriorsGuild.Authorization.MustBeSubscriber;
using WarriorsGuild.Data;
using WarriorsGuild.Data.Models;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Helpers.Authentication;
using WarriorsGuild.Helpers.Filters;
using WarriorsGuild.Helpers.Handlers.Errors;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Helpers.Utilities.Models;
using WarriorsGuild.Providers.Payments;

// -----------------------------------------------------------------------------
// Application setup
// -----------------------------------------------------------------------------

var builder = WebApplication.CreateBuilder( args );

builder.Host
    .ConfigureAppConfiguration( ( context, config ) =>
    {
        if ( context.HostingEnvironment.IsProduction() )
        {
            var builtConfig = config.Build();
            var secretClient = new SecretClient(
                new Uri( $"https://{builtConfig["KeyVaultName"]}.vault.azure.net/" ),
                new DefaultAzureCredential() );
            config.AddAzureKeyVault( secretClient, new KeyVaultSecretManager() );
        }
    } )
    .ConfigureLogging( ( context, logging ) =>
    {
        logging.ClearProviders();
        logging.SetMinimumLevel( Microsoft.Extensions.Logging.LogLevel.Trace );

        if ( context.HostingEnvironment.IsDevelopment() )
        {
            logging.AddConsole();
            logging.AddDebug();
            logging.AddEventSourceLogger();
        }
    } )
    .UseNLog()
    .UseLamar( ( context, registry ) =>
    {
        ConfigureContainer( registry );
        ConfigureServices( registry, builder.Environment );
    } );

builder.Services.AddHealthChecks();

var app = builder.Build();
app.MapHealthChecks( "/health" );

Configure( app, app.Services.GetRequiredService<IDbInitializer>() );

app.Run();

// -----------------------------------------------------------------------------
// Service configuration
// -----------------------------------------------------------------------------

void ConfigureServices( IServiceCollection services, IWebHostEnvironment env )
{
    // Database
    services.AddDbContext<ApplicationDbContext>( options =>
        options.UseSqlServer( builder.Configuration.GetConnectionString( "DefaultConnection" ) ) );

    // Configuration
    var emailConfig = builder.Configuration.GetSection( "EmailConfiguration" ).Get<EmailConfiguration>();
    services.AddSingleton( emailConfig );

    // Identity token providers
    services.Configure<EmailConfirmationTokenProviderOptions>( opt => opt.TokenLifespan = TimeSpan.FromDays( 3 ) );
    services.Configure<DataProtectionTokenProviderOptions>( opt => opt.TokenLifespan = TimeSpan.FromHours( 2 ) );

    // Application cookie
    services.ConfigureApplicationCookie( config =>
    {
        config.Cookie.Name = "IdentityServer.Cookie";
        config.LoginPath = "/Home/Login";
    } );

    // Session
    services.AddSession( options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes( 1 );
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    } );

    // Data protection
    services.AddDataProtection()
        .PersistKeysToDbContext<ApplicationDbContext>()
        .SetApplicationName( "28764B8A-7849-4EB3-8647-9CAD82AFED67" );

    // Antiforgery
    services.AddAntiforgery( options => options.HeaderName = "X-XSRF-TOKEN" );

    if ( env.IsDevelopment() )
    {
        services.AddHttpLogging( _ => { } );
    }

    // Authentication & authorization
    services.ConfigureAuthentication( builder.Configuration, env );
    services.AddAuthorization( options =>
    {
        options.AddPolicy( "ApiScope", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim( "scope", IdentityServerConstants.LocalApi.ScopeName );
        } );
        options.AddPolicy( "MustBeAdmin", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole( "Admin" );
        } );
        options.AddPolicy( "MustBeAdminApi", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim( "scope", IdentityServerConstants.LocalApi.ScopeName );
            policy.RequireRole( "Admin" );
        } );
        options.AddPolicy( "MustBeSubscriber", policy =>
            policy.Requirements.Add( new MustBeSubscriberRequirement() ) );
    } );
    services.AddScoped<IAuthorizationHandler, MustBeSubscriberAuthorizationHandler>();

    // MVC & API
    ConfigureControllers( services );

    // Swagger
    services.AddSwaggerGen( options => options.CustomSchemaIds( x => x.FullName ) );

    // Application Insights
    var aiConn = builder.Configuration["ApplicationInsights:ConnectionString"]
        ?? Environment.GetEnvironmentVariable( "APPLICATIONINSIGHTS_CONNECTION_STRING" );
    if ( !string.IsNullOrWhiteSpace( aiConn ) )
    {
        services.AddOpenTelemetry().UseAzureMonitor( options => options.ConnectionString = aiConn );
    }
}

static void ConfigureControllers( IServiceCollection services )
{
    services.AddControllersWithViews( options =>
    {
        options.Filters.Add<HttpResponseExceptionFilter>();
        options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
        options.Filters.Add<SubscriptionFinderFilter>();
        options.Filters.Add<WarriorsActionFilter>();
    } )
    .AddRazorRuntimeCompilation()
    .AddNewtonsoftJson( options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.Converters.Add( new Newtonsoft.Json.Converters.StringEnumConverter() );
    } )
    .ConfigureApiBehaviorOptions( options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var result = new BadRequestObjectResult( context.ModelState );
            result.ContentTypes.Add( MediaTypeNames.Application.Json );
            result.ContentTypes.Add( MediaTypeNames.Application.Xml );
            return result;
        };
    } )
    .AddJsonOptions( options =>
    {
        options.JsonSerializerOptions.DictionaryKeyPolicy = null;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add( new System.Text.Json.Serialization.JsonStringEnumConverter() );
    } );
}

// -----------------------------------------------------------------------------
// Lamar container (DI)
// -----------------------------------------------------------------------------

void ConfigureContainer( ServiceRegistry services )
{
    services.Scan( s =>
    {
        s.AssembliesFromApplicationBaseDirectory( a => a.FullName!.StartsWith( "WarriorsGuild" ) );
        s.WithDefaultConventions( ServiceLifetime.Scoped );
    } );

    services.AddScoped<IGuildDbContext, ApplicationDbContext>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.For<Stripe.InvoiceItemService>().Use( new Stripe.InvoiceItemService() );
    services.For<Stripe.CustomerService>().Use( new Stripe.CustomerService() );
    services.For<Stripe.SubscriptionService>().Use( new Stripe.SubscriptionService() );
    services.For<Stripe.PlanService>().Use( new Stripe.PlanService() );
    services.For<Stripe.ProductService>().Use( new Stripe.ProductService() );

    services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomClaimsFactory>();
    services.AddScoped<IEmailSender, EmailSender>();
    services.AddScoped<IDateTimeProvider, Helpers>();
    services.AddScoped<IEmailValidator, Helpers>();
    services.AddScoped<IAddOnPriceOptionRepository, AddOnPriceOptionRepository>();
    services.AddScoped<IAccountRepository, AccountRepository>();
}

// -----------------------------------------------------------------------------
// HTTP pipeline
// -----------------------------------------------------------------------------

void Configure( WebApplication app, IDbInitializer dbInitializer )
{
    var env = app.Environment;

    if ( env.IsDevelopment() )
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler( "/Home/Error" );
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseSession();
    if ( env.IsDevelopment() )
    {
        app.UseHttpLogging();
    }
    app.UseRouting();
    app.UseCookiePolicy();
    app.UseAuthentication();
    app.UseIdentityServer();
    app.UseAuthorization();

    if ( env.IsDevelopment() )
    {
        app.Use( async ( ctx, next ) =>
        {
            var path = ctx.Request.Path + ctx.Request.QueryString;
            var method = ctx.Request.Method;
            await next();
            var controller = ctx.Request.RouteValues["controller"]?.ToString();
            var action = ctx.Request.RouteValues["action"]?.ToString();
            var routeInfo = string.IsNullOrEmpty( controller ) ? ( ctx.GetEndpoint()?.DisplayName ?? "static" ) : $"{controller}/{action}";
            var logger = ctx.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation( "Request: {Method} {Path} -> {RouteInfo} -> {StatusCode}", method, path, routeInfo ?? "-", ctx.Response.StatusCode );
        } );
    }

    if ( env.IsDevelopment() )
    {
        app.UseSwagger();
        app.UseSwaggerUI( c => c.SwaggerEndpoint( "/swagger/v1/swagger.json", "Warriors Guild" ) );
    }

    app.Use( async ( ctx, next ) =>
    {
        ctx.Response.Headers.Append( "Content-Security-Policy",
            "default-src 'self' 'unsafe-inline' 'unsafe-eval' " +
            "https://fonts.gstatic.com https://www.googletagmanager.com https://connect.facebook.net https://js.stripe.com " +
            "https://www.google-analytics.com https://maxcdn.bootstrapcdn.com https://az416426.vo.msecnd.net https://dc.services.visualstudio.com " +
            "d79i1fxsrar4t.cloudfront.net https://us-street.api.smartystreets.com/ https://us-autocomplete.api.smartystreets.com https://fonts.googleapis.com " +
            "http://www.youtube.com/ https://www.sprymedia.co.uk/;" );
        await next();
    } );

    StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

    app.MapControllerRoute( "default", "{controller=Home}/{action=Index}/{id?}" );
    app.MapControllerRoute( "DefaultApi", "api/{controller=Home}/{action=Index}/{id?}" );
    app.MapRazorPages();
}

// -----------------------------------------------------------------------------
// Authentication configuration (extension)
// -----------------------------------------------------------------------------

file static class ServiceExtensions
{
    public static void ConfigureAuthentication( this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env )
    {
        var host = configuration["Authentication:IdentityServer:Host"]
            ?? ( env.IsDevelopment() ? "https://localhost:5000" : null );

        services.AddIdentity<ApplicationUser, IdentityRole>( opt =>
        {
            opt.Password.RequiredLength = 6;
            opt.Password.RequireNonAlphanumeric = true;
            opt.Password.RequireDigit = true;
            opt.Password.RequireLowercase = true;
            opt.Password.RequireUppercase = true;
            opt.SignIn.RequireConfirmedEmail = true;
            opt.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";
        } )
        .AddDefaultUI()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddTokenProvider<EmailConfirmationTokenProvider<ApplicationUser>>( "emailconfirmation" );

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddAuthentication( options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultChallengeScheme = "oidc";
        } )
        .AddJwtBearer( "Bearer", options =>
        {
            options.Authority = host;
            options.Audience = IdentityServerConstants.LocalApi.ScopeName;
            options.TokenValidationParameters = new TokenValidationParameters { ValidateAudience = false };
        } )
        .AddGoogle( options =>
        {
            options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            var section = configuration.GetSection( "Authentication:Google" );
            options.ClientId = section["ClientId"];
            options.ClientSecret = section["ClientSecret"];
            options.Scope.Add( "email" );
        } )
        .AddFacebook( options =>
        {
            options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            options.AppId = configuration["Authentication:Facebook:AppId"];
            options.AppSecret = configuration["Authentication:Facebook:AppSecret"];
        } )
        .AddOpenIdConnect( "oidc", options =>
        {
            options.Authority = host;
            options.ClientId = "warriorsGuildMVC";
            options.ClientSecret = configuration["Authentication:IdentityServer:SecretKey"];
            options.GetClaimsFromUserInfoEndpoint = true;
            options.ResponseType = "code id_token";
            options.Scope.Add( "profile" );
            options.Scope.Add( "roles" );
            options.Scope.Add( "offline_access" );
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = "name",
                RoleClaimType = "role"
            };
            options.ClaimActions.Add( new JsonKeyClaimAction( "role", null, "role" ) );
            options.ClaimActions.MapJsonKey( "role", "role" );
            options.SaveTokens = true;
        } );

        services.AddIdentityServer()
            .AddDeveloperSigningCredential()
            .AddInMemoryIdentityResources( Config.IdentityResources )
            .AddInMemoryApiScopes( Config.ApiScopes )
            .AddInMemoryApiResources( Config.Apis )
            .AddInMemoryClients( Config.GetClients( configuration ) )
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<ProfileService>();

        services.AddLocalApiAuthentication();
    }
}
