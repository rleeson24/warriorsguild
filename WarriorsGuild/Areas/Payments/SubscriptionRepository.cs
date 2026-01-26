using Microsoft.EntityFrameworkCore;
using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Models.Payments;

namespace WarriorsGuild.Areas.Payments
{
    public interface ISubscriptionRepository
    {
        Task CreateSubscription( BillingAgreement ba, IEnumerable<UserSubscription> newSubscriptions );
        Task<BillingAgreement?> GetBillingAgreement( Guid id );
        MySubscription? GetMySubscription( string userId );
        Task<MySubscription?> GetMySubscriptionAsync( string userId );
        Task<IEnumerable<UserSubscription>> GetUsersOnSubscriptionAsync( Guid billingAgreementId );
        Task Unsubscribe( BillingAgreement billingAgreement, string userId );
        Task AddUserToSubscription( string userId, Guid billingAgreementId, UserRole role );
    }

    public class SubscriptionRepository : ISubscriptionRepository
    {
        private MySubscription? _mySubscription;

        private IGuildDbContext Db { get; }

        public SubscriptionRepository( IGuildDbContext db )
        {
            Db = db;
        }

        public MySubscription? GetMySubscription( string userId )
        {
            return GetMySubscriptionQueryable( userId ).FirstOrDefault();
        }

        public async Task<MySubscription?> GetMySubscriptionAsync( string userId )
        {
            if ( _mySubscription == null )
            {
                _mySubscription = await GetMySubscriptionQueryable( userId ).FirstOrDefaultAsync();
            }
            return _mySubscription;
        }

        public IQueryable<MySubscription> GetMySubscriptionQueryable( string userId )
        {
            return Db.BillingAgreements.Include( ba => ba.PriceOption ).Include( ba => ba.PriceOption.AdditionalWarriorPlan ).Include( ba => ba.PriceOption.AdditionalGuardianPlan ).Join( Db.UserSubscriptions.Where( us => us.UserId == new Guid( userId ) && !us.Revised.HasValue ),
                    ba => ba.Id,
                    us1 => us1.BillingAgreementId,
                    ( inner, outer ) => new MySubscription { UserSubscription = outer, BillingAgreement = inner } ).OrderByDescending( m => m.BillingAgreement.Created );
        }

        public async Task<BillingAgreement?> GetBillingAgreement( Guid id )
        {
            return await Db.BillingAgreements.FindAsync( id );
        }

        public async Task<IEnumerable<UserSubscription>> GetUsersOnSubscriptionAsync( Guid billingAgreementId )
        {
            return await Db.UserSubscriptions.Where( us => us.BillingAgreementId == billingAgreementId && !us.Revised.HasValue && Db.Users.Any( u => u.Id == us.UserId.ToString() ) ).ToArrayAsync();
        }

        public async Task CreateSubscription( BillingAgreement ba, IEnumerable<UserSubscription> newSubscriptions )
        {
            Db.BillingAgreements.Add( ba );
            Db.UserSubscriptions.AddRange( newSubscriptions );

            await Db.SaveChangesAsync();
        }

        public async Task AddUserToSubscription( string userId, Guid billingAgreementId, UserRole role )
        {
            var userSub = new UserSubscription()
            {
                UserId = new Guid( userId ),
                BillingAgreementId = billingAgreementId,
                Role = role
            };
            Db.UserSubscriptions.Add( userSub );
            await Db.SaveChangesAsync();
        }

        public async Task Unsubscribe( BillingAgreement billingAgreement, string userId )
        {
            Db.Entry( billingAgreement ).CurrentValues.SetValues( new { NextPaymentDue = DateTime.UtcNow, Cancelled = DateTime.UtcNow } );
            //Db.UserSubscriptions.Where( s => s.BillingAgreementId == billingAgreement.Id ).ToList().ForEach( s => { s.Revised = DateTime.UtcNow; s.RevisedBy = new Guid( userId ); } );
            await Db.SaveChangesAsync();
        }

        private bool BillingAgreementExists( Guid id )
        {
            return Db.BillingAgreements.Count( e => e.Id == id ) > 0;
        }
    }
}