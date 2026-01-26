//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Download;
//using Google.Apis.Drive.v3;
//using Google.Apis.Drive.v3.Data;
//using Google.Apis.Services;
//using Google.Apis.Upload;
//using log4net;
//using WarriorsGuild.Helpers.Utilities;
//using WarriorsGuild.Providers.FileStorage;

//namespace WarriorsGuild.Providers
//{
//	public class GoogleProvider : IFileProvider
//	{
//		private bool _isDev;
//		private string _secretsJsonFilePath;

//		public GoogleProvider()
//		{
//			// initialize the log instance
//			//ApplicationContext.RegisterLogger( sadf );
//			//Logger = ApplicationContext.Logger.ForType<ResumableUpload<Program>>();
//			_isDev = System.Configuration.ConfigurationManager.AppSettings[ "IsDev" ] != null;
//			_secretsJsonFilePath = HttpContextManager.Current.Server.MapPath( "~/client_secrets.json" );
//		}

//		#region Consts

//		private const int KB = 0x400;
//		private const int DownloadChunkSize = 256 * KB;

//		#endregion

//		/// <summary>The logger instance.</summary>
//		private ILog Logger => log4net.LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

//		private string Prefix => (_isDev ? "DEV - " : String.Empty);

//		/// <summary>The Drive API scopes.</summary>
//		private static readonly string[] Scopes = new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive };
//		private DriveService _service;

//		private async Task Configure()
//		{
//			using ( var stream = new System.IO.StreamReader( _secretsJsonFilePath ) )
//			{
//				var credential = GoogleCredential.FromStream( stream.BaseStream ).CreateScoped( Scopes );

//				// Create the  Analytics service.
//				_service = new DriveService( new BaseClientService.Initializer()
//				{
//					HttpClientInitializer = credential,
//					ApplicationName = "WarriorsGuild",
//				} );
//			};
//		}

//		/// <summary>Uploads file asynchronously.</summary>
//		public async Task<FileUploadResult> UploadFileAsync( WarriorsGuildFileType fileType, String srcFileName, String id, String mediaType )
//		{
//			await Configure();
//			File uploadedFile = null;
//			var title = id;
//			if ( title.LastIndexOf( '\\' ) != -1 )
//			{
//				title = title.Substring( title.LastIndexOf( '\\' ) + 1 );
//			}

//			using ( var uploadStream = new System.IO.FileStream( srcFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read ) )
//			{
//				var contentType = folderIds[ fileType ][ 1 ];
//				var gFileDetail = GetFileDetail( id + "." + DetermineFileExtension( fileType ) );
//				if ( gFileDetail != null )
//				{
//					uploadedFile = UpdateFile( uploadStream, contentType, gFileDetail );
//				}
//				else
//				{
//					uploadedFile = CreateFile( fileType, title, uploadStream, contentType );
//				}
//			}

//			return new FileUploadResult()
//			{
//				FileName = uploadedFile.Name,
//				FileSizeInBytes = uploadedFile.Size.Value,
//				FileUrl = uploadedFile.WebContentLink
//			};
//		}

//		private File CreateFile( WarriorsGuildFileType fileType, string title, System.IO.FileStream uploadStream, string contentType )
//		{
//			File uploadedFile = null;
//			var insert = _service.Files.Create( new File
//			{
//				Name = $"{Prefix}{title}.{DetermineFileExtension( fileType )}",
//				Parents = new List<string>
//				{
//					folderIds[fileType][0]
//				}
//			}, uploadStream, contentType );

//			insert.ChunkSize = FilesResource.CreateMediaUpload.MinimumChunkSize * 2;
//			insert.ProgressChanged += HandleProgressChanged;
//			insert.ResponseReceived += file => { uploadedFile = file; };

//			RecordUploadResult( insert.Upload() );

//			return uploadedFile;
//		}

//		private File UpdateFile( System.IO.FileStream uploadStream, string contentType, File gFileDetail )
//		{
//			File uploadedFile = null;
//			var update = _service.Files.Update( new File { Name = gFileDetail.Name }, gFileDetail.Id, uploadStream, contentType );
//			update.ChunkSize = FilesResource.CreateMediaUpload.MinimumChunkSize * 2;
//			update.ProgressChanged += HandleProgressChanged;
//			update.ResponseReceived += file => { uploadedFile = file; };
//			RecordUploadResult( update.Upload() );

