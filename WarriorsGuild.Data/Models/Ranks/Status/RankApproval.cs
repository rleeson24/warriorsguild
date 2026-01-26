using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Data.Models.Ranks.Status
{
    public class RankApproval
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid RankId { get; set; }

        [Required]
        public int PercentComplete { get; set; }

        [Required]
        public DateTime CompletedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? RecalledByWarriorTs { get; set; }

        public DateTime? ReturnedTs { get; set; }

        public string ReturnedReason { get; set; }

        public virtual Rank Rank { get; set; }
        public Guid ReturnedBy { get; set; }
    }
}