using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Crosses.Models
{
    public class CrossDayViewModel
    {
        public Guid? Id { get; set; }
        [Required]
        public int Weight { get; set; }
        [Required]
        public string Passage { get; set; }
        public bool IsCheckpoint { get; set; }
        [Required]
        public int Index { get; set; }
        public DateTime? CompletedAt { get; internal set; }
        public IEnumerable<CrossQuestionViewModel> Questions { get; internal set; }
    }
}
