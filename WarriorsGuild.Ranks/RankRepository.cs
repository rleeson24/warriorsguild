using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.Data.Models.Ranks.Status;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Models;
using WarriorsGuild.Ranks.ViewModels;

namespace WarriorsGuild.Ranks
{
    public interface IRankRepository
    {
        DbSet<Rank> Ranks { get; }
        DbSet<RankRequirement> Requirements { get; }
        DbSet<RankStatus> Statuses { get; }

        void Add( Rank rank );
        void AddStatusEntry( RankStatus newValues );
        Task DeleteRankAsync( Guid id );
        void DeleteRankStatus( RankStatus rankStatus );
        Rank Get( Guid id, bool includeRequirements = false );
        IQueryable<Rank> List();
        IQueryable<RankStatus> RankStatuses();
        void Update( RankStatus existingChild );
        void Update( Guid id, Rank existingRank );
        Task UpdateRequirementsAsync( Guid id, IEnumerable<RankRequirementViewModel> requirements, IEnumerable<RankRequirement> existingRequirements );
        void SetHasImage( Rank rank, string fileExtension );
        void SetHasGuide( Rank rank, string fileExtension );
        Task<RankStatus> GetRequirementStatusAsync( Guid rankId, Guid rankRequirementId, Guid userIdForStatuses );
        Task<Rank> GetHighestRankWithCompletionsAsync( Guid userIdForStatuses );
        IQueryable<RankRequirement> GetRequirements( Guid rankId );
        Task<Rank> GetRankByIndexAsync( int index, Guid userIdForStatuses );
        void UpdateOrder( IEnumerable<GoalIndexEntry> request );
        Task<int> GetMaxRankIndexAsync();
        void AddApprovalEntry( RankApproval approvalEntry );
        Task<IEnumerable<MinimalCrossDetail>> CrossesForRankReq( Guid rankId, Guid requirementId );
    }
    public class RankRepository : IRankRepository
    {
        private IGuildDbContext _dbContext { get; }

        public DbSet<Rank> Ranks { get => _dbContext.Ranks; }
        public DbSet<RankRequirement> Requirements { get => _dbContext.RankRequirements; }
        public DbSet<RankStatus> Statuses { get => _dbContext.RankStatuses; }

        public RankRepository( IGuildDbContext db )
        {
            _dbContext = db;
        }

        #region Ranks

        public IQueryable<Rank> List()
        {
            //OrderBy must come after DefaultIfEmpty() or Linq does not respect it
            return _dbContext.Ranks.OrderBy( r => r.Index );
        }

        public async Task<int> GetMaxRankIndexAsync()
        {
            var rankIndexes = from rank in _dbContext.Ranks
                              orderby rank.Index
                              select rank.Index;
            if ( rankIndexes.Any() )
            {
                return await rankIndexes.MaxAsync();
            }
            else
            {
                return 0;
            }
        }

        public void Add( Rank rank )
        {
            _dbContext.Ranks.Add( rank );
        }

        public Rank Get( Guid id, bool includeRequirements = false )
        {
            var result = List();
            if ( includeRequirements ) result = result.Include( r => r.Requirements );
            return result.SingleOrDefault( r => r.Id == id );
        }

        public void Update( Guid id, Rank existingRank )
        {
            // Update parent
            _dbContext.Entry( existingRank ).CurrentValues.SetValues( existingRank );
        }

        public IQueryable<RankRequirement> GetRequirements( Guid rankId )
        {
            return  _dbContext.RankRequirements.Where( rr => rr.RankId == rankId ).OrderBy( rr => rr.Index );
        }

        public async Task UpdateRequirementsAsync( Guid id, IEnumerable<RankRequirementViewModel> requirements, IEnumerable<RankRequirement> existingRequirements )
        {
            // Delete children
            foreach ( var existingChild in existingRequirements.ToList() )
            {
                if ( !requirements.Any( c => c.Id == existingChild.Id ) )
                {
                    _dbContext.Entry( existingChild ).State = EntityState.Deleted;
                }
            }

            var crossesToRemove = await _dbContext.RankCrosses.Where( rc => rc.RankId == id ).ToArrayAsync();
            _dbContext.RankCrosses.RemoveRange( crossesToRemove );

            // Update and Insert children
            foreach ( var childModel in requirements )
            {
                var existingChild = existingRequirements
                    .Where( c => c.Id == childModel.Id )
                    .SingleOrDefault();
                if ( !childModel.RequireRing ) childModel.RequiredRingCount = null;
                if ( !childModel.RequireCross ) childModel.RequiredCrossCount = null;
                if ( existingChild != null )
                    // Update child
                    _dbContext.Entry( existingChild ).CurrentValues.SetValues( childModel );
                else
                {
                    _dbContext.RankRequirements.Add( childModel );
                }

                foreach ( var cross in childModel.CrossesToComplete )
                {
                    _dbContext.RankCrosses.Add( new RankRequirementCross()
                    {
                        RankId = childModel.RankId,
                        RankRequirementId = childModel.Id,
                        CrossId = cross.Id
                    } );
                }
            }
        }

        public void SetHasImage( Rank rank, string fileExtension )
        {
            rank.ImageUploaded = DateTime.UtcNow;
            rank.ImageExtension = fileExtension;
            _dbContext.Entry( rank ).State = EntityState.Modified;
        }

