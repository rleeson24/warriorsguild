using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarriorsGuild.Data.Models.Crosses
{
    public class PinnedCross
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid CrossId { get; set; }

        [NotMapped]
        public int PercentComplete { get; set; }

        [Required]
        public virtual Cross Cross { get; set; }
    }
}