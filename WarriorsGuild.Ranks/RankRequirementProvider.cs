using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Email;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Providers;
using WarriorsGuild.Ranks.Mappers;
using WarriorsGuild.Ranks.ViewModels;
using WarriorsGuild.Storage;

namespace WarriorsGuild.Ranks
{
    public class RankRequirementProvider : IRankRequirementProvider
    {
        private readonly IRankStatusProvider rankStatusProvider;
        private IRankMapper _rankMapper { get; }

        private IGuildDbContext _dbContext { get; }
        private IRankRepository _repo { get; }

        public RankRequirementProvider( IGuildDbContext dbContext, IRankRepository repo, IRankMapper rankMapper, IRankStatusProvider rankStatusProvider )
        {
            _dbContext = dbContext;
            _repo = repo;
            this.rankStatusProvider = rankStatusProvider;
            _rankMapper = rankMapper;
        }

        public async Task<IEnumerable<MinimalCrossDetail>> GetCrossesForRankReq( Guid rankId, Guid requirementId )
        {
            return await _repo.CrossesForRankReq( rankId, requirementId );
        }

        public async Task<RankRequirement> GetRequirementAsync( Guid rankId, Guid requirementId )
        {
            return await _repo.GetRequirements( rankId ).SingleOrDefaultAsync( rr => rr.RankId == rankId && rr.Id == requirementId );
        }

        public async Task<IEnumerable<RankRequirementViewModel>> GetRequirementsWithStatus( Guid rankId, Guid userIdForStatuses )
        {
            var requirements = await _repo.GetRequirements( rankId ).ToArrayAsync();
            if ( requirements == null || !requirements.Any() ) return null;

            var statuses = await rankStatusProvider.GetStatusesAsync( rankId, userIdForStatuses );

            var result = new List<RankRequirementViewModel>();
            foreach ( var requirement in requirements )
            {
                var status = statuses?.FirstOrDefault( s => s.RankRequirementId == requirement.Id );
                IEnumerable<MinimalRingDetail> rings = new MinimalRingDetail[ 0 ];
                IEnumerable<MinimalCrossDetail> crosses = new MinimalCrossDetail[ 0 ];
                IEnumerable<MinimalGoalDetail> attachments = new MinimalGoalDetail[ 0 ];
                if ( requirement.RequireRing && status != null )
                {
                    rings = await rankStatusProvider.GetRingsForRankStatus( requirement.RankId, requirement.Id, userIdForStatuses );
                }
                if ( requirement.RequireCross )
                {
                    crosses = await GetCrossesForRankReq( requirement.RankId, requirement.Id );
                }
                if ( requirement.RequireAttachment && status != null )
                {
                    attachments = await rankStatusProvider.GetAttachmentsForRankStatus( requirement.Id, userIdForStatuses );
                }
                result.Add( _rankMapper.CreateRequirementViewModel( requirement, status?.WarriorCompleted, status?.GuardianCompleted, rings, crosses, attachments ) );
            }
            return result;
        }

        public async Task UpdateRequirementsAsync( Guid rankId, IEnumerable<RankRequirementViewModel> requirements )
        {
            var existingRequirements = await _repo.GetRequirements( rankId ).ToArrayAsync();
            await _repo.UpdateRequirementsAsync( rankId, requirements, existingRequirements );
            await _dbContext.SaveChangesAsync();
        }
    }
}
