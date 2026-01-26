using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Crosses;
using WarriorsGuild.Data.Models.Crosses.Status;
using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.Data.Models.Ranks.Status;
using WarriorsGuild.Data.Models.Rings;
using WarriorsGuild.Data.Models.Rings.Status;
using WarriorsGuild.DataAccess.Models;

namespace WarriorsGuild.DataAccess
{

    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IGuildDbContext, IDataProtectionKeyContext
    {
        public static readonly ILoggerFactory loggerFactory = LoggerFactory.Create( builder =>
        {
            //TODO: only log when in production
            builder.AddFilter( ( category, level ) => true )
                                .AddConsole();
        } );

        public ApplicationDbContext( DbContextOptions<ApplicationDbContext> options )
            : base( options )
        {
        }

        protected override void OnConfiguring( DbContextOptionsBuilder optionsBuilder )
        {
            optionsBuilder.UseLoggerFactory( loggerFactory );
        }

        protected override void OnModelCreating( ModelBuilder modelBuilder )
        {
            base.OnModelCreating( modelBuilder );
            //modelBuilder.Entity<ApplicationUser>()
            //    .HasMany( p => p.ChildUsers )
            //    .WithMany()
            //    .Map( m =>
            //    {
            //        m.MapLeftKey( "Father_Id" );
            //        m.MapRightKey( "Son_Id" );
            //        m.ToTable( "father_son_relation" );
            //    } );


            modelBuilder.Entity<ProofOfCompletionAttachment>().HasIndex( m => new { m.RequirementId, m.UserId } );//.HasDatabaseName( "IX_Req_User" );
            modelBuilder.Entity<RankStatusCompletedCross>().HasKey( m => new { m.RankId, m.RankRequirementId, m.UserId, m.CrossId } );//.HasDatabaseName( "IX_UserId_Req" );
            modelBuilder.Entity<RankStatusCompletedRing>().HasKey( m => new { m.RankId, m.RankRequirementId, m.UserId, m.RingId } );//.HasDatabaseName( "IX_UserId_Req" );
            modelBuilder.Entity<PinnedCross>().HasKey( m => new { m.UserId, m.CrossId } );//.HasDatabaseName( "IX_UserId_Ring" );
            modelBuilder.Entity<PinnedRing>().HasKey( m => new { m.UserId, m.RingId } );//.HasDatabaseName( "IX_UserId_Ring" );
            modelBuilder.Entity<SubscriptionBillingAgreement>().HasKey( m => new { m.SubscriptionId, m.BillingAgreementId } );
            modelBuilder.Entity<UserSubscription>().HasKey( m => new { m.UserId, m.SubscriptionId, m.BillingAgreementId } );
            modelBuilder.Entity<CrossAnswer>().HasKey( m => new { m.CrossId, m.CrossQuestionId, m.UserId } );
            modelBuilder.Entity<PriceOption>().HasKey( m => new { m.Id } );
            modelBuilder.Entity<PriceOption>().HasIndex( m => new { m.Id, m.ModifiedDate } );
            modelBuilder.Entity<CrossDayStatus>().HasKey( m => new { m.Id } );
            modelBuilder.Entity<SignedCovenant>().HasKey( m => new { m.SignedAt, m.SignedBy } );
        }

        public override int SaveChanges()
        {
            foreach ( var entry in ChangeTracker.Entries() )
            {
                var entity = entry.Entity;
                if ( entity is EntityBase )
                {
                    if ( entry.State == EntityState.Added )
                    {
                        var props = typeof( EntityBase ).GetProperties();
                        var createdDate = props.FirstOrDefault( p => p.Name == "CreatedDate" );
                        if ( createdDate != null )
                        {
                            createdDate.SetValue( entity, DateTime.UtcNow );
                        }
                        var modifiedDate = props.FirstOrDefault( p => p.Name == "ModifiedDate" );
                        if ( modifiedDate != null )
                        {
                            modifiedDate.SetValue( entity, DateTime.UtcNow );
                        }
                    }
                    else if ( entry.State == EntityState.Modified )
                    {
                        var props = typeof( EntityBase ).GetProperties();
                        var modifiedDate = props.FirstOrDefault( p => p.Name == "ModifiedDate" );
                        if ( modifiedDate != null )
                        {
                            modifiedDate.SetValue( entity, DateTime.UtcNow );
                        }
                    }
                }
            }
            return base.SaveChanges();
        }
        public async Task<int> SaveChangesAsync() => await base.SaveChangesAsync();


        public void SetDeleted( object entity )
        {
            Entry( entity ).State = EntityState.Deleted;
        }

        public DbSet<MailingListEntry> MailingList { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<RankRequirement> RankRequirements { get; set; }
        public DbSet<RankStatus> RankStatuses { get; set; }
        public DbSet<RankStatusCompletedCross> RankStatusCrosses { get; set; }
        public DbSet<RankStatusCompletedRing> RankStatusRings { get; set; }
        public DbSet<RankApproval> RankApprovals { get; set; }

        public DbSet<Ring> Rings { get; set; }
        public DbSet<RingRequirement> RingRequirements { get; set; }
        public DbSet<RingStatus> RingStatuses { get; set; }
        public DbSet<RingApproval> RingApprovals { get; set; }

        public DbSet<PriceOption> PriceOptions { get; set; }
        public DbSet<PriceOptionPerk> PriceOptionPerks { get; set; }
        public DbSet<AddOnPriceOption> AddOnPriceOptions { get; set; }
        public DbSet<BillingAgreement> BillingAgreements { get; set; }
        //public DbSet<WarriorsGuild.Areas.Payments.Models.PaypalIPNDetail> PaypalIPNDetail { get; set; }

        public DbSet<WarriorsGuild.Areas.Payments.Models.Stripe.StripeWebhookMessage> StripeWebhookMessages { get; set; }
        public DbSet<PinnedRing> PinnedRings { get; set; }
        public DbSet<PinnedCross> PinnedCrosses { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<SubscriptionBillingAgreement> SubscriptionBillingAgreements { get; set; }
        public DbSet<Cross> Crosses { get; set; }
        public DbSet<CrossQuestion> CrossQuestions { get; set; }
        public DbSet<CrossApproval> CrossApprovals { get; set; }
        public DbSet<CrossAnswer> CrossAnswers { get; set; }
        public DbSet<ProofOfCompletionAttachment> ProofOfCompletionAttachments { get; set; }
        public DbSet<SingleUseFileDownloadKey> SingleUseFileDownloadKey { get; set; }
        public DbSet<InvitedEmailAddress> InvitedEmailAddresses { get; set; }
        public DbSet<CrossDay> CrossDays { get; set; }
        public DbSet<CrossDayAnswer> CrossDayAnswers { get; set; }
        public DbSet<CrossDayStatus> CrossDayStatuses { get; set; }
        public DbSet<SignedCovenant> SignedCovenants { get; set; }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<RankRequirementCross> RankCrosses { get; set; }
    }

    public class GuildDbFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {

        public ApplicationDbContext CreateDbContext( string[] args )
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer( "Server=(localdb)\\mssqllocaldb;Database=WarriorsGuild;Trusted_Connection=True;MultipleActiveResultSets=true" );
            return new ApplicationDbContext( optionsBuilder.Options );
        }
    }

    public class PersistedGrantDbContextFactory : IDesignTimeDbContextFactory<PersistedGrantDbContext>
    {
        public PersistedGrantDbContext CreateDbContext( string[] args )
        {
            var migrationsAssembly = "WarriorsGuild.Data";
            var localConnectionString = "Server=(localdb)\\mssqllocaldb;Database=WarriorsGuild;Trusted_Connection=True;MultipleActiveResultSets=true";
            var optionsBuilder = new DbContextOptionsBuilder<PersistedGrantDbContext>();
            optionsBuilder.UseSqlServer( localConnectionString, sql => sql.MigrationsAssembly( migrationsAssembly ) );
            var operStoreOptions = new IdentityServer4.EntityFramework.Options.OperationalStoreOptions();
            operStoreOptions.ConfigureDbContext = builder => builder.UseSqlServer( localConnectionString, sql => sql.MigrationsAssembly( migrationsAssembly ) );
            return new PersistedGrantDbContext( optionsBuilder.Options, operStoreOptions );
        }
    }

    public class ConfigurationDbContextFactory : IDesignTimeDbContextFactory<ConfigurationDbContext>
    {
        public ConfigurationDbContext CreateDbContext( string[] args )
        {
            var migrationsAssembly = "WarriorsGuild.Data";
            var localConnectionString = "Server=(localdb)\\mssqllocaldb;Database=WarriorsGuild;Trusted_Connection=True;MultipleActiveResultSets=true";
            var optionsBuilder = new DbContextOptionsBuilder<ConfigurationDbContext>();
            optionsBuilder.UseSqlServer( localConnectionString, sql => sql.MigrationsAssembly( migrationsAssembly ) );
            var operStoreOptions = new IdentityServer4.EntityFramework.Options.ConfigurationStoreOptions();
            operStoreOptions.ConfigureDbContext = builder => builder.UseSqlServer( localConnectionString, sql => sql.MigrationsAssembly( migrationsAssembly ) );
            return new ConfigurationDbContext( optionsBuilder.Options, operStoreOptions );
        }
    }
}