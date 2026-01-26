using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WarriorsGuild.Data.Models.Rings;

namespace WarriorsGuild.Rings.ViewModels
{
    public class RingViewModel
    {
        public IEnumerable<RingStatusViewModel> Statuses { get; set; }
        public string Description { get; set; }
        public DateTime? ImageUploaded { get; set; }
        public Guid Id { get; set; }
        [Required]
        public int Index { get; set; }
        [Required]
        public string Name { get; set; }
        public IEnumerable<RingRequirementViewModel> Requirements { get; set; }
        public RingType Type { get; set; }
        public string ImageExtension { get; set; }
    }
}