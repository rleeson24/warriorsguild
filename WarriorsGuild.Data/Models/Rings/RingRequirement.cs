using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Data.Models.Rings
{
    public class RingRequirement
    {
        public RingRequirement()
        {
            Id = Guid.NewGuid();
        }

        [Required]
        public Guid RingId { get; set; }

        [Key]
        public Guid Id { get; set; }

        //[Required]
        //public RingStepType Type { get; set; }

        [Required]
        public string ActionToComplete { get; set; }

        [Required]
        public int Index { get; set; }

        [Required]
        public int Weight { get; set; }

        public bool RequireAttachment { get; set; }

        [DataType( DataType.Url )]
        public string SeeHowLink { get; set; }

    }
}