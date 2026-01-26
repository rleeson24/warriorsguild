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
            return new List<Client>
            {
                new Client
                {
                    ClientId = "warriorsGuildMAUI",
                    ClientName = "Warrior Guild Mobile",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
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
                    AccessTokenLifetime = 1
                },
                new Client
                {
                    ClientId = "warriorsGuildMVC",
                    ClientName = "Warrior Guild Client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
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
                    AccessTokenLifetime = 1
                },
                new Client
                {
                    ClientId = "js",
                    ClientName = "JavaScript Client",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true,

                    RedirectUris =           { $"{host}/callback.html" },
                    PostLogoutRedirectUris = { $"{host}" },
                    AllowedCorsOrigins =     { $"{host}" },
                    AllowOfflineAccess = true,

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1",
                        IdentityServerConstants.LocalApi.ScopeName,
                        "role"
                    },
                    AccessTokenLifetime = 1
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
