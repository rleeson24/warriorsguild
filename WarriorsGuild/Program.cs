using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using IdentityServer4;
using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog.Web;
using Stripe;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
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
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder( args );

builder.Host.ConfigureAppConfiguration( ( context, config ) =>
    {
        //logger.Info( $"{context.HostingEnvironment.EnvironmentName} is production: {context.HostingEnvironment.IsProduction()}" );
        //logger.Info( $"{context.HostingEnvironment.EnvironmentName} is staging: {context.HostingEnvironment.IsStaging()}" );
        if ( context.HostingEnvironment.IsProduction() )
        {
            var builtConfig = config.Build();
            //logger.Info( $"KeyVault uri: https://{builtConfig[ "KeyVaultName" ]}.vault.azure.net/" );
            var secretClient = new SecretClient( new Uri( $"https://{builtConfig[ "KeyVaultName" ]}.vault.azure.net/" ),
                                                        new DefaultAzureCredential() );
            config.AddAzureKeyVault( secretClient, new KeyVaultSecretManager() );
        }
    } )
    .ConfigureLogging( logging =>
    {
        logging.ClearProviders();
        logging.SetMinimumLevel( Microsoft.Extensions.Logging.LogLevel.Trace );
    } )
    .UseNLog()
    .UseLamar( ( context, registry ) =>
    {
        ConfigureContainer( registry );
        // Add services to the container.
        ConfigureServices( registry, builder.Environment );
    } );
builder.Services.AddHealthChecks();
//builder.Services.AddAzureClients(clientBuilder =>
//{
//	clientBuilder.AddBlobServiceClient(builder.Configuration["BlobStorage:ConnectionString:blob"], preferMsi: true);
//	clientBuilder.AddQueueServiceClient(builder.Configuration["BlobStorage:ConnectionString:queue"], preferMsi: true);
//});
var app = builder.Build();
app.MapHealthChecks( "/health" );

Configure( app, app.Services.GetRequiredService<IDbInitializer>() );

//// Configure the HTTP request pipeline.
//if ( app.Environment.IsDevelopment() )
//{
//    app.UseMigrationsEndPoint();
//}
app.Run();

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// This method gets called by the runtime. Use this method to add services to the container.
void ConfigureServices( IServiceCollection services, IWebHostEnvironment env )
{
    services.AddDbContext<ApplicationDbContext>( options =>
         options.UseSqlServer(
             builder.Configuration.GetConnectionString( "DefaultConnection" ) ) );
    var emailConfig = builder.Configuration
        .GetSection( "EmailConfiguration" )
        .Get<EmailConfiguration>();
    services.AddSingleton( emailConfig );

    services.Configure<EmailConfirmationTokenProviderOptions>( opt => opt.TokenLifespan = TimeSpan.FromDays( 3 ) );

    services.Configure<DataProtectionTokenProviderOptions>( opt => opt.TokenLifespan = TimeSpan.FromHours( 2 ) );

    //services.AddDistributedMemoryCache();

    services.ConfigureApplicationCookie( config =>
    {
        config.Cookie.Name = "IdentityServer.Cookie";
        config.LoginPath = "/Home/Login";
    } );
    services.AddSession( options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes( 1 );
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    } );
    services.AddDataProtection()
        .PersistKeysToDbContext<ApplicationDbContext>()
        .SetApplicationName( "28764B8A-7849-4EB3-8647-9CAD82AFED67" );
    //    .PersistKeysToFileSystem( new DirectoryInfo( @"./" ) );
    //.PersistKeysToAzureBlobStorage( new Uri( "<blobUriWithSasToken>" ) )
    //.PersistKeysToAzureBlobStorage( "<storage account connection string", "<key store container name>", "<key store blob name>" );
    //.ProtectKeysWithAzureKeyVault( new Uri( "<keyIdentifier>" ), new DefaultAzureCredential() );

    services.AddAntiforgery( options => options.HeaderName = "X-XSRF-TOKEN" );
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
        {
            policy.Requirements.Add( new MustBeSubscriberRequirement() );
        } );
    } );
    services.AddScoped<IAuthorizationHandler, MustBeSubscriberAuthorizationHandler>();

    //services.Configure<FormOptions>( o =>
    //{
    //    o.ValueLengthLimit = int.MaxValue;
    //    o.MultipartBodyLengthLimit = int.MaxValue;
    //    o.MemoryBufferThreshold = int.MaxValue;
    //} );
    ConfigureControllersWithPages( services );

    ConfigureMvc( services );


    // Register the Swagger generator, defining 1 or more Swagger documents
    services.AddSwaggerGen( options =>
    {
        options.CustomSchemaIds( x => x.FullName );
    } );

    services.AddApplicationInsightsTelemetry();
}

