using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Data.Models.Crosses.Status
{
    public class CrossAnswer
    {
        public Guid CrossId { get; set; }

        public Guid CrossQuestionId { get; set; }

        [Required( AllowEmptyStrings = false )]
        public string Answer { get; set; } = string.Empty;

        [Required]
        public Guid UserId { get; set; }
    }
}