using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WarriorsGuild.Data.Models.Rings.Status;

namespace WarriorsGuild.Data.Models.Rings
{
    public class Ring
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public RingType Type { get; set; }

        public List<RingRequirement> Requirements { get; set; } = new List<RingRequirement>();

        public DateTime? ImageUploaded { get; set; }
        public DateTime? GuideUploaded { get; set; }
        public string GuideFileExtension { get; set; }

        [NotMapped]
        public bool IsPinned { get; set; }

        public ICollection<RingStatus> Statuses { get; set; } = new List<RingStatus>();

        [Required]
        public int Index { get; set; }
        public string ImageExtension { get; set; }
    }

    public enum RingType
    {
        Silver,
        Gold,
        Platinum
    }
}