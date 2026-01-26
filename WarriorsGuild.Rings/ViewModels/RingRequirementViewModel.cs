using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WarriorsGuild.Data.Models.Ranks;

namespace WarriorsGuild.Rings.ViewModels
{
    public class RingRequirementViewModel
    {
        [Required]
        public Guid RingId { get; set; }

        public Guid Id { get; set; }

        //[Required]
        //public RankStepType Type { get; set; }

        [Required]
        public string ActionToComplete { get; set; }

        [Required]
        public int Index { get; set; }

        [Required]
        public int Weight { get; set; }
        public DateTime? GuardianReviewedTs { get; internal set; }
        public string UserId { get; internal set; }
        public DateTime? WarriorCompletedTs { get; internal set; }

        public bool RequireAttachment { get; set; }
        public IEnumerable<MinimalGoalDetail> Attachments { get; internal set; }
        public string SeeHowLink { get; internal set; }
    }
}