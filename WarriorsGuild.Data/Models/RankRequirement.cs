using System;
using System.ComponentModel.DataAnnotations;
using WarriorsGuild.Data.Models.Rings;

namespace WarriorsGuild.Data.Models
{
    public class RankRequirement
    {
        public RankRequirement()
        {
            Id = Guid.NewGuid();
        }

        [Required]
        public Guid RankId { get; set; }

        [Key]
        public Guid Id { get; set; }

        //[Required]
        //public RankStepType Type { get; set; }

        [Required]
        public string ActionToComplete { get; set; }

        [Required]
        public int Index { get; set; }

        [Required]
        public int Weight { get; set; }

        public bool RequireAttachment { get; set; }
        public bool RequireRing { get; set; }
        public bool RequireCross { get; set; }

        public RingType? RequiredRingType { get; set; }
        public int? RequiredRingCount { get; set; }
        public int? RequiredCrossCount { get; set; }

        [DataType( DataType.Url )]
        public string SeeHowLink { get; set; }

        public bool Optional { get; set; }
        public bool InitiatedByGuardian { get; set; }
        public double ShowAtPercent { get; set; }
    }
}