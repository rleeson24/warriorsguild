using System;

namespace WarriorsGuild.Helpers.Utilities.Files
{
    public interface IFilePathResolver
    {
        string GetCrossImagePath( Guid id, string fileExtension, DateTime? imageUploaded );
        string GetRankImagePath( Guid id, string fileExtension, DateTime? imageUploaded );
        string GetRingImagePath( Guid id, string fileExtension, DateTime? imageUploaded );
    }

    public class FilePathResolver : IFilePathResolver
    {
        public String GetCrossImagePath( Guid id, String fileExtension, DateTime? imageUploaded )
        {
            return imageUploaded.HasValue ? "/images/crosses/" + id + fileExtension : "/images/logo/Warriors-Guild-icon-sm.png";
        }
        public String GetRingImagePath( Guid id, String fileExtension, DateTime? imageUploaded )
        {
            return imageUploaded.HasValue ? "/images/rings/" + id + fileExtension : "/images/logo/Warriors-Guild-icon-sm.png";
        }
        public String GetRankImagePath( Guid id, String fileExtension, DateTime? imageUploaded )
        {
            return imageUploaded.HasValue ? "/images/ranks/" + id + fileExtension : "/images/logo/Warriors-Guild-icon-sm.png";
        }
    }
}