static void ConfigureControllersWithPages( IServiceCollection services )
{
    services.AddControllersWithViews( options =>
    {
        options.Filters.Add<HttpResponseExceptionFilter>();
        options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
    } )
    .AddRazorRuntimeCompilation()
    .AddMvcOptions( options =>
    {
        options.Filters.Add<SubscriptionFinderFilter>();
        options.Filters.Add<WarriorsActionFilter>();
        options.Filters.Add<HttpResponseExceptionFilter>();
        options.Filters.Add( new AutoValidateAntiforgeryTokenAttribute() );
        // options.EnableEndpointRouting = false;
    } )
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

static void ConfigureMvc( IServiceCollection services )
{
    //services.AddRazorPages();
    //.AddJsonOptions( options =>
    //{
    //    options
    //    options.JsonSerializerOptions.DictionaryKeyPolicy = null;
    //    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    //} );
}

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
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseSession();

    if ( env.IsDevelopment() )
    {
        // Enable middleware to serve generated Swagger as a JSON endpoint.
        app.UseSwagger();

        // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
        // specifying the Swagger JSON endpoint.
        app.UseSwaggerUI( c =>
        {
            c.SwaggerEndpoint( "/swagger/v1/swagger.json", "Warriors Guild" );
        } );
    }

    app.UseRouting();

    app.UseCookiePolicy();
    app.UseIdentityServer();
    app.UseAuthorization();

    app.Use( async ( ctx, next ) =>
    {
        ctx.Response.Headers.Add( "Content-Security-Policy",
                                 "default-src 'self' 'unsafe-inline' 'unsafe-eval' https://fonts.gstatic.com https://www.googletagmanager.com https://connect.facebook.net https://js.stripe.com " +
                                 "https://www.google-analytics.com https://maxcdn.bootstrapcdn.com https://az416426.vo.msecnd.net https://dc.services.visualstudio.com " +
                                 "d79i1fxsrar4t.cloudfront.net https://us-street.api.smartystreets.com/ https://us-autocomplete.api.smartystreets.com https://fonts.googleapis.com " +
                                 "http://www.youtube.com/ https://www.sprymedia.co.uk/;" );
        await next();
    } );

    dbInitializer.SeedAsync().Wait();

    StripeConfiguration.ApiKey = builder.Configuration[ "Stripe:SecretKey" ];

    app.UseEndpoints( endpoints =>
    {
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}" );
        endpoints.MapControllerRoute(
            name: "DefaultApi",
            pattern: "api/{controller=Home}/{action=Index}/{id?}" );
        endpoints.MapRazorPages();
    } );
}


// Take in Lamar's ServiceRegistry instead of IServiceCollection
// as your argument, but fear not, it implements IServiceCollection
// as well
void ConfigureContainer( ServiceRegistry services )
{
    // Also exposes Lamar specific registrations
    // and functionality
    services.Scan( s =>
    {
        //s.TheCallingAssembly();
        s.AssembliesFromApplicationBaseDirectory( a => a.FullName!.StartsWith( "WarriorsGuild" ) );
        s.WithDefaultConventions( ServiceLifetime.Scoped );
    } );

    // Using ASP.Net Core DI style registrations
    services.AddScoped<IGuildDbContext, ApplicationDbContext>();
    services.For<Stripe.InvoiceItemService>().Use( new Stripe.InvoiceItemService() );
    services.For<Stripe.CustomerService>().Use( new Stripe.CustomerService() );
    services.For<Stripe.SubscriptionService>().Use( new Stripe.SubscriptionService() );
    services.For<Stripe.PlanService>().Use( new Stripe.PlanService() );
    services.For<Stripe.ProductService>().Use( new Stripe.ProductService() );
    services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomClaimsFactory>();
    //services.AddScoped<IEmailProvider, EmailProvider>();
    services.AddScoped<IEmailSender, EmailSender>();
    //services.AddScoped<IDbInitializer, DbInitializer>();

    //services.AddScoped<ICrossRepository, CrossRepository>();
    //services.AddScoped<ICrossProvider, CrossProvider>();
    //services.AddScoped<ICrossMapper, CrossMapper>();

    //services.AddScoped<ICovenantProvider, CovenantProvider>();

    //services.AddScoped<IRecordCompletion, RecordCompletion>();
    //services.AddScoped<IRankValidator, RankValidator>();
    //services.AddScoped<IRanksProvider, RanksProvider>();
    //services.AddScoped<IRankRepository, RankRepository>();
    //services.AddScoped<IRankMapper, RankMapper>();
    //services.AddScoped<IRanksProviderHelpers, RanksProviderHelpers>();
    //services.AddScoped<IBlobProvider, BlobProvider>();
    //services.AddScoped<IHelpers, Helpers>();

    //services.AddScoped<IRecordRingCompletion, RecordRingCompletion>();
    //services.AddScoped<IRingValidator, RingValidator>();
    //services.AddScoped<IRingsProvider, RingsProvider>();
    //services.AddScoped<IRingRepository, RingRepository>();
    //services.AddScoped<IRingMapper, RingMapper>();

    //services.AddScoped<IMultipartFormReader, MultipartFormReader>();
    //services.AddScoped<IFileSystemProvider, FileSystemProvider>();



    //services.AddScoped<IGuardianIntroProvider, GuardianIntroProvider>();
    //services.For<IFileProvider>().Use<BlobProvider>();
    services.AddScoped<UserManager<ApplicationUser>>();
    //services.AddScoped<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>();
    //services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
}


