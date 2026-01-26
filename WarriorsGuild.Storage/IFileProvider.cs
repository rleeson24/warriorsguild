using System.Threading.Tasks;
using WarriorsGuild.Storage.Models;

namespace WarriorsGuild.Storage
{
    public interface IFileProvider
    {
        Task<FileDownloadResult> DownloadFile( WarriorsGuildFileType fileType, string id );
        Task<FileUploadResult> UploadFileAsync( WarriorsGuildFileType fileType, string srcFileName, string id, string mediaType );
    }
}