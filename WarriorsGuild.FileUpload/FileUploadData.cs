namespace WarriorsGuild.Processes.Models
{
    public class FileUploadData
    {
        public string FileExtension { get; set; }
        public string LocalFileName { get; set; }
        public string MediaType { get; set; }
        public byte[] FileContent { get; set; }
    }
}