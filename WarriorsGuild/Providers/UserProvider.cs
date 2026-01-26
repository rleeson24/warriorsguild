using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WarriorsGuild.Data.Models;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Helpers.Utilities;

namespace WarriorsGuild.Providers
{

    public class UserProvider : IUserProvider
    {
        private ISessionManager _sessionManager;

        private IGuildDbContext _dbContext { get; }

        public UserProvider( IGuildDbContext dbContext,ISessionManager sessionManager )
        {
            _dbContext = dbContext;
            this._sessionManager = sessionManager;
        }

        public async Task<bool> UserIsRelatedToWarrior( Guid guardianUserId, Guid warriorUserId )
        {
            return await _dbContext.Set<ApplicationUser>().Include( u => u.ChildUsers ).AnyAsync( u => u.Id == guardianUserId.ToString() && u.ChildUsers.Any( c => c.Id == warriorUserId.ToString() ) );
        }

        public async Task<IEnumerable<ApplicationUser>> GetChildUsers( string guardianUserId )
        {
            return (await _dbContext.Set<ApplicationUser>().Include( u => u.ChildUsers ).SingleAsync( u => u.Id == guardianUserId )).ChildUsers;
        }

        public async Task<String> ValidateUserId( string userId )
        {
            if ( String.IsNullOrEmpty( userId ) )
            {
                return "No user was supplied for status update.";
            }
            else if ( await _dbContext.Set<ApplicationUser>().FindAsync( userId ) == null )
            {
                return "Invalid user was supplied for status update.";
            }
            return null;
        }

        public Guid GetUserIdForStatuses( ClaimsPrincipal user )
        {
            var userIdForStatuses = _sessionManager.UserIdForStatuses;
            userIdForStatuses = userIdForStatuses ?? user.FindFirstValue( claimType: "sub" );
            return userIdForStatuses != null ? Guid.Parse( userIdForStatuses ) : Guid.Empty;
        }

        public Guid GetMyUserId( ClaimsPrincipal user )
        {
            var userId = user.FindFirstValue( claimType: "sub" );
            return userId != null ? Guid.Parse( userId ) : Guid.Empty;
        }
    }
}