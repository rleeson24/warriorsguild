using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WarriorsGuild.Crosses;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Crosses;
using WarriorsGuild.DataAccess;

namespace WarriorsGuild.Data
{
    public interface IDbInitializer
    {
        Task SeedAsync();
    }

    public class DbInitializer : IDbInitializer
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly PersistedGrantDbContext _persistedGrantContext;
        private readonly ConfigurationDbContext _configurationContext;
        private readonly ILogger _logger;
        private readonly ICrossRepository crossRepository;
        private readonly IConfiguration _configuration;

        public DbInitializer( ApplicationDbContext context,
            PersistedGrantDbContext persistedGrantContext,
            ConfigurationDbContext configurationContext,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<DbInitializer> logger,
            ICrossRepository crossRepository,
            IConfiguration configuration )
        {
            _persistedGrantContext = persistedGrantContext;
            _configurationContext = configurationContext;
            this.roleManager = roleManager;
            this.userManager = userManager;
            _logger = logger;
            this.crossRepository = crossRepository;
            this._configuration = configuration;
        }

        public async Task SeedAsync()
        {
            SeedRoles();
            SeedUsers();
            await SeedTemplateCrosses();
            await SeedIdentityServerAsync();
        }

        public void SeedRoles()
        {
            if ( !roleManager.RoleExistsAsync( "Guardian" ).Result )
            {
                var role = new IdentityRole();
                role.Name = "Guardian";
                var roleResult = roleManager.CreateAsync( role ).Result;
            }

            if ( !roleManager.RoleExistsAsync( "Warrior" ).Result )
            {
                var role = new IdentityRole();
                role.Name = "Warrior";
                var roleResult = roleManager.CreateAsync( role ).Result;
            }


            if ( !roleManager.RoleExistsAsync( "Admin" ).Result )
            {
                var role = new IdentityRole();
                role.Name = "Admin";
                var roleResult = roleManager.CreateAsync( role ).Result;
            }
        }

        public void SeedUsers()
        {
            var user1Result = userManager.FindByNameAsync( "rleeson24@gmail.com" ).Result;
            if ( user1Result == null )
            {
                ApplicationUser user = new ApplicationUser();
                user.UserName = "Bert";
                user.Email = "rleeson24@gmail.com";
                user.FirstName = "Robert";
                user.LastName = "Leeson";

                IdentityResult result = userManager.CreateAsync( user, "|K.fV:@:8qPOFck:Jn^1" ).Result;

                if ( result.Succeeded )
                {
                    userManager.AddToRoleAsync( user, "Admin" ).Wait();
                    userManager.AddToRoleAsync( user, "Guardian" ).Wait();
                }
            }
            var user2Result = userManager.FindByNameAsync( "scott@warriorsguild.com" ).Result;
            if ( user2Result == null )
            {
                ApplicationUser user = new ApplicationUser();
                user.UserName = "scott@warriorsguild.com";
                user.Email = "scott@warriorsguild.com";
                user.FirstName = "Scott";
                user.LastName = "Ward";

                IdentityResult result = userManager.CreateAsync( user, "|K.fV:@:8qPOFck:Jn^1" ).Result;

                if ( result.Succeeded )
                {
                    userManager.AddToRoleAsync( user, "Admin" ).Wait();
                    userManager.AddToRoleAsync( user, "Guardian" ).Wait();
                }
            }
            var user3Result = userManager.FindByNameAsync( "brian@warriorsguild.com" ).Result;
            if ( user3Result == null )
            {
                ApplicationUser user = new ApplicationUser();
                user.UserName = "brian@warriorsguild.com";
                user.Email = "brian@warriorsguild.com";
                user.FirstName = "Brian";
                user.LastName = "Jones";

                IdentityResult result = userManager.CreateAsync( user, "|K.fV:@:8qPOFck:Jn^1" ).Result;

                if ( result.Succeeded )
                {
                    userManager.AddToRoleAsync( user, "Admin" ).Wait();
                    userManager.AddToRoleAsync( user, "Guardian" ).Wait();
                }
            }
        }

