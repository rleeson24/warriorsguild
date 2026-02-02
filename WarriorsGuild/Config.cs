using IdentityServer4;
using IdentityServer4.Models;

namespace WarriorsGuild
{
    public class Config
    {
        public static IEnumerable<ApiScope> ApiScopes =>
           new List<ApiScope>
           {
            new ApiScope("api1", "My API"),
            new ApiScope(IdentityServerConstants.LocalApi.ScopeName, "WarriorsGuild")
           };

        public static IEnumerable<Client> GetClients( IConfiguration Configuration )
        {
            var host = Configuration[ "Authentication:IdentityServer:Host" ]; //https://localhost:5000
            if ( string.IsNullOrWhiteSpace( host ) )
            {
                host = "https://localhost:5000";
            }
            return new List<Client>
            {
                new Client
                {
                    ClientId = "warriorsGuildMAUI",
                    ClientName = "Warrior Guild Mobile",
                    // This client is used for interactive login from the MAUI/mobile client,
                    // enable the authorization code flow.
                    AllowedGrantTypes = GrantTypes.Code,
                    AlwaysSendClientClaims = true,
                    //UpdateAccessTokenClaimsOnRefresh = true,
                    //AlwaysIncludeUserClaimsInIdToken = true,

                    RequireConsent = false,

                    ClientSecrets =
                    {
                        new Secret(Configuration["Authentication:IdentityServer:SecretKey"].Sha256())
                    },

                    RedirectUris           = { $"{host}/signin-oidc" },
                    FrontChannelLogoutUri = $"{host}/signout-oidc",
                    PostLogoutRedirectUris = { $"{host}/signout-callback-oidc" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1",
                        "wg_api",
                        IdentityServerConstants.LocalApi.ScopeName,
                        "role"
                    },
                    AllowOfflineAccess = true,
                    AccessTokenLifetime = 3600,
                    AbsoluteRefreshTokenLifetime = 2592000,
                    SlidingRefreshTokenLifetime = 1296000
                },
                new Client
                {
                    ClientId = "warriorsGuildMVC",
                    ClientName = "Warrior Guild Client",
                    // This is the MVC application client which uses the authorization
                    // code flow (OIDC). Configure as such.
                    AllowedGrantTypes = GrantTypes.Code,
                    AlwaysSendClientClaims = true,
                    //UpdateAccessTokenClaimsOnRefresh = true,
                    //AlwaysIncludeUserClaimsInIdToken = true,

                    RequireConsent = false,

                    ClientSecrets =
                    {
                        new Secret(Configuration["Authentication:IdentityServer:SecretKey"].Sha256())
                    },

                    RedirectUris           = { $"{host}/signin-oidc" },
                    FrontChannelLogoutUri = $"{host}/signout-oidc",
                    PostLogoutRedirectUris = { $"{host}/signout-callback-oidc" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1",
                        "wg_api",
                        IdentityServerConstants.LocalApi.ScopeName,
                        "role"
                    },
                    AllowOfflineAccess = true,
                    AccessTokenLifetime = 3600,
                    AbsoluteRefreshTokenLifetime = 2592000,
                    SlidingRefreshTokenLifetime = 1296000
                },
                new Client
                {
                    ClientId = "js",
                    ClientName = "JavaScript Client",
                    // This is a public Single Page Application (SPA). Use the
                    // authorization code flow with PKCE (recommended for SPAs).
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true,

                    RedirectUris = { $"{host}/callback.html" },
                    PostLogoutRedirectUris = { $"{host}" },
                    AllowedCorsOrigins =     { $"{host}" },
                    // Explicitly enable this public client
                    Enabled = true,
                    AllowOfflineAccess = true,

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1",
                        IdentityServerConstants.LocalApi.ScopeName,
                        "role"
                    },
                    AccessTokenLifetime = 3600,
                    AbsoluteRefreshTokenLifetime = 2592000,
                    SlidingRefreshTokenLifetime = 1296000
                }
            };
        }

        public static IEnumerable<IdentityResource> IdentityResources =
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Address(),
                new IdentityResource("role", "User role(s)", new List<string> { "Admin","Warrior","Guardian" }),
                new IdentityResource("position", "Your position", new List<string> { "position" }),
                new IdentityResource("country", "Your country", new List<string> { "country" })
            };

        public static IEnumerable<ApiResource> Apis = new List<ApiResource>
            {
                //new ApiResource("api1", "My API"),
                //new ApiResource("wg_api", "WarriorsGuild API"),
                // local API
                new ApiResource(IdentityServerConstants.LocalApi.ScopeName)
            };
    }
}
