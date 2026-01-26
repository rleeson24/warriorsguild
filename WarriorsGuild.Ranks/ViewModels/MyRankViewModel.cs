namespace WarriorsGuild.Ranks.ViewModels
{
    public class MyRankViewModel
    {
        public RankViewModel CompletedRank { get; set; }
        public RankViewModel WorkingRank { get; set; }
        public int WorkingCompletionPercentage { get; set; }
        public int CompletedCompletionPercentage { get; set; }
    }
}