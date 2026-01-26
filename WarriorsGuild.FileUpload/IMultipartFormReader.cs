using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using WarriorsGuild.Models;
using WarriorsGuild.Processes.Models;

namespace WarriorsGuild.Helpers.Utilities.Files
{
    public interface IMultipartFormReader
    {
        Task Read( HttpRequest request, ModelStateDictionary ModelState, string[] permittedExtensions, long fileSizeLimit, Func<MultipartParsedRequest, Task> FileOperation );
        Task<bool> ResolveFileDataAsync( HttpRequestMessage request, Func<MultipartFileData, Task> fileHandlerAsync );
        Task<MultipartParseResult> ParseDataAsync( HttpRequestMessage request, Func<MultipartFormDataStreamProvider, Task<MultipartParseResult>> fileHandlerAsync );
        NameValueCollection GetFormFields( NameValueCollection formData, IEnumerable<string> fieldKeys );
        FileUploadData CreateFileUploadData( MultipartFileData fileData );
        IEnumerable<FileUploadData> CreateFileUploadData( Collection<MultipartFileData> fileData );
    }

    public class MultipartFileData
    {
        public byte[] Content { get; set; }
        public ContentDispositionHeaderValue ContentDisposition { get; set; }
        public string Extension { get; set; }
    }

    public class MultipartParsedRequest
    {
        public IEnumerable<MultipartFileData> FileData { get; set; }
        public NameValueCollection FormData { get; set; }
    }

    public class MultipartFormDataStreamProvider
    {
    }
}