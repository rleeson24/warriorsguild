using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using WarriorsGuild.Helpers.Utilities.Files;
using WarriorsGuild.Storage.Models;

namespace WarriorsGuild.Storage
{
    public interface IFileSystemProvider
    {
        Task<FileUploadResult> SaveFileToContentImages( string contentPath, WarriorsGuildFileType imgType, MultipartFileData fileData, string fileName );
    }

    public class FileSystemProvider : IFileSystemProvider
    {
        private readonly ILogger<FileSystemProvider> _logger;

        public FileSystemProvider( ILogger<FileSystemProvider> logger )
        {
            _logger = logger;
        }

        public async Task<FileUploadResult> SaveFileToContentImages( string contentPath, WarriorsGuildFileType imgType, MultipartFileData fileData, string fileName )
        {
            // Don't trust the file name sent by the client. To display
            // the file name, HTML-encode the value.
            //trustedFileNameForDisplay = WebUtility.HtmlEncode( fileData.ContentDisposition.FileName.Value );
            var relativePath = GetImageFolderByType( imgType ) + fileName;

            //if ( !Directory.Exists( relativePath ) )
            //{
            //    Directory.CreateDirectory( relativePath );
            //}
            var path = Path.Combine( contentPath, relativePath );

            using ( var targetStream = File.Create( path ) )
            {
                await targetStream.WriteAsync( fileData.Content );
            }

            _logger.LogInformation(
                "Uploaded file '{TrustedFileNameForDisplay}' saved to " +
                "'{TargetFilePath}'",
                WebUtility.HtmlEncode( fileData.ContentDisposition.FileName.Value ), relativePath );
            return new FileUploadResult()
            { Path = relativePath };
        }

        //public async Task<FileUploadResult> UploadImageAsync( WarriorsGuildFileType imgType, string fileName, IFormFile file )
        //{
        //    var imageFolder = ;
        //    var contentPath = this.Environment.ContentRootPath;

        //    var imagesFolderPath = Path.Combine( contentPath, @"\Images\" + imageFolder );
        //    if ( !Directory.Exists( imagesFolderPath ) )
        //    {
        //        Directory.CreateDirectory( imagesFolderPath );
        //    }

        //    var path = Path.Combine( imagesFolderPath, Path.GetExtension( file.FileName ) );
        //    using ( var stream = new FileStream( path, FileMode.Create ) )
        //    {
        //        await file.CopyToAsync( stream );
        //    }

        //    //var filePath = BuildFilePath( imgType, id );
        //    //var fileData = File.ReadAllBytes( srcFileName );
        //    //File.WriteAllBytes( filePath, fileData );
        //    return await Task.FromResult( new FileUploadResult()
        //    {
        //        FileName = path,
        //        FileSizeInBytes = file.Length
        //    } );
        //}

        private string GetImageFolderByType( WarriorsGuildFileType uploadType )
        {
            switch ( uploadType )
            {
                case WarriorsGuildFileType.RankImage:
                    return @"images\ranks\";
                case WarriorsGuildFileType.RingImage:
                    return @"images\rings\";
                case WarriorsGuildFileType.CrossImage:
                    return @"images\crosses\";
                default:
                    throw new Exception( $"{uploadType} not mappable" );
            }
        }

        //private string BuildFilePath( WarriorsGuildFileType imgType, string fileName )
        //{
        //    switch ( imgType )
        //    {
        //        case WarriorsGuildFileType.RankImage:
        //            return HttpContextManager.Current.Server.MapPath( "~/images/ranks/" + fileName.ToString() );
        //        case WarriorsGuildFileType.RingImage:
        //            return HttpContextManager.Current.Server.MapPath( "~/images/rings/" + fileName.ToString() );
        //        default:
        //            throw new Exception( $"{imgType} not mappable" );
        //    }
        //}

        //public async Task<FileDownloadResult> DownloadFile( WarriorsGuildFileType imgType, String id )
        //{
        //    return await Task.FromResult( new FileDownloadResult() { FilePathToServe = BuildFilePath( imgType, id ) } );
        //}


        //or ValidateFileAttribute : RequiredAttribute
        //private bool IsValidFile( FileByte[] file )
        //{
        //    if ( file == null )
        //    {
        //        return false;
        //    }

        //    //if (file.Length > 1 * 1024 * 1024)
        //    //{
        //    //    return false;
        //    //}

        //    try
        //    {
        //        using ( var img = Image.FromStream( file ) )
        //        {
        //            return img.RawFormat.Equals( ImageFormat.Png ) || img.RawFormat.Equals( ImageFormat.Jpeg );
        //        }
        //    }
        //    catch { }
        //    return false;
        //}
    }
}