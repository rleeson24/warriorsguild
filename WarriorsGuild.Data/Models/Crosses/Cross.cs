using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Data.Models.Crosses
{
    public class Cross
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Description { get; set; }
        [Required]
        public string Name { get; set; }

        public string ExplainText { get; set; }

        public ICollection<CrossQuestion> Questions { get; set; } = new CrossQuestion[ 0 ];

        public DateTime? ImageUploaded { get; set; }
        public DateTime? GuideUploaded { get; set; }
        public string GuideFileExtension { get; set; }

        [Required]
        public int Index { get; set; }
        public string ImageExtension { get; set; }
    }
}