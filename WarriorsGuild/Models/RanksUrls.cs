namespace WarriorsGuild.Models
{
    public class RanksUrls
    {
        public string? RanksUrl { get; set; }
        public string? RecordCompletion { get; set; }
        public string? ImageUploadBaseUrl { get; set; }
        public string? UploadGuideUrl { get; set; }
        public string? DownloadGuideUrl { get; internal set; }
        public string? ImageBaseUrl { get; internal set; }
        public string? PublicRankUrl { get; internal set; }
        public string? RankStatusUrl { get; internal set; }
        public string? RingStatusUrl { get; internal set; }
        public string? CrossStatusUrl { get; internal set; }
        public string? ProofOfCompletionUrl { get; internal set; }
        public string? CrossUrl { get; internal set; }
    }
}