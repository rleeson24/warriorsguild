using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Rings.Models.Status
{
    public class RingStatusUpdateModel
    {
        [Required]
        public Guid RingId { get; set; }

        [Required]
        public Guid RingRequirementId { get; set; }
    }
}