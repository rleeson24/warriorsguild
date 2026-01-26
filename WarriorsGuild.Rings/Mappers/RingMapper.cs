using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.Data.Models.Rings;
using WarriorsGuild.Data.Models.Rings.Status;
using WarriorsGuild.Rings.Models.Status;
using WarriorsGuild.Rings.ViewModels;

namespace WarriorsGuild.Rings.Mappers
{
    public interface IRingMapper
    {
        RingViewModel MapToRingViewModel( Ring ring );
        RingStatus CreateRingStatus( Guid ringId, Guid ringRequirementId, DateTime warriorCompletedTs, DateTime? guardianConfirmed, Guid userIdForStatuses );
        RingApproval CreateRingApproval( Guid ringId, string userIdForStatuses, int totalCompleted, DateTime dateTime );
        RingStatusUpdateModel CreateRingStatusUpdateModel( Guid ringId, Guid reqId );
        RingRequirementViewModel CreateRequirementViewModel( RingRequirement requirement, RingStatus status, IEnumerable<MinimalGoalDetail> attachments );
        //RingStatusUpdateModel CreateRingStatusUpdateModel( Guid ringId, Guid reqId, Guid[] crosses, Guid[] rings );
    }

    public class RingMapper : IRingMapper
    {
        public RingApproval CreateRingApproval( Guid ringId, string userIdForStatuses, int totalCompleted, DateTime dateTime )
        {
            return new RingApproval()
            {
                RingId = ringId,
                UserId = new Guid( userIdForStatuses ),
                //PercentComplete = totalCompleted,
                CompletedAt = dateTime
            };
        }

        public RingStatus CreateRingStatus( Guid ringId, Guid ringRequirementId, DateTime warriorCompletedTs, DateTime? guardianConfirmed, Guid userIdForStatuses )
        {
            return new RingStatus()
            {
                RingId = ringId,
                RingRequirementId = ringRequirementId,
                WarriorCompleted = warriorCompletedTs,
                GuardianCompleted = guardianConfirmed,
                UserId = userIdForStatuses
            };
        }

        public RingViewModel MapToRingViewModel( Ring ring )
        {
            if ( ring == null ) return null;
            return new RingViewModel
            {
                Statuses = ring.Statuses.Select( i => new RingStatusViewModel
                {
                    GuardianCompleted = i.GuardianCompleted,
                    Id = i.Id,
                    RingId = i.RingId,
                    RingRequirementId = i.RingRequirementId,
                    UserId = i.UserId.ToString(),
                    WarriorCompleted = i.WarriorCompleted
                } ),
                Description = ring.Description,
                ImageUploaded = ring.ImageUploaded,
                Id = ring.Id,
                Index = ring.Index,
                Name = ring.Name,
                Type = ring.Type,
                Requirements = ring.Requirements.Select( rr => new RingRequirementViewModel
                {
                    ActionToComplete = rr.ActionToComplete,
                    Id = rr.Id,
                    Index = rr.Index,
                    RingId = rr.RingId,
                    Weight = rr.Weight,
                    RequireAttachment = rr.RequireAttachment
                } )
            };
        }

        public RingStatusUpdateModel CreateRingStatusUpdateModel( Guid ringId, Guid reqId )
        {
            return new RingStatusUpdateModel()
            {
                RingId = ringId,
                RingRequirementId = reqId
            };
        }

        public RingRequirementViewModel CreateRequirementViewModel( RingRequirement requirement, RingStatus status, IEnumerable<MinimalGoalDetail> attachments )
        {
            return new RingRequirementViewModel()
            {
                Id = requirement.Id,
                Index = requirement.Index,
                RingId = requirement.RingId,
                Weight = requirement.Weight,
                ActionToComplete = requirement.ActionToComplete,
                RequireAttachment = requirement.RequireAttachment,
                GuardianReviewedTs = status?.GuardianCompleted,
                UserId = status?.UserId.ToString(),
                WarriorCompletedTs = status?.WarriorCompleted,
                Attachments = attachments,
                SeeHowLink = requirement.SeeHowLink
            };
        }
    }
}