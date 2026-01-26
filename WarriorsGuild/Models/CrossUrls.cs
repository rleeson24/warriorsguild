namespace WarriorsGuild.Models
{
    public class CrossUrls
    {
        public string? CrossesUrl { get; set; }
        public string? CrossStatusUrl { get; set; }
        public string? RecordCompletion { get; set; }
        public string? ImageUploadBaseUrl { get; set; }
        public string? ImageBaseUrl { get; internal set; }
        public string? PublicCrossUrl { get; internal set; }
        public string? UploadGuideUrl { get; internal set; }
        public string? DownloadGuideUrl { get; internal set; }
    }
}