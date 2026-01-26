using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WarriorsGuild.Data.Models.Ranks.Status;

namespace WarriorsGuild.Data.Models.Ranks
{
    public class Rank
    {
        public Rank()
        {
            Id = Guid.NewGuid();
            Requirements = new List<RankRequirement>();
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<RankRequirement> Requirements { get; set; }

        public string Description { get; set; }

        public DateTime? ImageUploaded { get; set; }
        public DateTime? GuideUploaded { get; set; }
        public string GuideFileExtension { get; set; }

        public ICollection<RankStatus> Statuses { get; set; }

        [Required]
        public int Index { get; set; }
        public string ImageExtension { get; set; }
    }
}