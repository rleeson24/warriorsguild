using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WarriorsGuild.Models;
using WarriorsGuild.Processes.Models;

namespace WarriorsGuild.Helpers.Utilities.Files
{
    public class MultipartFormReader : IMultipartFormReader
    {
        private readonly ILogger<MultipartFormReader> _logger;

        // Get the default form options so that we can use them to set the default 
        // limits for request body data.
        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        public MultipartFormReader( ILogger<MultipartFormReader> logger )
        {
            _logger = logger;
        }

        public async Task Read( HttpRequest request, ModelStateDictionary ModelState, string[] permittedExtensions, long fileSizeLimit,
                                                Func<MultipartParsedRequest, Task> FileOperation )
        {
            if ( !MultipartRequestHelper.IsMultipartContentType( request.ContentType ) )
            {
                ModelState.AddModelError( "File", $"The request couldn't be processed (Error 1)." );
                return;
            }

            var noFiles = true;
            var success = new List<bool>();

            var mediaType = MediaTypeHeaderValue.Parse( request.ContentType );
            var boundary = MultipartRequestHelper.GetBoundary(
                mediaType,
                _defaultFormOptions.MultipartBoundaryLengthLimit );
            var reader = new MultipartReader( boundary, request.Body );
            var section = await reader.ReadNextSectionAsync();

            //var formAccumulator = new KeyValueAccumulator();
            var formAccumulator = new NameValueCollection();
            var fileAccumulator = new List<MultipartFileData>();
            var parsedRequest = new MultipartParsedRequest();
            while ( section != null )
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse( section.ContentDisposition, out var contentDisposition );

                if ( hasContentDispositionHeader )
                {
                    if ( MultipartRequestHelper.HasFileContentDisposition( contentDisposition ) )
                    {
                        success.Add( false );
                        // **WARNING!**
                        // In the following example, the file is saved without
                        // scanning the file's contents. In most production
                        // scenarios, an anti-virus/anti-malware scanner API
                        // is used on the file before making the file available
                        // for download or for use by other systems. 
                        // For more information, see the topic that accompanies 
                        // this sample.

                        var streamedFileContent = await FileHelpers.ProcessStreamedFile(
                        section, contentDisposition, ModelState,
                        permittedExtensions, fileSizeLimit );

                        if ( !ModelState.IsValid )
                        {
                            return;
                        }
                        var file = new MultipartFileData();
                        file.Content = streamedFileContent;
                        file.ContentDisposition = contentDisposition;
                        file.Extension = Path.GetExtension( contentDisposition.FileName.Value ).ToLowerInvariant();
                        fileAccumulator.Add( file );
                        success[ ^1 ] = true;
                        noFiles = false;
                    }
                    else if ( MultipartRequestHelper.HasFormDataContentDisposition( contentDisposition ) )
                    {
                        // Don't limit the key name length because the 
                        // multipart headers length limit is already in effect.
                        var key = HeaderUtilities.RemoveQuotes( contentDisposition.Name ).Value;
                        var encoding = GetEncoding( section );

                        if ( encoding == null )
                        {
                            ModelState.AddModelError( "File", $"The request couldn't be processed (Error 2)." );
                            // Log error

                            return;
                        }

                        using ( var streamReader = new StreamReader(
                            section.Body,
                            encoding,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024,
                            leaveOpen: true ) )
                        {
                            // The value length limit is enforced by 
                            // MultipartBodyLengthLimit
                            var value = await streamReader.ReadToEndAsync();

                            if ( string.Equals( value, "undefined",
                                StringComparison.OrdinalIgnoreCase ) )
                            {
                                value = string.Empty;
                            }

                            //formAccumulator.Append( key, value );
                            if ( !String.IsNullOrEmpty( key ) )
                            {
                                formAccumulator.Add( key, value );
                            }

                            //if ( formAccumulator.ValueCount > _defaultFormOptions.ValueCountLimit )
                            //{
                            //    // Form key count limit of 
                            //    // _defaultFormOptions.ValueCountLimit 
                            //    // is exceeded.
                            //    ModelState.AddModelError( "File",
                            //        $"The request couldn't be processed (Error 3)." );
                            //    // Log error

                            //    return;
                            //}
                        }
                    }
                }

                // Drain any remaining section body that hasn't been consumed and
                // read the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }
            parsedRequest.FileData = fileAccumulator;
            parsedRequest.FormData = formAccumulator;
            if ( noFiles )
            {
                ModelState.AddModelError( "File", $"The request couldn't be processed (Error 2)." );
                _logger.LogError( "File upload request does not have file content disposition" );
                return;
            }
            else if ( !success.All( b => b == true ) )
            {
                if ( success.Any( b => b == true ) )
                {
                    ModelState.AddModelError( "File", $"One or more files failed to be parsed." );
                    _logger.LogError( "Unknown failure occurred", request.Body );
                }
                else
                {
                    ModelState.AddModelError( "File", $"No files could be parsed." );
                    _logger.LogError( "Unknown failure occurred", request.Body );
                }
                return;
            }
            else
            {
                await FileOperation( parsedRequest );
            }
        }

