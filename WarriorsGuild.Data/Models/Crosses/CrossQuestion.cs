using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarriorsGuild.Data.Models.Crosses
{
    public class CrossQuestion
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Text { get; set; } = string.Empty;

        [Required]
        public int Index { get; set; }

        [NotMapped]
        public string Answer { get; set; } = string.Empty;

        [Required]
        public Guid CrossId { get; set; }
    }
}