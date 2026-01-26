using System;
using System.Collections.Generic;
using WarriorsGuild.Data.Models.Rings;

namespace WarriorsGuild.Rings.Models.Status
{
    public class PendingApprovalDetail
    {
        public Guid ApprovalRecordId { get; set; }
        public Guid RingId { get; internal set; }
        public string RingName { get; internal set; }
        public DateTime? RingImageUploaded { get; internal set; }
        public double PercentComplete { get; internal set; }
        public IEnumerable<RingRequirement> UnconfirmedRequirements { get; set; }
        public DateTime WarriorCompleted { get; internal set; }
        public DateTime? GuardianConfirmed { get; internal set; }
        public string ImageExtension { get; internal set; }
    }
}