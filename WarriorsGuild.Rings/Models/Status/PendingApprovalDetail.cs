using System;
using System.Collections.Generic;
using WarriorsGuild.Data.Models.Rings;

namespace WarriorsGuild.Rings.Models.Status
{
    public class PendingApprovalDetail
    {
        public Guid ApprovalRecordId { get; set; }
        public Guid RingId { get; set; }
        public string RingName { get; set; }
        public DateTime? RingImageUploaded { get; set; }
        public double PercentComplete { get; set; }
        public IEnumerable<RingRequirement> UnconfirmedRequirements { get; set; }
        public DateTime WarriorCompleted { get; set; }
        public DateTime? GuardianConfirmed { get; set; }
        public string ImageExtension { get; set; }
    }
}