        public async Task SeedTemplateCrosses()
        {
            var dayQuestionsGuid = new Guid( "df1e6a25-b570-4664-831b-2d31e1b03832" );
            var existingDayQuestions = await crossRepository.GetTemplateCross( dayQuestionsGuid ).ToArrayAsync();
            if ( existingDayQuestions == null )
            {
                await crossRepository.AddAsync( new Cross()
                {
                    Id = dayQuestionsGuid,
                    Name = "DayQuestions",
                    Description = "Cross that keeps track of day questions",
                    Index = 1000
                } );
                await crossRepository.AddQuestionsAsync( dayQuestionsGuid, new[]
                {
                    new CrossQuestion()
                    {
                        Text = "What jumped out at you while reading the passage?",
                        Index = 0
                    },
                    new CrossQuestion()
                    {
                        Text = "What does this say about God and how are we to respond?",
                        Index = 1
                    },
                    new CrossQuestion()
                    {
                        Text = "What is one thing that I am grateful for today?",
                        Index = 2
                    },
                    new CrossQuestion()
                    {
                        Text = "Who is one person I can pray for and how can I pray for them?",
                        Index = 3
                    }
                } );
            }
            var summaryQuestionsGuid = new Guid( "5893b2f4-6ca5-4eed-9ad1-8d1b512574a3" );
            var existingSummaryQuestions = await crossRepository.GetTemplateCross( summaryQuestionsGuid ).ToArrayAsync();
            if ( existingSummaryQuestions == null )
            {
                await crossRepository.AddAsync( new Cross()
                {
                    Id = summaryQuestionsGuid,
                    Name = "CrossQuestions",
                    Description = "Cross that keeps track of summary questions",
                    Index = 1001
                } );
                await crossRepository.AddQuestionsAsync( summaryQuestionsGuid, new[]
                {
                    new CrossQuestion()
                    {
                        Text = "Identify and describe the background, setting, author, date, and key people for the book of {BookName}",
                        Index = 0
                    },
                    new CrossQuestion()
                    {
                        Text = "Write 1-5 sentences summarizing the overall theme of the book of {BookName}",
                        Index = 1
                    },
                    new CrossQuestion()
                    {
                        Text = "What is your favorite verse? Commit it to memory and recite for your Guardian during your commendation/ordination.",
                        Index = 2
                    },
                    new CrossQuestion()
                    {
                        Text = "In light of what you have just read, {explain}",
                        Index = 3
                    },
                    new CrossQuestion()
                    {
                        Text = "List 5 ways on how I can apply what I have learned from the book of {BookName} in my daily life.",
                        Index = 4
                    }
                } );
            }
        }


        public async Task SeedIdentityServerAsync()
        {
            await _persistedGrantContext.Database.MigrateAsync().ConfigureAwait( false );
            await _configurationContext.Database.MigrateAsync().ConfigureAwait( false );
            if ( !await _configurationContext.Clients.AnyAsync() )
            {
                _logger.LogInformation( "Seeding IdentityServer Clients" );
                foreach ( var client in Config.GetClients( _configuration ) )
                {
                    _configurationContext.Clients.Add( client.ToEntity() );
                }
                _configurationContext.SaveChanges();
            }
            if ( !await _configurationContext.IdentityResources.AnyAsync() )
            {
                _logger.LogInformation( "Seeding IdentityServer Identity Resources" );
                foreach ( var resource in Config.IdentityResources )
                {
                    _configurationContext.IdentityResources.Add( resource.ToEntity() );
                }
                _configurationContext.SaveChanges();
            }
            if ( !await _configurationContext.ApiResources.AnyAsync() )
            {
                _logger.LogInformation( "Seeding IdentityServer API Resources" );
                foreach ( var resource in Config.Apis )
                {
                    _configurationContext.ApiResources.Add( resource.ToEntity() );
                }
                _configurationContext.SaveChanges();
            }
            if ( !await _configurationContext.ApiScopes.AnyAsync() )
            {
                _logger.LogInformation( "Seeding IdentityServer API Scopes" );
                foreach ( var resource in Config.ApiScopes )
                {
                    _configurationContext.ApiScopes.Add( resource.ToEntity() );
                }
                _configurationContext.SaveChanges();
            }
        }
    }
}
