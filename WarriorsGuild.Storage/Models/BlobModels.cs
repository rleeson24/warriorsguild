using System;
using System.IO;

namespace WarriorsGuild.Storage.Models
{
    public class FileUploadResult
    {
        public string Path { get; set; }
        public long FileSizeInBytes { get; set; }
        public long FileSizeInKb { get { return (long)Math.Ceiling( (double)FileSizeInBytes / 1024 ); } }
    }

    public class BlobDownloadModel
    {
        public MemoryStream BlobStream { get; set; }
        public string BlobFileName { get; set; }
        public string BlobContentType { get; set; }
        public long BlobLength { get; set; }
    }
}