//			return uploadedFile;
//		}

//		private void HandleProgressChanged( IUploadProgress progress )
//		{
//			Logger.Debug( progress.Status + " " + progress.BytesSent );
//		}

//		private void RecordUploadResult( IUploadProgress uploadResult )
//		{
//			if ( uploadResult.Exception != null )
//			{
//				Logger.Debug( "Upload Failed. " + uploadResult.Exception );
//			}
//			else if ( uploadResult.Status == Google.Apis.Upload.UploadStatus.Completed )
//			{
//				Logger.Debug( "Upload succeeded" );
//			}
//		}

//		/// <summary>Downloads the media from the given URL.</summary>
//		public async Task<FileDownloadResult> DownloadFile( WarriorsGuildFileType fileType, String id )
//		{
//			var filePath = System.IO.Path.GetTempPath() + id;
//			await Configure();
//			var gFileDetail = GetFileDetail( id + "." + DetermineFileExtension( fileType ) );
//			if ( gFileDetail != null )
//			{
//				var request = _service.Files.Get( gFileDetail.Id );

//				// Add a handler which will be notified on progress changes.
//				// It will notify on each chunk download and when the
//				// download is completed or failed.
//				request.MediaDownloader.ProgressChanged +=
//					( IDownloadProgress progress ) =>
//					{
//						switch ( progress.Status )
//						{
//							case DownloadStatus.Downloading:
//								{
//									Console.WriteLine( progress.BytesDownloaded );
//									break;
//								}
//							case DownloadStatus.Completed:
//								{
//									Console.WriteLine( "Download complete." );
//									break;
//								}
//							case DownloadStatus.Failed:
//								{
//									Console.WriteLine( "Download failed." );
//									break;
//								}
//						}
//					};
//				;
//				using ( var stream = new System.IO.FileStream( filePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write ) )
//				{
//					request.Download( stream );
//				}
//			}
//			return new FileDownloadResult()
//			{
//				FilePathToServe = filePath
//			};
//		}

//		private File GetFileDetail( string fileName )
//		{
//			File file = null;
//			string pageToken = null;
//			do
//			{
//				var request = _service.Files.List();
//				request.Q = $"name='{Prefix + fileName}'";
//				request.Spaces = "drive";
//				request.Fields = "nextPageToken, files(id, name)";  //properties to fetch
//				request.PageToken = pageToken;
//				var result = request.Execute();
//				if ( result.Files.Count() > 1 )
//				{
//					//var re = _service.Files.Delete(result.Files[0].Id );
//					//re.Execute();
//					throw new Exception( "More than 1 file returned for the given name" );
//				}
//				else if ( result.Files.Any() )
//				{
//					file = result.Files[ 0 ];
//				}
//				pageToken = result.NextPageToken;
//			} while ( pageToken != null );
//			return file;
//		}

//		/// <summary>Deletes the given file from drive (not the file system).</summary>
//		public async Task DeleteFile( File file )
//		{
//			await Configure();
//			Console.WriteLine( "Deleting file '{0}'...", file.Id );
//			await _service.Files.Delete( file.Id ).ExecuteAsync();
//			Console.WriteLine( "File was deleted successfully" );
//		}

//		private string DetermineFileExtension( WarriorsGuildFileType fileType )
//		{
//			switch ( fileType )
//			{
//				case WarriorsGuildFileType.RankImage:
//				case WarriorsGuildFileType.RingImage:
//					return "jpg";
//				default:
//					throw new Exception( $"Unmapped file type: {fileType}" );
//			}
//		}

//		private Dictionary<WarriorsGuildFileType, String[]> folderIds = new Dictionary<WarriorsGuildFileType, string[]>()
//		{
//			{ WarriorsGuildFileType.RankImage, new [] { "1NmIOb_eVJzRA7xRoc6m1cHhncBGsRKFS", @"image/jpeg" } },
//			{ WarriorsGuildFileType.RingImage, new [] { "1XZMD4cT94NoAGZgm8OJsyffHE5CbDONX", @"image/jpeg" } }
//		};
//	}
//}