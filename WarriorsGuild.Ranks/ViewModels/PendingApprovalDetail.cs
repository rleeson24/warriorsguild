using System;
using System.Collections.Generic;

namespace WarriorsGuild.Ranks.ViewModels
{
    public class PendingApprovalDetail
    {
        public Guid ApprovalRecordId { get; set; }
        public Guid RankId { get; internal set; }
        public string RankName { get; internal set; }
        public Guid UserId { get; internal set; }
        public DateTime? RankImageUploaded { get; internal set; }
        public double PercentComplete { get; internal set; }
        public IEnumerable<RankRequirementViewModel> UnconfirmedRequirements { get; set; }
        public DateTime? WarriorCompletedTs { get; internal set; }
        public DateTime? GuardianApprovedTs { get; internal set; }
        public string ImageExtension { get; internal set; }
        public DateTime? ReturnedTs { get; internal set; }
        public string ReturnReason { get; internal set; }
    }
}