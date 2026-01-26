using System.Collections.Generic;
using WarriorsGuild.Data.Models.Ranks;

namespace WarriorsGuild.Ranks.Models
{
    internal class MoveRankResponse
    {
        public IEnumerable<Rank> Ranks { get; set; }
        public bool Moved { get; set; }
    }
}