using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.Ranks.ViewModels;

namespace WarriorsGuild.Ranks
{
    public interface IRankRequirementProvider
    {
        Task<IEnumerable<MinimalCrossDetail>> GetCrossesForRankReq( Guid rankId, Guid requirementId );
        Task<RankRequirement> GetRequirementAsync( Guid rankId, Guid requirementId );
        Task<IEnumerable<RankRequirementViewModel>> GetRequirementsWithStatus( Guid rankId, Guid userIdForStatuses );
        Task UpdateRequirementsAsync( Guid rankId, IEnumerable<RankRequirementViewModel> requirements );
    }
}