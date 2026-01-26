using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WarriorsGuild.Data.Models.Crosses;

namespace WarriorsGuild.DataAccess.Models
{
    public class RankStatusCompletedCross
    {
        [Key, DatabaseGenerated( DatabaseGeneratedOption.Identity )]
        public int Key { get; set; }

        [Required]
        public Guid RankId { get; set; }
        [Required]
        public Guid RankRequirementId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid CrossId { get; set; }
        public virtual Cross Cross { get; set; }
    }
}