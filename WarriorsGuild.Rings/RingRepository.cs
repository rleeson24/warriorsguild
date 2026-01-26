using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.Data.Models.Rings;
using WarriorsGuild.Data.Models.Rings.Status;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Models;
using WarriorsGuild.Rings.Models.Status;

namespace WarriorsGuild.Rings
{
    public interface IRingRepository
    {
        Task<Ring> AddAsync( Ring ring );
        Task AddStatusEntryAsync( RingStatusUpdateModel newValues, Guid userIdForStatuses );
        Task<Ring> DeleteRingAsync( Guid id );
        Task DeleteRingStatusAsync( RingStatus ringStatus );
        Task<Ring> GetAsync( Guid id, bool includeRequirements = false );
        Task<IEnumerable<RingRequirement>> GetRequirementsAsync( Guid id );
        Task<IEnumerable<RingStatus>> GetStatusesAsync( Guid id, Guid userIdForStatuses );
        Task<IEnumerable<Ring>> GetListAsync();
        Task<Ring> GetPublicAsync();
        IQueryable<RingStatus> RingStatuses();
        Task<RingStatus> GetRingStatusAsync( int id );
        Task MarkGuardianReviewedAsync( RingStatus existingChild );
        Task<PinnedRing> GetPinnedRing( Guid userIdForStatuses, Guid ringId );
        Task<IEnumerable<PinnedRing>> GetActivePinnedRings( Guid userIdForStatuses );
        Task<IEnumerable<PinnedRing>> GetPinnedRings( Guid userIdForStatuses );
        Task PinAsync( PinnedRing ring );
        Task UnpinAsync( PinnedRing ring );
        Task<RingStatus> PostRingStatusAsync( RingStatus ringStatus );
        Task UpdateAsync( Guid id, Ring existingRing );
        Task UpdateRequirementsAsync( Guid id, IEnumerable<RingRequirement> requirements, IEnumerable<RingRequirement> existingRequirements );
        Task SetHasImageAsync( Guid id, string fileExtension );
        Task SetHasGuideAsync( Guid id, string fileExtension );
        Task AddApprovalEntry( RingApproval approvalEntry );
        Task<IEnumerable<Ring>> GetCompletedAsync( Guid userIdForStatuses );
        Task<int> GetMaxIndexAsync();
        Task<RingStatus> GetRequirementStatusAsync( Guid ringId, Guid ringRequirementId, Guid userIdForStatuses );
        Task<IEnumerable<Ring>> UpdateRingOrderAsync( IEnumerable<GoalIndexEntry> request );
        Task<IEnumerable<MinimalGoalDetail>> GetAttachmentsForRingStatus( Guid requirementId, Guid userIdForStatuses );
    }
    public class RingRepository : IRingRepository
    {
        private IGuildDbContext _dbContext { get; }

        public RingRepository( IGuildDbContext db )
        {
            _dbContext = db;
        }

        #region Rings
        public IQueryable Rings()
        {
            return _dbContext.Rings;
        }

        public async Task<IEnumerable<Ring>> GetListAsync()
        {
            //OrderBy must come after DefaultIfEmpty() or Linq does not respect it
            var rings = await _dbContext.Rings.OrderBy( r => r.Index ).ToArrayAsync();

            return rings;
        }

        public async Task<int> GetMaxIndexAsync()
        {
            var ringIndexes = from ring in _dbContext.Rings
                              orderby ring.Index
                              select ring.Index;

            if ( ringIndexes.Any() )
            {
                return await ringIndexes.MaxAsync();
            }
            else
            {
                return 0;
            }
        }

