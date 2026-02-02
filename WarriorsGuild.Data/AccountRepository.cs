using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WarriorsGuild.Data.Models;

namespace WarriorsGuild.DataAccess
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IGuildDbContext _context;

        public AccountRepository( IGuildDbContext context )
        {
            _context = context;
        }

        public async Task<string[]> GetGuardianEmailsForWarriorAsync( Guid warriorUserId )
        {
            return await _context.Users
                .Where( u => u.ChildUsers.Any( cu => cu.Id == warriorUserId.ToString() ) )
                .Select( g => g.Email )
                .ToArrayAsync();
        }

        public async Task<ApplicationUser> GetUserByIdAsync( Guid userId )
        {
            return await _context.Users.FindAsync( userId.ToString() );
        }
    }
}
