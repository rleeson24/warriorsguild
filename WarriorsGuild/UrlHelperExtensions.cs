using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace WarriorsGuild.Utilities
{
    public static class UrlHelperExtensions
    {
        private const string WebRootBasePath = "wwwroot";
        private static readonly ConcurrentDictionary<string, string> CachedFileHashes =
            new ConcurrentDictionary<string, string>();

        public static string ContentVersioned( this IUrlHelper urlHelper, string? contentPath )
        {
            string? url = urlHelper.Content( contentPath );

            // Check if we already cached the file hash in the cache. If not, add it using the inner method.
            string? fileHash = CachedFileHashes.GetOrAdd( url, key =>
            {
                var fileInfo = new FileInfo( WebRootBasePath + key );

                // If file exists, generate a hash of it, otherwise return null.
                return fileInfo.Exists
                    ? ComputeFileHash( fileInfo.OpenRead() )
                    : null;
            } );

            return $"{url}?v={fileHash}";
        }

        private static string? ComputeFileHash( Stream fileStream )
        {
            using ( var hasher = SHA512.Create() )
            using ( fileStream )
            {
                byte[] hashBytes = hasher.ComputeHash( fileStream );

                var sb = new StringBuilder( hashBytes.Length * 2 );

                foreach ( byte hashByte in hashBytes )
                {
                    sb.AppendFormat( "{0:x2}", hashByte );
                }

                return sb.ToString();
            }
        }
    }
}