        public async Task<Ring> GetPublicAsync()
        {
            return await _dbContext.Rings.OrderBy( r => r.Index ).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RingRequirement>> GetRequirementsAsync( Guid id )
        {
            var result = await _dbContext.RingRequirements.Where( rr => rr.RingId == id ).OrderBy( rr => rr.Index ).ToArrayAsync();
            return result;
        }

        public async Task<IEnumerable<RingStatus>> GetStatusesAsync( Guid id, Guid userIdForStatuses )
        {
            var result = await _dbContext.RingStatuses.Where( rs => rs.RingId == id && rs.UserId == userIdForStatuses && !rs.RecalledByWarriorTs.HasValue && !rs.ReturnedTs.HasValue ).ToArrayAsync();
            return result;
        }

        public async Task<IEnumerable<Ring>> GetCompletedAsync( Guid userIdForStatuses )
        {
            return await _dbContext.RingApprovals.Include( ra => ra.Ring ).Where( ra => ra.ApprovedAt.HasValue && ra.UserId == userIdForStatuses ).Select( ra => ra.Ring ).ToArrayAsync();
        }

        public async Task<Ring> GetAsync( Guid id, bool includeRequirements = false )
        {
            var result = (IQueryable<Ring>)_dbContext.Rings;
            if ( includeRequirements ) result = result.Include( r => r.Requirements );
            return await result.FirstOrDefaultAsync( r => r.Id == id );
        }

        public async Task<IEnumerable<PinnedRing>> GetActivePinnedRings( Guid userIdForStatuses )
        {
            //var ringsQuery = Database.PinnedRings.Include( p => p.Ring ).Where( p => p.UserId == userIdForStatuses
            //                                        && !Database.RingApprovals.Any( a => a.RingId == p.Ring.Id && a.UserId == userIdForStatuses && a.ApprovedAt.HasValue ) );
            var pinnedRings = await (from p in _dbContext.PinnedRings.Include( p => p.Ring )
                                     join ringReq in _dbContext.RingRequirements on p.RingId equals ringReq.RingId into reqs
                                     from ringReq in reqs.DefaultIfEmpty()
                                     join status in _dbContext.RingStatuses.Where( a => a.UserId == userIdForStatuses && !a.RecalledByWarriorTs.HasValue && !a.ReturnedTs.HasValue ) on ringReq.Id equals status.RingRequirementId into reqStatuses
                                     from status in reqStatuses.DefaultIfEmpty()
                                     join approval in _dbContext.RingApprovals.Where( a => a.UserId == userIdForStatuses && !a.RecalledByWarriorTs.HasValue && !a.ReturnedTs.HasValue ) on p.RingId equals approval.RingId into approvals
                                     from approval in approvals.DefaultIfEmpty()
                                     where p.UserId == userIdForStatuses
                                     select new { p.Id, Ring = p, Req = ringReq, Status = status, Approval = approval }
                        ).ToArrayAsync();
            var rings = pinnedRings.Select( p => p.Ring ).Distinct().GroupJoin( pinnedRings.Select( p => p.Req ), o => o.RingId, i => i.RingId, ( c, reqs ) => new { PinnedRing = c, Reqs = reqs } );

            var result = new List<PinnedRing>();
            foreach ( var c in rings.ToArray() )
            {
                var pinnedRingList = pinnedRings.Where( pc => pc.Ring.RingId == c.PinnedRing.Ring.Id );
                var completedReqs = pinnedRingList.Where( pc => pc.Status != null ).Select( r => r.Req ).ToArray();
                var approval = pinnedRingList.FirstOrDefault( pr => pr.Approval != null );
                c.PinnedRing.PercentComplete = completedReqs.Sum( r => r.Weight ) * 100 / c.Reqs.Sum( r => r.Weight );
                if ( c.PinnedRing.PercentComplete < 100 || approval == null )
                {
                    result.Add( c.PinnedRing );
                }
            }
            return result;
        }

        public async Task<IEnumerable<PinnedRing>> GetPinnedRings( Guid userIdForStatuses )
        {
            var pinnedRings = await (from p in _dbContext.PinnedRings.Include( p => p.Ring )
                                     where p.UserId == userIdForStatuses
                                     select p
                        ).ToArrayAsync();
            return pinnedRings;
        }

        public async Task<PinnedRing> GetPinnedRing( Guid userIdForStatuses, Guid ringId )
        {
            return await _dbContext.PinnedRings.FirstOrDefaultAsync( p => p.UserId == userIdForStatuses && p.RingId == ringId );
        }


        public async Task UpdateAsync( Guid id, Ring existingRing )
        {
            // Update parent
            //Database.Entry( existingRing ).CurrentValues.SetValues( existingRing );
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateRequirementsAsync( Guid id, IEnumerable<RingRequirement> requirements, IEnumerable<RingRequirement> existingRequirements )
        {
            // Delete children
            foreach ( var existingChild in existingRequirements.ToList() )
            {
                if ( !requirements.Any( c => c.Id == existingChild.Id ) )
                {
                    _dbContext.Entry( existingChild ).State = EntityState.Deleted;
                }
            }

            // Update and Insert children
            foreach ( var childModel in requirements )
            {
                var existingChild = existingRequirements
                    .Where( c => c.Id == childModel.Id )
                    .SingleOrDefault();

                if ( existingChild != null )
                    // Update child
                    _dbContext.Entry( existingChild ).CurrentValues.SetValues( childModel );
                else
                {
                    _dbContext.RingRequirements.Add( childModel );
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<Ring> AddAsync( Ring ring )
        {
            _dbContext.Rings.Add( ring );

            await _dbContext.SaveChangesAsync();
            return ring;
        }

        public async Task PinAsync( PinnedRing ring )
        {
            if ( !PinnedRingExists( ring.UserId, ring.Ring.Id ) )
            {
                _dbContext.PinnedRings.Add( ring );
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UnpinAsync( PinnedRing ring )
        {
            _dbContext.PinnedRings.Remove( ring );
            await _dbContext.SaveChangesAsync();
        }

        public async Task SetHasImageAsync( Guid ringId, string fileExtension )
        {
            var ring = await _dbContext.Rings.FindAsync( ringId );
            ring.ImageUploaded = DateTime.UtcNow;
            ring.ImageExtension = fileExtension;
            _dbContext.Entry( ring ).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task SetHasGuideAsync( Guid ringId, string fileExtension )
        {
            var ring = await _dbContext.Rings.FindAsync( ringId );
            ring.GuideUploaded = DateTime.UtcNow;
            ring.GuideFileExtension = fileExtension;
            _dbContext.Entry( ring ).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Ring> DeleteRingAsync( Guid id )
        {
            Ring ring = await _dbContext.Rings.FindAsync( id );
            if ( ring == null )
            {
                return null;
            }

            _dbContext.Rings.Remove( ring );
            await _dbContext.SaveChangesAsync();

            return ring;
        }

        private bool PinnedRingExists( Guid userId, Guid ringId )
        {
            return _dbContext.PinnedRings.Count( e => e.UserId == userId && e.RingId == ringId ) > 0;
        }
        #endregion

        #region Status
        public IQueryable<RingStatus> RingStatuses()
        {
            return _dbContext.RingStatuses;
        }

        public async Task<RingStatus> GetRingStatusAsync( int id )
        {
            return await _dbContext.RingStatuses.FindAsync( id );
        }

        //public async Task<RingStatus> GetRingStatusAsync( Guid ringId, Guid requirementId, Guid userId )
        //{
        //    return await Database.RingStatuses.FirstOrDefaultAsync( rs => rs.RingId == ringId
        //                                                     && rs.RingRequirementId == requirementId
        //                                                     && rs.UserId == userId && !rs.RecalledByWarriorTs.HasValue && !rs.ReturnedTs.HasValue );
        //}

        public async Task AddStatusEntryAsync( RingStatusUpdateModel newValues, Guid userIdForStatuses )
        {
            var status = new RingStatus();
            status.RingId = newValues.RingId;
            status.RingRequirementId = newValues.RingRequirementId;
            status.WarriorCompleted = DateTime.UtcNow;
            status.UserId = userIdForStatuses;
            _dbContext.RingStatuses.Add( status );
            await _dbContext.SaveChangesAsync();
        }

        public async Task MarkGuardianReviewedAsync( RingStatus existingChild )
        {
            existingChild.GuardianCompleted = DateTime.UtcNow;
            _dbContext.Entry( existingChild ).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<RingStatus> PostRingStatusAsync( RingStatus ringStatus )
        {
            _dbContext.RingStatuses.Add( ringStatus );
            await _dbContext.SaveChangesAsync();
            return ringStatus;
        }

        public async Task DeleteRingStatusAsync( RingStatus ringStatus )
        {
            _dbContext.RingStatuses.Remove( ringStatus );
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddApprovalEntry( RingApproval approvalEntry )
        {
            _dbContext.RingApprovals.Add( approvalEntry );
            await _dbContext.SaveChangesAsync();
        }

        private bool RingStatusExists( int id )
        {
            return _dbContext.RingStatuses.Count( e => e.Id == id ) > 0;
        }

        #endregion

        public async Task<RingStatus> GetRequirementStatusAsync( Guid ringId, Guid ringRequirementId, Guid userIdForStatuses )
        {
            return await _dbContext.RingStatuses.FirstOrDefaultAsync( rs => rs.RingId == ringId
                                                             && rs.RingRequirementId == ringRequirementId
                                                             && rs.UserId == userIdForStatuses && !rs.RecalledByWarriorTs.HasValue && !rs.ReturnedTs.HasValue );
        }

        public async Task<IEnumerable<Ring>> UpdateRingOrderAsync( IEnumerable<GoalIndexEntry> request )
        {
            var allRings = _dbContext.Rings.ToArray();
            foreach ( var ring in request )
            {
                var ringToShift = allRings.First( r => r.Id == ring.Id );
                _dbContext.Entry( ringToShift ).Entity.Index = ring.Index;
            }
            await _dbContext.SaveChangesAsync();
            return _dbContext.Rings;
        }

        public async Task<IEnumerable<MinimalGoalDetail>> GetAttachmentsForRingStatus( Guid requirementId, Guid userIdForStatuses )
        {
            return await _dbContext.ProofOfCompletionAttachments.Where( r => r.RequirementId == requirementId && r.UserId == userIdForStatuses )
                                                .Select( r => new MinimalAttachmentDetail() { Id = r.Id } ).ToArrayAsync();
        }
    }
}