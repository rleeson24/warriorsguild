namespace WarriorsGuild.Storage.Models
{
    public class FileDownloadResult
    {
        public string BlobFileName { get; set; }
        public string ContentType { get; set; }
        public long BlobLength { get; set; }
        public string FilePathToServe { get; set; }
    }
}