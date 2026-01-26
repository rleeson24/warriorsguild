using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Rings.ViewModels
{
    public class RingStatusViewModel
    {
        public int Id { get; set; }

        public Guid RingId { get; set; }

        public Guid? RingRequirementId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime? WarriorCompleted { get; set; }

        public DateTime? GuardianCompleted { get; set; }

        //public WGAttachment Attachment { get; set; }
    }
}