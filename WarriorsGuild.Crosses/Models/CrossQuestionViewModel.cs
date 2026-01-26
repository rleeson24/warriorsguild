using System;

namespace WarriorsGuild.Crosses.Models
{
    public class CrossQuestionViewModel
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Index { get; set; }
        public string Answer { get; set; } = string.Empty;
    }
}