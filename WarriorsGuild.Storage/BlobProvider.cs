using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WarriorsGuild.Storage.Models;

namespace WarriorsGuild.Storage
{
    public interface IBlobProvider
    {
        Task<FileDownloadResult> DownloadFile( WarriorsGuildFileType fileType, string id );
        Task<FileUploadResult> UploadFileAsync( WarriorsGuildFileType fileType, byte[] file, string id, string mediaType );
    }

    public class BlobProvider : IBlobProvider
    {
        private string _fileNamePrefix;
        private readonly IConfiguration _config;

        public BlobProvider( IConfiguration config )
        {
            _fileNamePrefix = config[ "FilenamePrefix" ].ToString();
            if ( !string.IsNullOrEmpty( _fileNamePrefix ) ) _fileNamePrefix += "-";
            _config = config;
        }

        private async Task<FileUploadResult> UploadBlob( WarriorsGuildFileType fileType, string key, string mediaType, Stream fileStream )
        {
            key = _fileNamePrefix + key;
            // Retrieve reference to a blob
            var blobContainer = GetBlobContainer( MapFileTypeToBlobType( fileType ) );
            var blob = blobContainer.GetBlobClient( key );


            // Upload file into blob storage, basically copying it from local disk into Azure
            fileStream.Position = 0;
            await blob.UploadAsync( fileStream, overwrite: true );

            // Set the blob content type
            var metadata = new Dictionary<string, string>();
            metadata.Add( "ContentType", mediaType );
            blob.SetMetadata( metadata );

            // Create blob upload model with properties from blob info
            var blobUpload = new FileUploadResult
            {
                Path = blob.Name,
                //FileUrl = blob.Uri.AbsoluteUri,
                FileSizeInBytes = (await blob.GetPropertiesAsync()).Value.ContentLength
            };
            return blobUpload;
        }

        private BlobStorageContainer MapFileTypeToBlobType( WarriorsGuildFileType fileType )
        {
            switch ( fileType )
            {
                case WarriorsGuildFileType.RankImage:
                    return BlobStorageContainer.RankImage;
                case WarriorsGuildFileType.RingImage:
                    return BlobStorageContainer.RingImage;
                case WarriorsGuildFileType.CrossImage:
                    return BlobStorageContainer.CrossImage;
                case WarriorsGuildFileType.UserProfilePhoto:
                    return BlobStorageContainer.UserPhoto;
                case WarriorsGuildFileType.Guide:
                    return BlobStorageContainer.Guides;
                case WarriorsGuildFileType.ProofOfCompletion:
                    return BlobStorageContainer.ProofOfCompletion;
                default:
                    throw new Exception( $"{fileType} could not be mapped" );
            }
        }

        private async Task<BlobDownloadModel> DownloadBlob( WarriorsGuildFileType fileType, string blobName )
        {
            blobName = _fileNamePrefix + blobName;
            // TODO: You must implement this helper method. It should retrieve blob info
            // from your database, based on the blobId. The record should contain the
            // blobName, which you should return as the result of this helper method.
            if ( !string.IsNullOrEmpty( blobName ) )
            {
                var container = GetBlobContainer( MapFileTypeToBlobType( fileType ) );
                var blob = container.GetBlobClient( blobName );
                if ( !await blob.ExistsAsync() )
                {
                    throw new FileNotFoundException( $"Blob {blobName} not found in {container.Name}" );
                }

                // Download the blob into a memory stream. Notice that we’re not putting the memory
                // stream in a using statement. This is because we need the stream to be open for the
                // API controller in order for the file to actually be downloadable. The closing and
                // disposing of the stream is handled by the Web API framework.
                var ms = new MemoryStream();
                BlobDownloadInfo dl = await blob.DownloadAsync();
                await dl.Content.CopyToAsync( ms );

                // Strip off any folder structure so the file name is just the file name
                var lastPos = blob.Name.LastIndexOf( '/' );
                var fileName = blob.Name.Substring( lastPos + 1, blob.Name.Length - lastPos - 1 );

                // Build and return the download model with the blob stream and its relevant info
                BlobProperties props = (await blob.GetPropertiesAsync()).Value;
                var download = new BlobDownloadModel
                {
                    BlobStream = ms,
                    BlobFileName = fileName,
                    BlobLength = props.ContentLength,
                    BlobContentType = props.ContentType
                };

                return download;
            }

            // Otherwise
            return null;
        }


        //public Task DownloadToStream(String blobName, Stream str)
        //{
        //	var isDev = System.Configuration.ConfigurationManager.AppSettings[ "IsDev" ] != null;
        //	if (isDev)
        //		blobName = "dev-" + blobName;
        //	// TODO: You must implement this helper method. It should retrieve blob info
        //	// from your database, based on the blobId. The record should contain the
        //	// blobName, which you should return as the result of this helper method.
        //	if (!String.IsNullOrEmpty(blobName))
        //	{
        //		var container = BlobHelper.GetBlobContainer();
        //		var blob = container.GetBlockBlobReference(blobName);

        //		return blob.DownloadToStreamAsync(str);
        //	}
        //	return null;
        //}

        public async Task<FileDownloadResult> DownloadFile( WarriorsGuildFileType fileType, string id )
        {
            var result = await DownloadBlob( fileType, id );

            // Reset the stream position; otherwise, download will not work
            result.BlobStream.Position = 0;

            var filePath = Path.GetTempPath() + id;
            using ( var fs = new FileStream( filePath, FileMode.OpenOrCreate, FileAccess.Write ) )
            {
                result.BlobStream.WriteTo( fs );
                result.BlobStream.Close();
                fs.Close();
            }
            return new FileDownloadResult()
            {
                ContentType = result.BlobContentType,
                BlobFileName = result.BlobFileName,
                BlobLength = result.BlobLength,
                FilePathToServe = filePath
            };
        }

        public async Task<FileUploadResult> UploadFileAsync( WarriorsGuildFileType fileType, byte[] file, string id, string mediaType )
        {
            FileUploadResult uploadResponse = null;
            using ( var fs = new MemoryStream( file ) )
            {
                uploadResponse = await UploadBlob( fileType, id, mediaType, fs );
            }
            return uploadResponse;
        }

        public BlobContainerClient GetBlobContainer( BlobStorageContainer containerType )
        {
            // Pull these from config
            var blobStorageConnectionString = _config[ "BlobStorage:ConnectionString" ];
            var blobStorageContainerName = _config[ $"BlobStorage:{containerType}ContainerName" ];
            // Create blob client and return reference to the container
            var blobStorageAccount = new BlobServiceClient( blobStorageConnectionString );
            return blobStorageAccount.GetBlobContainerClient( blobStorageContainerName );
        }
    }
}