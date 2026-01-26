using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Data.Models.Crosses.Status
{
    public class CrossDayAnswer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public Guid CrossId { get; set; }
        [Required]
        public Guid DayId { get; set; }
        [Required]
        public Guid QuestionId { get; set; }
        [Required( AllowEmptyStrings = true )]
        public string Answer { get; set; } = string.Empty;
        [Required]
        public Guid UserId { get; set; }
    }
}
