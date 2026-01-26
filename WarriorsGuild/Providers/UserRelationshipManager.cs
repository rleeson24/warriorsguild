using Microsoft.EntityFrameworkCore;
using WarriorsGuild.Areas.Payments;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.DataAccess;

namespace WarriorsGuild.Providers
{
    public interface IUserRelationshipManager
    {
        Task AddUserAsChildToGivenUser( ApplicationUser user, string myUserId, UserRole role );
    }
    public class UserRelationshipManager : IUserRelationshipManager
    {
        private readonly IGuildDbContext _appDbContext;

        public readonly ISubscriptionRepository SubscriptionRepository;

        public UserRelationshipManager( IGuildDbContext appDbContext, ISubscriptionRepository subscriptionRepository )
        {
            _appDbContext = appDbContext;
            SubscriptionRepository = subscriptionRepository;
        }

        public async Task AddUserAsChildToGivenUser( ApplicationUser user, string myUserId, UserRole role )
        {
            var users = _appDbContext.Users.Where( u => u.Id == myUserId ).Include( u => u.ChildUsers );
            var u2 = users.First();
            u2.ChildUsers.Add( user );
            _appDbContext.Entry( user ).State = EntityState.Unchanged;
            //foreach ( var rol in user.Roles )
            //{
            //    AppDbContext.Entry( rol ).State = EntityState.Unchanged;
            //}

            await _appDbContext.SaveChangesAsync();

            var mySub = await SubscriptionRepository.GetMySubscriptionAsync( myUserId );
            if ( mySub != null )
            {
                await SubscriptionRepository.AddUserToSubscription( user.Id, mySub.BillingAgreement.Id, role );
            }
        }
    }
}