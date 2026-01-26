using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Ranks.Models.Status
{
    public class RankStatusUpdateModel
    {
        [Required]
        public Guid RankId { get; set; }

        [Required]
        public Guid RankRequirementId { get; set; }

        public Guid[] Rings { get; set; }
        public Guid[] Crosses { get; set; }
    }
}