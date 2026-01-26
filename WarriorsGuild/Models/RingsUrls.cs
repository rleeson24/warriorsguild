namespace WarriorsGuild.Models
{
    public class RingsUrls
    {
        public string? RingsUrl { get; set; }
        public string? RecordCompletion { get; set; }
        public string? ImageUploadBaseUrl { get; set; }
        public string? UploadGuideUrl { get; set; }
        public string? DownloadGuideUrl { get; internal set; }
        public string? ImageBaseUrl { get; internal set; }
        public string? PublicRingUrl { get; internal set; }
        public string? RingStatusUrl { get; internal set; }
        public string? ProofOfCompletionUrl { get; internal set; }
    }
}