using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.Data.Models.Ranks.Status;
using WarriorsGuild.DataAccess;
using WarriorsGuild.DataAccess.Models;
using WarriorsGuild.Models;
using WarriorsGuild.Ranks.Models.Status;
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
        Task<Rank> GetRankWithGuideAsync( Guid rankId );
        void UpdateOrder( IEnumerable<GoalIndexEntry> request );
        Task<int> GetMaxRankIndexAsync();
        void AddApprovalEntry( RankApproval approvalEntry );
        Task<IEnumerable<MinimalCrossDetail>> CrossesForRankReq( Guid rankId, Guid requirementId );
        Task<RankApproval> GetLatestPendingOrApprovedRankApprovalAsync( Guid rankId, Guid userId );
        Task PostRankStatusAsync( RankStatus rankStatus );
        Task<int> GetTotalCompletedPercentAsync( Guid rankId, Guid userIdForStatuses );
        Task<bool> AllAssociatedRingsAndCrossesAreCompletedAndApprovedAsync( Guid rankId, IEnumerable<Guid> requirementIds, Guid approvalRecordUserId );
        Task<bool> AllPreviousRanksCompleteAsync( Guid rankId, Guid userId );
        Task<RecordCompletionResponse> DeleteRankStatusWithRelatedAsync( RankStatusUpdateModel rankForStatus, Guid userIdForStatuses );
        Task<ProofOfCompletionAttachment> GetProofOfCompletionAttachmentByIdAsync( Guid attachmentId );
        Task SaveProofOfCompletionOneUseFileKeyAsync( Guid attachmentId, Guid fileKey );
        Task<Guid> AddProofOfCompletionAttachmentAsync( ProofOfCompletionAttachment entity );
        Task<ProofOfCompletionAttachment> GetAndConsumeProofOfCompletionByFileKeyAsync( Guid oneUseFileKey );
        Task<IEnumerable<MinimalCrossDetail>> GetCrossesForRankStatusAsync( Guid rankId, Guid requirementId );
        Task<IEnumerable<MinimalGoalDetail>> GetAttachmentsForRankStatusAsync( Guid requirementId, Guid userIdForStatuses );
        Task<IEnumerable<MinimalRingDetail>> GetRingsForRankStatusAsync( Guid rankId, Guid requirementId, Guid userIdForStatuses );
        Task<RankApproval[]> GetPendingRankApprovalsWithRankAsync( Guid userId );
        Task<RankApproval[]> GetApprovalsForRankAsync( Guid rankId, Guid userIdForStatuses );
        Task<RankApproval> GetRankApprovalByIdAsync( Guid approvalRecordId );
        Task<RankApproval> GetRankApprovalByIdWithRankAsync( Guid approvalRecordId );
        Task<RankStatus[]> GetUnapprovedRankStatusesAsync( Guid rankId, Guid userId );
        Task<(RankRequirement[] Requirements, RankStatus[] Statuses)> GetPendingRequirementsWithStatusForApprovalAsync( Guid rankId, Guid userId );
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

        public async Task<Rank> GetRankWithGuideAsync( Guid rankId )
        {
            return await _dbContext.Ranks.SingleOrDefaultAsync( r => r.Id == rankId && r.GuideUploaded.HasValue );
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

        public async Task<RankApproval> GetLatestPendingOrApprovedRankApprovalAsync( Guid rankId, Guid userId )
        {
            return await _dbContext.RankApprovals.Where( ra => ra.UserId == userId && ra.RankId == rankId && !ra.RecalledByWarriorTs.HasValue && !ra.ReturnedTs.HasValue )
                .OrderByDescending( r => r.CompletedAt )
                .FirstOrDefaultAsync();
        }

        public async Task PostRankStatusAsync( RankStatus rankStatus )
        {
            _dbContext.RankStatuses.Add( rankStatus );
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetTotalCompletedPercentAsync( Guid rankId, Guid userIdForStatuses )
        {
            var requirements = await _dbContext.RankRequirements.Where( r => r.RankId == rankId ).Select( r => new { Id = r.Id.ToString(), r.Weight } ).ToArrayAsync();
            var statusEntries = await _dbContext.RankStatuses.Where( rs => rs.RankId == rankId && rs.UserId == userIdForStatuses && !rs.RecalledByWarriorTs.HasValue && !rs.ReturnedTs.HasValue ).ToArrayAsync();

            var fullQuery = requirements.Join( statusEntries.Select( r => r.RankRequirementId.ToString() ), r => r.Id.ToLower(), s => s, ( r, s ) => r.Weight );
            return fullQuery.Sum();
        }

        public async Task<bool> AllAssociatedRingsAndCrossesAreCompletedAndApprovedAsync( Guid rankId, IEnumerable<Guid> requirementIds, Guid approvalRecordUserId )
        {
            var reqs = await _dbContext.RankRequirements.Where( rr => rr.RankId == rankId && requirementIds.Contains( rr.Id ) ).ToArrayAsync();
            var selectedRingsAreApproved = true;
            var selectedCrossesAreApproved = true;
            foreach ( var req in reqs )
            {
                if ( req.RequireRing )
                {
                    int approvedAssociatedRingsCount = await _dbContext.RankStatusRings.CountAsync( r => r.RankId == req.RankId
                                                       && r.RankRequirementId == req.Id
                                                       && r.UserId == approvalRecordUserId
                                                       && _dbContext.RingApprovals.Any( ra => ra.RingId == r.RingId && ra.ApprovedAt.HasValue ) );
                    selectedRingsAreApproved = approvedAssociatedRingsCount == req.RequiredRingCount.Value;
                }
                if ( req.RequireCross )
                {
                    var approvedAssociatedCrossesCount = await _dbContext.RankStatusCrosses.CountAsync( r => r.RankId == req.RankId
                                                                && r.RankRequirementId == req.Id
                                                                && r.UserId == approvalRecordUserId
                                                                && _dbContext.CrossApprovals.Any( ra => ra.CrossId == r.CrossId && ra.ApprovedAt.HasValue ) );
                    selectedCrossesAreApproved = approvedAssociatedCrossesCount == req.RequiredCrossCount.Value;
                }
                if ( !selectedCrossesAreApproved || !selectedRingsAreApproved )
                {
                    break;
                }
            }
            return selectedCrossesAreApproved && selectedRingsAreApproved;
        }

        public async Task<bool> AllPreviousRanksCompleteAsync( Guid rankId, Guid userId )
        {
            var rankForStatus = _dbContext.Set<Rank>().First( r => r.Id == rankId ).Index;
            var previousRankIds = await _dbContext.Set<Rank>().Where( r => r.Index < rankForStatus ).Select( r => r.Id ).ToArrayAsync();
            var approvals = await _dbContext.Set<RankApproval>().Where( r => previousRankIds.Contains( r.RankId ) && r.UserId == userId && r.ApprovedAt.HasValue && r.PercentComplete == 100 ).Select( r => r.RankId ).Distinct().CountAsync();
            return approvals == previousRankIds.Count();
        }

        public async Task<RecordCompletionResponse> DeleteRankStatusWithRelatedAsync( RankStatusUpdateModel rankForStatus, Guid userIdForStatuses )
        {
            var response = new RecordCompletionResponse();
            var rankStatus = await _dbContext.RankStatuses.FirstOrDefaultAsync( rs => rs.RankId == rankForStatus.RankId && rs.RankRequirementId == rankForStatus.RankRequirementId && rs.UserId == userIdForStatuses && !rs.RecalledByWarriorTs.HasValue && !rs.ReturnedTs.HasValue );
            if ( rankStatus == null )
            {
                response.Error = "The given requirement is not marked as complete";
                return response;
            }
            if ( rankStatus.GuardianCompleted.HasValue )
            {
                response.Error = "This requirement completion cannot be undone because it has already been approved";
                return response;
            }
            rankStatus.RecalledByWarriorTs = DateTime.UtcNow;
            var pocToDelete = await _dbContext.ProofOfCompletionAttachments.Where( a => a.RequirementId == rankForStatus.RankRequirementId && a.UserId == userIdForStatuses ).ToArrayAsync();
            foreach ( var p in pocToDelete )
            {
                _dbContext.SetDeleted( p );
            }
            var crossesToDelete = await _dbContext.RankStatusCrosses.Where( c => c.RankId == rankForStatus.RankId
                                                                        && c.RankRequirementId == rankForStatus.RankRequirementId
                                                                        && c.UserId == userIdForStatuses ).ToArrayAsync();
            foreach ( var c in crossesToDelete )
            {
                _dbContext.RankStatusCrosses.Remove( c );
            }
            var ringsToDelete = await _dbContext.RankStatusRings.Where( c => c.RankId == rankForStatus.RankId
                                                                        && c.RankRequirementId == rankForStatus.RankRequirementId
                                                                        && c.UserId == userIdForStatuses ).ToArrayAsync();
            foreach ( var c in ringsToDelete )
            {
                _dbContext.RankStatusRings.Remove( c );
            }
            await _dbContext.SaveChangesAsync();
            response.Success = true;
            return response;
        }

        public async Task<ProofOfCompletionAttachment> GetProofOfCompletionAttachmentByIdAsync( Guid attachmentId )
        {
            return await _dbContext.ProofOfCompletionAttachments.SingleOrDefaultAsync( a => a.Id == attachmentId );
        }

        public async Task SaveProofOfCompletionOneUseFileKeyAsync( Guid attachmentId, Guid fileKey )
        {
            _dbContext.SingleUseFileDownloadKey.Add( new SingleUseFileDownloadKey() { AttachmentId = attachmentId, Key = fileKey } );
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Guid> AddProofOfCompletionAttachmentAsync( ProofOfCompletionAttachment entity )
        {
            _dbContext.ProofOfCompletionAttachments.Add( entity );
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<ProofOfCompletionAttachment> GetAndConsumeProofOfCompletionByFileKeyAsync( Guid oneUseFileKey )
        {
            var attachment = await _dbContext.ProofOfCompletionAttachments
                .Where( poca => _dbContext.SingleUseFileDownloadKey.Any( a => a.Key == oneUseFileKey && a.AttachmentId == poca.Id ) ).SingleOrDefaultAsync();
            if ( attachment == null )
                return null;
            var keyObject = await _dbContext.SingleUseFileDownloadKey.FindAsync( oneUseFileKey );
            _dbContext.SingleUseFileDownloadKey.Remove( keyObject );
            await _dbContext.SaveChangesAsync();
            return attachment;
        }

        public async Task<IEnumerable<MinimalCrossDetail>> GetCrossesForRankStatusAsync( Guid rankId, Guid requirementId )
        {
            return await CrossesForRankReq( rankId, requirementId );
        }

        public async Task<IEnumerable<MinimalGoalDetail>> GetAttachmentsForRankStatusAsync( Guid requirementId, Guid userIdForStatuses )
        {
            return await _dbContext.ProofOfCompletionAttachments.Where( r => r.RequirementId == requirementId && r.UserId == userIdForStatuses )
                .Select( r => new MinimalAttachmentDetail() { Id = r.Id } ).ToArrayAsync();
        }

        public async Task<IEnumerable<MinimalRingDetail>> GetRingsForRankStatusAsync( Guid rankId, Guid requirementId, Guid userIdForStatuses )
        {
            return await _dbContext.RankStatusRings.Include( r => r.Ring )
                .Where( r => r.RankId == rankId && r.RankRequirementId == requirementId && r.UserId == userIdForStatuses )
                .Select( r => new MinimalRingDetail()
                {
                    Id = r.RingId,
                    Name = r.Ring.Name,
                    ImageUploaded = r.Ring.ImageUploaded,
                    ImageExtension = r.Ring.ImageExtension
                } ).ToArrayAsync();
        }

        public async Task<RankApproval[]> GetPendingRankApprovalsWithRankAsync( Guid userId )
        {
            return await _dbContext.RankApprovals.Include( ra => ra.Rank )
                .Where( ra => ra.UserId == userId && !ra.RecalledByWarriorTs.HasValue && !ra.ReturnedTs.HasValue && !ra.ApprovedAt.HasValue )
                .ToArrayAsync();
        }

        public async Task<RankApproval[]> GetApprovalsForRankAsync( Guid rankId, Guid userIdForStatuses )
        {
            return await _dbContext.RankApprovals
                .Where( ra => ra.RankId == rankId && ra.UserId == userIdForStatuses )
                .OrderByDescending( r => r.CompletedAt )
                .ToArrayAsync();
        }

        public async Task<RankApproval> GetRankApprovalByIdAsync( Guid approvalRecordId )
        {
            return await _dbContext.RankApprovals.FirstOrDefaultAsync( ra => ra.Id == approvalRecordId );
        }

        public async Task<RankApproval> GetRankApprovalByIdWithRankAsync( Guid approvalRecordId )
        {
            return await _dbContext.RankApprovals.Include( s => s.Rank )
                .FirstOrDefaultAsync( c => c.Id == approvalRecordId && !c.RecalledByWarriorTs.HasValue && !c.ReturnedTs.HasValue && !c.ApprovedAt.HasValue );
        }

        public async Task<RankStatus[]> GetUnapprovedRankStatusesAsync( Guid rankId, Guid userId )
        {
            return await _dbContext.RankStatuses
                .Where( rs => rs.RankId == rankId && rs.UserId == userId && !rs.GuardianCompleted.HasValue && !rs.RecalledByWarriorTs.HasValue && !rs.ReturnedTs.HasValue )
                .ToArrayAsync();
        }

        public async Task<(RankRequirement[] Requirements, RankStatus[] Statuses)> GetPendingRequirementsWithStatusForApprovalAsync( Guid rankId, Guid userId )
        {
            var statusEntries = await _dbContext.RankStatuses
                .Where( rs => rs.RankId == rankId && rs.UserId == userId && !rs.GuardianCompleted.HasValue && !rs.RecalledByWarriorTs.HasValue && !rs.ReturnedTs.HasValue )
                .ToArrayAsync();
            var requirementIds = statusEntries.Select( s => s.RankRequirementId ).Distinct().ToArray();
            var requirements = await _dbContext.RankRequirements
                .Where( rr => rr.RankId == rankId && requirementIds.Contains( rr.Id ) )
                .ToArrayAsync();
            return (requirements, statusEntries);
        }
    }
}