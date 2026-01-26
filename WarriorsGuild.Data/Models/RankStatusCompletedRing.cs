using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WarriorsGuild.Data.Models.Rings;

namespace WarriorsGuild.DataAccess.Models
{
    public class RankStatusCompletedRing
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
        public Guid RingId { get; set; }
        public virtual Ring Ring { get; set; }
    }
}