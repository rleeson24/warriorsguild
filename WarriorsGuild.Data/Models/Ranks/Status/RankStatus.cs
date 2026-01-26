using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarriorsGuild.Data.Models.Ranks.Status
{
    public class RankStatus
    {
        [Key, DatabaseGenerated( DatabaseGeneratedOption.Identity )]
        public int Id { get; set; }

        [Required]
        public Guid RankId { get; set; }

        [Required]
        public Guid RankRequirementId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DateTime WarriorCompleted { get; set; }

        public DateTime? GuardianCompleted { get; set; }

        public DateTime? ReturnedTs { get; set; }

        public DateTime? RecalledByWarriorTs { get; set; }
    }
}