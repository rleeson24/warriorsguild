using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models;

namespace WarriorsGuild.Providers
{
    public interface IUserProvider
    {
        Task<bool> UserIsRelatedToWarrior( Guid guardianUserId, Guid warriorUserId );
        Task<String> ValidateUserId( String userId );
        Task<IEnumerable<ApplicationUser>> GetChildUsers( String guardianUserId );
        Guid GetUserIdForStatuses( ClaimsPrincipal user );
        Guid GetMyUserId( ClaimsPrincipal user );
    }
}