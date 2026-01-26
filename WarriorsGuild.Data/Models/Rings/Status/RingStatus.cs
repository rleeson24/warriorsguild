using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarriorsGuild.Data.Models.Rings.Status
{
    public class RingStatus
    {
        [Key, DatabaseGenerated( DatabaseGeneratedOption.Identity )]
        public int Id { get; set; }

        [Required]
        public Guid RingId { get; set; }

        [Required]
        public Guid RingRequirementId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DateTime WarriorCompleted { get; set; }

        public DateTime? GuardianCompleted { get; set; }

        public DateTime? ReturnedTs { get; set; }

        public DateTime? RecalledByWarriorTs { get; set; }
    }
}