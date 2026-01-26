using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WarriorsGuild.Data.Models.Crosses;

namespace WarriorsGuild.Data.Models
{
    public class CrossApproval
    {
        [Key, DatabaseGenerated( DatabaseGeneratedOption.Identity )]
        public int Id { get; set; }
        [Required]
        public Guid CrossId { get; set; }
        [Required]
        public Guid UserId { get; set; }
        public Guid? DayId { get; set; }

        [Required]
        public int PercentComplete { get; set; }

        [Required]
        public DateTime CompletedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? ReturnedTs { get; set; }

        public DateTime? RecalledByWarriorTs { get; set; }

        public string ReturnedReason { get; set; }

        public virtual Cross Cross { get; set; }
        public virtual CrossDay Day { get; set; }
    }
}