        public void SetHasGuide( Rank rank, string fileExtension )
        {
            rank.GuideUploaded = DateTime.UtcNow;
            rank.GuideFileExtension = fileExtension;
            _dbContext.Entry( rank ).State = EntityState.Modified;
        }

        public async Task DeleteRankAsync( Guid rankId )
        {
            Rank rank = await _dbContext.Ranks.FindAsync( rankId );
            if ( rank == null )
            {
                return;
            }

            _dbContext.Ranks.Remove( rank );
            await _dbContext.SaveChangesAsync();
        }
        #endregion

        #region Status
        public IQueryable<RankStatus> RankStatuses()
        {
            return _dbContext.RankStatuses;
        }

        public async Task<RankStatus> GetRequirementStatusAsync( Guid rankId, Guid requirementId, Guid userId )
        {
            return await _dbContext.RankStatuses.FirstOrDefaultAsync( rs => rs.RankId == rankId
                                                             && rs.RankRequirementId == requirementId
                                                             && rs.UserId == userId && !rs.RecalledByWarriorTs.HasValue && !rs.ReturnedTs.HasValue );
        }

        public void AddStatusEntry( RankStatus status )
        {
            _dbContext.RankStatuses.Add( status );
        }

        public void Update( RankStatus existingChild )
        {
            _dbContext.Entry( existingChild ).State = EntityState.Modified;
        }

        public void DeleteRankStatus( RankStatus rankStatus )
        {
            _dbContext.RankStatuses.Remove( rankStatus );
        }

        public void AddApprovalEntry( RankApproval approvalEntry )
        {
            _dbContext.RankApprovals.Add( approvalEntry );
        }

        #endregion

        public async Task<Rank> GetHighestRankWithCompletionsAsync( Guid userIdForStatuses )
        {
//            var rank = await _dbContext.Ranks.FromSqlRaw( @"
//SELECT * FROM Ranks r
//WHERE EXISTS (SELECT 1 
//                FROM RankRequirements rr
//                INNER JOIN RankStatuses rs
//                    ON rs.rankid = rr.rankid
//                WHERE rr.rankid = r.id AND rs.userid = {userIdForStatuses} AND rs.RecalledByWarriorTs IS NULL AND rs.returnedts IS NULL
//)
//", userIdForStatuses ).SingleOrDefaultAsync();
//            return rank;

            //var rank2 = await _dbContext.Ranks.FromSqlRaw( @"
            //    SELECT * FROM Ranks r
            //    INNER JOIN RankRequirements rr
            //        ON rr.rankid = r.id
            //    LEFT OUTER JOIN RankStatuses rs
            //        ON rs.rankid = rr.rankid
            //    WHERE rs.userid = {0} AND rs.RecalledByWarriorTs IS NULL AND rs.returnedts IS NULL
            //", userIdForStatuses.ToString("D") ).SingleOrDefaultAsync();
            //return rank2;
            var query =
                from p in _dbContext.Ranks
                where _dbContext.RankStatuses.Any( s => s.RankId == p.Id && s.UserId == userIdForStatuses && !s.RecalledByWarriorTs.HasValue && !s.ReturnedTs.HasValue )
                join rr in _dbContext.RankRequirements on p.Id equals rr.RankId into reqs
                from rr in reqs.DefaultIfEmpty()
                join rs in _dbContext.RankStatuses on p.Id equals rs.RankId into stats
                from rs in stats.DefaultIfEmpty()
                select new { Rank = p, p.Requirements, Status = p.Statuses.Where( s => s.UserId == userIdForStatuses && !s.RecalledByWarriorTs.HasValue && !s.ReturnedTs.HasValue ) };
            var rank = await query.FirstOrDefaultAsync();
            if ( rank != null )
            {
                return rank.Rank;
            }
            else
            {
                return null;
            }
        }

        public async Task<Rank> GetRankByIndexAsync( int index, Guid userIdForStatuses )
        {
            return (await _dbContext.Ranks
                            .Where( r => r.Index == index )
                            .ToArrayAsync())
                            .FirstOrDefault();
        }

        private Rank MapToRank( Rank rank, IEnumerable<RankStatus> statuses, IEnumerable<RankRequirement> reqs )
        {
            rank.Requirements = reqs.Where( i => i != null ).ToArray();
            rank.Statuses = statuses.Where( i => i != null ).ToArray();
            return rank;
        }

        public void UpdateOrder( IEnumerable<GoalIndexEntry> request )
        {
            var allRanks = _dbContext.Ranks.ToArray();
            foreach ( var rank in request )
            {
                var rankToShift = allRanks.First( r => r.Id == rank.Id );
                _dbContext.Entry( rankToShift ).Entity.Index = rank.Index;
            }
        }

        public async Task<IEnumerable<MinimalCrossDetail>> CrossesForRankReq( Guid rankId, Guid requirementId )
        {
            return await _dbContext.RankCrosses.Include( r => r.Cross ).Where( r => r.RankId == rankId && r.RankRequirementId == requirementId )
                                                .Select( r => new MinimalCrossDetail()
                                                {
                                                    Id = r.CrossId,
                                                    Name = r.Cross.Name,
                                                    ImageUploaded = r.Cross.ImageUploaded,
                                                    ImageExtension = r.Cross.ImageExtension
                                                } ).ToArrayAsync();
        }
    }
}