public static class ServiceExtensions
{
    public static void ConfigureAuthentication( this IServiceCollection services, IConfiguration Configuration, IWebHostEnvironment env )
    {
        var host = Configuration[ "Authentication:IdentityServer:Host" ]; //https://localhost:5000
        services.AddIdentity<ApplicationUser, IdentityRole>( opt =>
        {
            opt.Password.RequiredLength = 6;
            opt.Password.RequireNonAlphanumeric = true;
            opt.Password.RequireDigit = true;
            opt.Password.RequireLowercase = true;
            opt.Password.RequireUppercase = true;
            //opt.User.RequireUniqueEmail = true;
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
            options.DefaultScheme = "Cookies";
            options.DefaultChallengeScheme = "oidc";
        } )
        .AddJwtBearer( "Bearer", options =>
        {
            options.Authority = host;
            options.Audience = IdentityServerConstants.LocalApi.ScopeName;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false
            };
        } )
        //.AddIdentityServerAuthentication(options =>
        //{
        //    options.Authority = "http://localhost:5000";//IdentityServer URL
        //    options.RequireHttpsMetadata = false;       //False for local addresses, true ofcourse for live scenarios
        //    options.ApiName = "ApiName";
        //    options.ApiSecret = "secret_for_the_api";
        //    //options.EnableCaching;
        //    //options.CacheDuration;
        //} )
        .AddGoogle( options =>
        {
            options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

            IConfigurationSection googleAuthNSection =
                Configuration.GetSection( "Authentication:Google" );

            options.ClientId = googleAuthNSection[ "ClientId" ];
            options.ClientSecret = googleAuthNSection[ "ClientSecret" ];
            options.Scope.Add( "email" );
        } )
        .AddFacebook( facebookOptions =>
        {
            facebookOptions.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            facebookOptions.AppId = Configuration[ "Authentication:Facebook:AppId" ];
            facebookOptions.AppSecret = Configuration[ "Authentication:Facebook:AppSecret" ];
            //facebookOptions.AccessDeniedPath = "/AccessDeniedPathInfo";

            //We recommend the AccessDeniedPath page contain the following information:

            //Remote authentication was canceled.
            //This app requires authentication.
            //To try sign -in again, select the Login link.
        } )
        .AddOpenIdConnect( "oidc", options =>
        {
            options.Authority = host;

            options.ClientId = "warriorsGuildMVC";
            options.ClientSecret = Configuration[ "Authentication:IdentityServer:SecretKey" ];
            options.GetClaimsFromUserInfoEndpoint = true;
            options.ResponseType = "code id_token";
            options.Scope.Add( "profile" );
            options.Scope.Add( "roles" );
            options.Scope.Add( "offline_access" );
            //options.SignedOutCallbackPath = "/";
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = "name",
                RoleClaimType = "role"
            };
            options.ClaimActions.Add( new JsonKeyClaimAction( "role", null, "role" ) );
            options.SaveTokens = true;
            options.ClaimActions.MapJsonKey( "role", "role" );
        } )
        .AddCookie( "Cookie" );

        //if ( env.IsDevelopment() )
        //{
        //    services.AddIdentityServer()
        //    .AddDeveloperSigningCredential()
        //    .AddInMemoryIdentityResources( Config.IdentityResources )
        //    .AddInMemoryClients( Config.GetClients( Configuration ) )
        //    .AddInMemoryApiResources( Config.Apis )
        //    .AddInMemoryApiScopes( Config.ApiScopes )
        //    .AddAspNetIdentity<ApplicationUser>();
        //    //.AddTestUsers( TestUsers.Users );
        //}
        //else
        //{
        var migrationsAssembly = typeof( Config ).GetTypeInfo().Assembly.GetName().Name;
        services.AddIdentityServer()
            .AddDeveloperSigningCredential()
            .AddConfigurationStore( options =>
            {
                options.ConfigureDbContext = builder => builder.UseSqlServer( Configuration.GetConnectionString( "DefaultConnection" ), sql => sql.MigrationsAssembly( migrationsAssembly ) );
            } )
            .AddOperationalStore( options =>
            {
                options.ConfigureDbContext = builder => builder.UseSqlServer( Configuration.GetConnectionString( "DefaultConnection" ), sql => sql.MigrationsAssembly( migrationsAssembly ) );

                // this enables automatic token cleanup. this is optional. 
                //options.EnableTokenCleanup = true;
                //options.TokenCleanupInterval = 30;
            } )
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<ProfileService>();
        //}
        services.AddLocalApiAuthentication();
    }
}