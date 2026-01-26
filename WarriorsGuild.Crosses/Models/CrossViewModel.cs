using System;

namespace WarriorsGuild.Crosses.Models
{
    public class CrossViewModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Description { get; set; }
        public string Name { get; set; }
        public DateTime? ImageUploaded { get; set; }

        public int Index { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string ImageExtension { get; set; }
        public DateTime? GuideUploaded { get; internal set; }
        public string GuideExtension { get; internal set; }
        public string ExplainText { get; set; }
    }
}