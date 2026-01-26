using System;

namespace WarriorsGuild.Crosses.Crosses.Status
{
    public class PendingApprovalDetail
    {
        public int ApprovalRecordId { get; set; }
        public Guid CrossId { get; internal set; }
        public Guid? DayId { get; internal set; }
        public string CrossName { get; internal set; }
        public DateTime? CrossImageUploaded { get; internal set; }
        public string ImageExtension { get; internal set; }
        public DateTime WarriorCompleted { get; internal set; }
        public DateTime? GuardianConfirmed { get; internal set; }
        public int PercentComplete { get; internal set; }
    }
}