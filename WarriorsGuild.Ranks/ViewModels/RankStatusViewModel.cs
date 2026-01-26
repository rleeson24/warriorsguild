using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Ranks.ViewModels
{
    public class RankStatusViewModel
    {
        public int Id { get; set; }

        public Guid RankId { get; set; }

        public Guid? RankRequirementId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime? WarriorCompleted { get; set; }

        public DateTime? GuardianCompleted { get; set; }

        public Guid[] RingIds { get; set; }
        public Guid[] CrossIds { get; set; }
        //public WGAttachment Attachment { get; set; }
    }
}