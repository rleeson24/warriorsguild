using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Data.Models.Rings.Status
{
    public class RingApproval
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid RingId { get; set; }

        [Required]
        public DateTime CompletedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? RecalledByWarriorTs { get; set; }

        public DateTime? ReturnedTs { get; set; }

        public string ReturnedReason { get; set; }

        public virtual Ring Ring { get; set; }
    }
}