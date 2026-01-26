using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace WarriorsGuild.Models
{
    public record FileDetail
    {
        public string ContentType { get; }

        //
        // Summary:
        //     Gets the file name that will be used in the Content-Disposition header of the
        //     response.
        public string FileDownloadName { get; set; } = String.Empty;
        public DateTimeOffset? LastModified { get; set; }
        public EntityTagHeaderValue EntityTag { get; set; }
        public bool EnableRangeProcessing { get; set; }
        
        public readonly string FilePath;

        public FileDetail( string filePath, string fileName = null, string contentType = null )
        {
            ContentType = contentType;
            this.FilePath = filePath;
            FileDownloadName = fileName;
        }

        public Task<HttpResponseMessage> ExecuteAsync( CancellationToken cancellationToken )
        {
            return Task.Run( () =>
             {
                 try
                 {
                     var response = new HttpResponseMessage( HttpStatusCode.OK )
                     {
                         Content = new StreamContent( File.OpenRead( FilePath ) ),
                     };
                     if ( !String.IsNullOrEmpty( FileDownloadName ) )
                     {
                         response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue( "attachment" )
                         {
                             FileName = FileDownloadName
                         };
                     }
                     response.Content.Headers.ContentType = new MediaTypeHeaderValue( ContentType ?? "application/octet-stream" );

                     //using (var img = Image.FromFile(filePath))
                     //{
                     //	using (var ms = new MemoryStream())
                     //	{
                     //		img.Save(ms, img.RawFormat);
                     //		response.Content = new ByteArrayContent(ms.ToArray());
                     //		var contentType = this.contentType ?? MimeMapping.GetMimeMapping(img.RawFormat.ToString());
                     //		response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                     //	}
                     //}

                     return response;
                 }
                 catch ( Exception ex )
                 {
                     return new HttpResponseMessage( HttpStatusCode.NotFound );
                 }
             }, cancellationToken );
        }

        private string GetContentType( string fileExtension )
        {
            switch ( fileExtension )
            {
                case "pdf":
                    return "application/pdf";
                default:
                    throw new Exception( $"fileExtension ({fileExtension}) not mapped" );
            }
        }
    }
}