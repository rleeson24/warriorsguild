using System;

namespace WarriorsGuild.Ranks.ViewModels
{
    public class RankViewModel
    {
        public Guid Id { get; set; }
        public int Index { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public DateTime? ImageUploaded { get; set; }
        public int PercentComplete { get; set; }
        public string ImageExtension { get; set; }
    }
}