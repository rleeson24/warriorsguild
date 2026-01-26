using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Data.Models.Crosses
{
    public class CrossDay
    {
        [Key]
        public Guid DayId { get; set; } = Guid.NewGuid();
        [Required]
        public Guid CrossId { get; set; }
        [Required]
        public int Weight { get; set; }
        [Required]
        public string Passage { get; set; }
        public bool IsCheckpoint { get; set; }
        [Required]
        public int Index { get; set; }
    }
}
