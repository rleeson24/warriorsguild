using System;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models;

namespace WarriorsGuild.DataAccess
{
    public interface IAccountRepository
    {
        Task<string[]> GetGuardianEmailsForWarriorAsync( Guid warriorUserId );
        Task<ApplicationUser> GetUserByIdAsync( Guid userId );
    }
}