        public async Task<bool> ResolveFileDataAsync( HttpRequestMessage request, Func<MultipartFileData, Task> fileHandlerAsync )
        {
            //var tmpFilePath = HttpContextManager.Current.Server.MapPath( "~/App_Data" );
            //var provider = new MultipartFormDataStreamProvider( tmpFilePath );
            //var multipartData = await request.Content.ReadAsMultipartAsync( provider );
            //foreach ( var fileData in multipartData.FileData )
            //{
            //    await fileHandlerAsync( fileData );
            //    File.Delete( fileData.LocalFileName );
            //}
            return true;
        }

        public async Task<MultipartParseResult> ParseDataAsync( HttpRequestMessage request, Func<MultipartFormDataStreamProvider, Task<MultipartParseResult>> fileHandlerAsync )
        {
            //var tmpFilePath = HttpContextManager.Current.Server.MapPath( "~/App_Data" );
            //var provider = new MultipartFormDataStreamProvider( tmpFilePath );
            //var multipartData = await request.Content.ReadAsMultipartAsync( provider );
            //return await fileHandlerAsync( multipartData );
            return new MultipartParseResult();
        }

        public NameValueCollection GetFormFields( NameValueCollection formData, IEnumerable<string> fieldKeys )
        {
            var result = new NameValueCollection();
            foreach ( var key in formData.AllKeys )
            {
                for ( var i = 0; i < fieldKeys.Count(); i++ )
                {
                    var fieldKey = fieldKeys.Skip( i ).First();
                    if ( key.StartsWith( fieldKey ) )
                        result.Add( fieldKey, formData[ key ] );
                }
            }
            return result;
        }

        public FileUploadData CreateFileUploadData( MultipartFileData fileData )
        {
            return new FileUploadData()
            {
                //FileExtension = Path.GetExtension( fileData.Headers.ContentDisposition.FileName.Replace( "\"", String.Empty ) ),
                //LocalFileName = fileData.LocalFileName,
                //MediaType = fileData.Headers.ContentType.MediaType
            };
        }

        public IEnumerable<FileUploadData> CreateFileUploadData( Collection<MultipartFileData> fileData )
        {
            var response = new List<FileUploadData>();
            foreach ( var fd in fileData )
            {
                response.Add( CreateFileUploadData( fd ) );
            }
            return response;
        }
        private static Encoding GetEncoding( MultipartSection section )
        {
            var hasMediaTypeHeader =
                MediaTypeHeaderValue.TryParse( section.ContentType, out var mediaType );

            // UTF-7 is insecure and shouldn't be honored. UTF-8 succeeds in 
            // most cases.
            if ( !hasMediaTypeHeader || Encoding.UTF7.Equals( mediaType.Encoding ) )
            {
                return Encoding.UTF8;
            }

            return mediaType.Encoding;
        }


        ////or ValidateFileAttribute : RequiredAttribute
        //private bool IsValidFile( FileStream file )
        //{
        //	if ( file == null )
        //	{
        //		return false;
        //	}

        //	//if (file.Length > 1 * 1024 * 1024)
        //	//{
        //	//    return false;
        //	//}

        //	try
        //	{
        //		using ( var img = Image.FromStream( file ) )
        //		{
        //			return img.RawFormat.Equals( ImageFormat.Png ) || img.RawFormat.Equals( ImageFormat.Jpeg );
        //		}
        //	}
        //	catch { }
        //	return false;
        //}
    }
}