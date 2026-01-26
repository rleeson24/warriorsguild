using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Crosses.Models
{
    public class CrossAnswerViewModel
    {
        [Required]
        public Guid CrossQuestionId { get; set; }

        [Required( AllowEmptyStrings = true )]
        public string Answer { get; set; } = string.Empty;
    }
}