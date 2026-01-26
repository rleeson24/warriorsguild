using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarriorsGuild.Data.Models.Rings
{
    public class PinnedRing
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid RingId { get; set; }

        [NotMapped]
        public int PercentComplete { get; set; }

        [Required]
        public virtual Ring Ring { get; set; }
    }
}