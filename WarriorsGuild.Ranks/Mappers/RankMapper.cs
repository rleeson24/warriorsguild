using System;
using System.Collections.Generic;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.Data.Models.Ranks.Status;
using WarriorsGuild.Ranks.Models.Status;
using WarriorsGuild.Ranks.ViewModels;

namespace WarriorsGuild.Ranks.Mappers
{
    public interface IRankMapper
    {
        RankViewModel MapToRankViewModel( Rank rank, int percentComplete );
        Rank CreateRank( string description, string name, int v );
        RankStatus CreateRankStatus( Guid rankId, Guid rankRequirementId, DateTime warriorCompletedTs, DateTime? guardianCompletedTs, Guid userIdForStatuses );
        RankApproval CreateRankApproval( Guid rankId, Guid userIdForStatuses, int totalCompleted, DateTime completedTs );
        RankRequirementViewModel CreateRequirementViewModel( RankRequirement req, DateTime? warriorCompleted, DateTime? guardianCompleted, IEnumerable<MinimalRingDetail> rings, IEnumerable<MinimalCrossDetail> crosses, IEnumerable<MinimalGoalDetail> attachments );
        RankStatusUpdateModel CreateRankStatusUpdateModel( Guid rankId, Guid reqId, Guid[] crosses, Guid[] rings );
    }
    public class RankMapper : IRankMapper
    {
        public Rank CreateRank( string description, string name, int index )
        {
            return new Rank()
            {
                Id = Guid.NewGuid(),
                Description = description,
                Name = name,
                Index = index
            };
        }

        public RankApproval CreateRankApproval( Guid rankId, Guid userIdForStatuses, int totalCompleted, DateTime completedAt )
        {
            var approvalEntry = new RankApproval();
            approvalEntry.CompletedAt = completedAt;
            approvalEntry.RankId = rankId;
            approvalEntry.UserId = userIdForStatuses;
            approvalEntry.PercentComplete = totalCompleted;
            return approvalEntry;
        }

        public RankStatus CreateRankStatus( Guid rankId, Guid rankRequirementId, DateTime warriorCompletedTs, DateTime? guardianCompletedTs, Guid userIdForStatuses )
        {
            var statusToSave = new RankStatus();
            statusToSave.RankId = rankId;
            statusToSave.RankRequirementId = rankRequirementId;
            statusToSave.WarriorCompleted = warriorCompletedTs;
            statusToSave.GuardianCompleted = guardianCompletedTs;
            statusToSave.UserId = userIdForStatuses;
            return statusToSave;
        }

        public RankRequirementViewModel CreateRequirementViewModel( RankRequirement requirement, DateTime? warriorCompleted, DateTime? guardianCompleted, IEnumerable<MinimalRingDetail> rings, IEnumerable<MinimalCrossDetail> crosses, IEnumerable<MinimalGoalDetail> attachments )
        {
            return new RankRequirementViewModel()
            {
                Id = requirement.Id,
                Index = requirement.Index,
                RankId = requirement.RankId,
                Weight = requirement.Weight,
                ActionToComplete = requirement.ActionToComplete,
                GuardianCompleted = guardianCompleted,
                WarriorCompleted = warriorCompleted,
                RequireAttachment = requirement.RequireAttachment,
                RequireCross = requirement.RequireCross,
                RequireRing = requirement.RequireRing,
                RequiredRingCount = requirement.RequiredRingCount,
                RequiredRingType = requirement.RequiredRingType,
                RequiredCrossCount = requirement.RequiredCrossCount,
                SavedRings = rings,
                CrossesToComplete = crosses,
                Attachments = attachments,
                SeeHowLink = requirement.SeeHowLink,
                Optional = requirement.Optional,
                InitiatedByGuardian = requirement.InitiatedByGuardian,
                ShowAtPercent = requirement.ShowAtPercent
            };
        }

        public RankViewModel MapToRankViewModel( Rank rank, int percentComplete )
        {
            if ( rank == null ) return null;
            return new RankViewModel
            {
                Description = rank.Description,
                ImageUploaded = rank.ImageUploaded,
                ImageExtension = rank.ImageExtension,
                Id = rank.Id,
                Index = rank.Index,
                Name = rank.Name,
                PercentComplete = percentComplete
            };
        }

        public RankStatusUpdateModel CreateRankStatusUpdateModel( Guid rankId, Guid reqId, Guid[] crosses, Guid[] rings )
        {
            return new RankStatusUpdateModel()
            {
                RankId = rankId,
                RankRequirementId = reqId,
                Crosses = crosses,
                Rings = rings
            };
        }
    }
}