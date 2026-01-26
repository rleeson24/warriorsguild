using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Ranks;

namespace WarriorsGuild.Ranks.ViewModels
{
    public class RankRequirementViewModel : RankRequirement
    {
        public DateTime? GuardianCompleted { get; internal set; }
        //public string UserId { get; internal set; }
        public DateTime? WarriorCompleted { get; internal set; }

        public IEnumerable<MinimalRingDetail> SavedRings { get; internal set; } = Enumerable.Empty<MinimalRingDetail>();
        public IEnumerable<MinimalCrossDetail> SavedCrosses { get; internal set; } = Enumerable.Empty<MinimalCrossDetail>();
        public IEnumerable<MinimalGoalDetail> Attachments { get; internal set; } = Enumerable.Empty<MinimalGoalDetail>();
        public IEnumerable<MinimalCrossDetail> CrossesToComplete { get; set; } = Enumerable.Empty<MinimalCrossDetail>();
    }
}