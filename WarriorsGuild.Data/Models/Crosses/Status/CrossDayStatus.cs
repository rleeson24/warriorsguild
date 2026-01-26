using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarriorsGuild.Data.Models.Crosses.Status
{
    public class CrossDayStatus
    {
        [Key, DatabaseGenerated( DatabaseGeneratedOption.Identity )]
        public int Id { get; set; }

        [Required]
        public Guid DayId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DateTime CompletedAt { get; set; }
    }
}
