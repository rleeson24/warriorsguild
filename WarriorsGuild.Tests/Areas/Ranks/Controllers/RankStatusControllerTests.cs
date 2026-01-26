using AutoFixture;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using WarriorsGuild.Areas.Ranks.Controllers;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Ranks.Status;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Helpers.Utilities.Files;
using WarriorsGuild.Providers;
using WarriorsGuild.Ranks;
using WarriorsGuild.Ranks.Mappers;
using WarriorsGuild.Ranks.Models.Status;
using static WarriorsGuild.Tests.TestHelpers;

namespace WarriorsGuild.Tests.Areas.Ranks.Controllers
{
    [TestFixture]
    public class RankStatusControllerTests
    {
        protected Fixture _fixture = new Fixture();
        private MockRepository mockRepository;

        private Mock<IRanksProvider> mockRanksProvider;
        private Mock<IUserProvider> mockUserProvider;
        private Mock<IRecordCompletion> mockRecordCompletionProcess;
        private Mock<IRankMapper> mockRankMapper;
        private Mock<IQueryableUserStore<ApplicationUser>> mockUserStore;
        private UserManager<ApplicationUser> mockApplicationUserManager;
        private Mock<IMultipartFormReader> mockMultipartReader;
        private Mock<IPrincipal> mockPrincipal;
        private Mock<ILogger<RankStatusController>> mockLogger;
        private Mock<IWebHostEnvironment> mockWebHostEnvironment;
        private IConfiguration mockConfiguration;
        private HttpContext _mockContext;
        private Mock<HttpRequest> _request;
        private Mock<IRankApprovalsProvider> mockRankApprovalsProvider;
        private Mock<IRankStatusProvider> mockRankStatusProvider;
        private readonly Guid _userIdForStatuses = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository( MockBehavior.Strict );

            this.mockRanksProvider = this.mockRepository.Create<IRanksProvider>();
            this.mockUserProvider = this.mockRepository.Create<IUserProvider>();
            this.mockRecordCompletionProcess = this.mockRepository.Create<IRecordCompletion>();
            this.mockRankMapper = this.mockRepository.Create<IRankMapper>();

            this.mockUserStore = this.mockRepository.Create<IQueryableUserStore<ApplicationUser>>();
            this.mockApplicationUserManager = new FakeUserManager( mockUserStore.Object );
            this.mockMultipartReader = this.mockRepository.Create<IMultipartFormReader>();
            this.mockLogger = this.mockRepository.Create<ILogger<RankStatusController>>();
            this.mockWebHostEnvironment = this.mockRepository.Create<IWebHostEnvironment>();
            this.mockRankApprovalsProvider = this.mockRepository.Create<IRankApprovalsProvider>();
            this.mockRankStatusProvider = this.mockRepository.Create<IRankStatusProvider>();

            var inMemorySettings = new Dictionary<string, string> {
                        {"FileSizeLimit", "62515654"},
                        //{"SectionName:SomeKey", "SectionValue"},
                        //...populate as needed for the test
                    };

            mockConfiguration = new ConfigurationBuilder()
                .AddInMemoryCollection( inMemorySettings )
                .Build();
        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        private RankStatusController CreateRankStatusController()
        {
            var controller = new RankStatusController(
                this.mockRanksProvider.Object,
                this.mockApplicationUserManager,
                this.mockUserProvider.Object,
                this.mockRecordCompletionProcess.Object,
                this.mockRankMapper.Object,
                this.mockLogger.Object,
                mockWebHostEnvironment.Object,
                mockConfiguration, mockMultipartReader.Object, mockRankApprovalsProvider.Object, mockRankStatusProvider.Object );

            _mockContext = new DefaultHttpContext();
            //mockHttpContextAccessor.Setup( m => m.HttpContext ).Returns( _mockContext );

            return controller;
        }

        //private HttpContext GetMockedHttpContext()
        //{
        //    var context = new DefaultHttpContext();

        //    //var request = new Mock<HttpRequestBase>();
        //    //var response = new Mock<HttpResponseBase>();
        //    //_mockSession = mockRepository.Create<HttpSessionStateBase>();
        //    //var server = new Mock<HttpServerUtilityBase>();
        //    //var user = new Mock<IPrincipal>();
        //    //var identity = new Mock<IIdentity>();
        //    //var urlHelper = new Mock<UrlHelper>();

        //    //var routes = new RouteCollection();
        //    //var requestContext = new Mock<RequestContext>();
        //    //requestContext.Setup( x => x.HttpContext ).Returns( context.Object );
        //    //context.Setup( ctx => ctx.Request ).Returns( request.Object );
        //    //context.Setup( ctx => ctx.Response ).Returns( response.Object );
        //    //context.Setup( ctx => ctx.Session ).Returns( _mockSession.Object );
        //    //context.Setup( ctx => ctx.Server ).Returns( server.Object );
        //    //context.Setup( ctx => ctx.User ).Returns( user.Object );
        //    //user.Setup( ctx => ctx.Identity ).Returns( identity.Object );
        //    //identity.Setup( id => id.IsAuthenticated ).Returns( true );
        //    //identity.Setup( id => id.Name ).Returns( "test" );
        //    //request.Setup( req => req.Url ).Returns( new Uri( "http://www.google.com" ) );
        //    //request.Setup( req => req.RequestContext ).Returns( requestContext.Object );
        //    //requestContext.Setup( x => x.RouteData ).Returns( new RouteData() );
        //    //request.SetupGet( req => req.Headers ).Returns( new NameValueCollection() );
        //    ////var nvc = new NameValueCollection();
        //    ////_mockSession.Setup( m => m.Keys ).Returns( nvc.Keys );

        //    return context;
        //}

        private RankStatusController AddClaimToContoller( RankStatusController controller )
        {
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = _mockContext
            };
            var identity = new ClaimsPrincipal( new ClaimsIdentity( new Claim[] {
                                                new Claim(type: "sub", _userIdForStatuses.ToString()),
                                                new Claim(ClaimTypes.Name, "gunnar@somecompany.com")
                                                // other required and custom claims
                                           }, "TestAuthentication" ) );
            //var claim = new Claim( "id", USER_ID.ToString() );
            //this.mockIdentity = this.mockRepository.Create<ClaimsIdentity>();
            //this.mockIdentity.Setup( m => m.FindFirst( It.IsAny<String>() ) ).Returns( claim );
            //this.mockPrincipal = this.mockRepository.Create<IPrincipal>( MockBehavior.Loose );
            //this.mockPrincipal.Setup( ip => ip.Identity ).Returns( mockIdentity.Object );
            //this.mockPrincipal.Setup( ip => ip.FindFirst( It.IsAny<string>() ) ).Returns( claim );
            _mockContext.User = identity;
            return controller;
        }

        private RankStatusController AddRoles( RankStatusController controller )
        {
            mockPrincipal.Setup( p => p.IsInRole( "Warrior" ) ).Returns( true );
            //mockPrincipal.Setup( p => p.IsInRole( "Guardian" ) ).Returns( false );
            return controller;
        }

        [Test]
        public void GetRankStatus_Returns_GetRankStatus_from_provider()
        {
            // Arrange
            var unitUnderTest = CreateRankStatusController();
            var expectedResult = new[] { new RankStatus() }.AsQueryable();
            mockRankStatusProvider.Setup( m => m.RankStatuses() ).Returns( expectedResult );
            // Act
            var result = unitUnderTest.GetRankStatus();

            // Assert
            Assert.AreSame( expectedResult, result );
        }

        //[Test]
        //public async Task GetRankStatus_Given_no_status_found_returns_NotFoundResult()
        //{
        //    // Arrange
        //    var unitUnderTest = CreateRankStatusController();
        //    Int32 id = 54;
        //    mockRanksProvider.Setup( m => m.GetStatusesAsync( id ) ).Returns( Task.FromResult( default( RankStatus ) ) );

        //    // Act
        //    var result = await unitUnderTest.GetRankStatus( id );

        //    // Assert
        //    Assert.IsInstanceOf( typeof( NotFoundResult ), result );
        //}

        //[Test]
        //public async Task GetRankStatus_Given_no_status_found_returns_OkResult_with_rankStatus()
        //{
        //    // Arrange
        //    var unitUnderTest = CreateRankStatusController();
        //    Int32 id = 54;
        //    
        //    var status =_fixture.Build<RankStatus>().Create();
        //    mockRanksProvider.Setup( m => m.GetStatusesAsync( id ) ).Returns( Task.FromResult( status ) );

        //    // Act
        //    var result = await unitUnderTest.GetRankStatus( id );
        //    var contentResult = result as ActionResult<RankStatus>;

        //    // Assert
        //    Assert.AreSame( status, contentResult.Content );
        //}

        [Test]
        public async Task RecordCompletion_Given_ModelState_invalid_Returns_BadRequest()
        {
            // Arrange
            var unitUnderTest = CreateRankStatusController();
            unitUnderTest.ModelState.AddModelError( "sadfds", "asdfasdfsad" );
            
            var rankToUpdate =_fixture.Build<RankStatusUpdateModel>().Create();

            // Act
            var result = await unitUnderTest.RecordCompletion(
                rankToUpdate );

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>( result );
        }



        [Test]
        public async Task RecordCompletion_Given_Provider_RecordCompletion_Success_Returns_Ok()
        {
            // Arrange
            var unitUnderTest = AddClaimToContoller( CreateRankStatusController() );
            
            var rankToUpdate =_fixture.Build<RankStatusUpdateModel>().Create();
            var recordCompletionResponse = new RecordCompletionResponse()
            {
                Error = String.Empty,
                Success = true
            };

            mockUserProvider.Setup( m => m.GetMyUserId( unitUnderTest.HttpContext.User ) ).Returns( _userIdForStatuses );
            //mockUserProvider.Setup( m => m.ValidateUserId( _userIdForStatuses.ToString() ) ).Returns( Task.FromResult<string>( null ) );
            mockRecordCompletionProcess.Setup( m => m.RecordCompletionAsync( rankToUpdate, _userIdForStatuses ) ).Returns( Task.FromResult( recordCompletionResponse ) );

            // Act
            var result = await unitUnderTest.RecordCompletion( rankToUpdate );

            // Assert
            Assert.IsInstanceOf<OkResult>( result );
        }

        [Test]
        public async Task RecordCompletion_Given_Provider_RecordCompletion_has_error_message_Returns_BadRequest()
        {
            // Arrange
            var unitUnderTest = AddClaimToContoller( CreateRankStatusController() );
            
            var rankToUpdate =_fixture.Build<RankStatusUpdateModel>().Create();
            var recordCompletionResponse = new RecordCompletionResponse()
            {
                Error = "No user was supplied for status update.",
                Success = false
            };
            mockUserProvider.Setup( m => m.GetMyUserId( unitUnderTest.HttpContext.User ) ).Returns( _userIdForStatuses );
            mockRecordCompletionProcess.Setup( m => m.RecordCompletionAsync( rankToUpdate, _userIdForStatuses ) ).Returns( Task.FromResult( recordCompletionResponse ) );

            // Act
            var result = await unitUnderTest.RecordCompletion( rankToUpdate );
            var contentResult = result as BadRequestObjectResult;

            // Assert
            Assert.AreEqual( recordCompletionResponse.Error, contentResult.Value );
        }

        //        [Test]
        //        public async Task UploadProofOfCompletion_Given_Content_is_not_Multipart_Returns_BadRequest()
        //        {
        //            
        //            _request = new Mock<HttpRequest>();
        //            var parsedRequest = new MultipartParsedRequest();
        //            var formFields = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>();
        //            formFields.Add( "rankId", Guid.NewGuid().ToString( "D" ) );
        //            formFields.Add( "reqId", Guid.NewGuid().ToString( "D" ) );
        //            var files = new FormFileCollection();
        //            //_mockContext.Request.Headers[ "X-Custom-Header" ] = "88-test-tcb";
        //            var formData = new FormCollection( formFields, files );
        //            _request.Setup( x => x.Form ).Returns( formData );
        //            //_request.Setup( x => x.Host ).Returns( HostString.FromUriComponent( "http://localhost:5080" ) );
        //            //_request.Setup( x => x.PathBase ).Returns( PathString.FromUriComponent( "/api" ) );
        //            //_request.Setup( x => x.Body ).Returns( new MemoryStream( new byte[ 0 ] ) );

        //            //Arrange
        //            var unitUnderTest = AddClaimToContoller( CreateRankStatusController() );
        //            var reqId = Guid.NewGuid();
        //            var rankId = Guid.NewGuid();

        //            var attachmentIds = new List<Guid>();
        //            IEnumerable<MultipartFileData> fileData = new List<MultipartFileData>();
        //            mockRecordCompletionProcess.Setup( m => m.UploadAttachmentsForRankReq( rankId, reqId, fileData, _userIdForStatuses ) ).Returns( Task.FromResult<List<Guid>>( attachmentIds ) );
        //            var rankToUpdate =_fixture.Create<RankStatusUpdateModel>();
        //            mockRankMapper.Setup( m => m.CreateRankStatusUpdateModel( rankId, reqId, new Guid[ 0 ], new Guid[ 0 ] ) ).Returns( rankToUpdate );
        //            var statusUpdateResponse =_fixture.Create<RecordCompletionResponse>();
        //            statusUpdateResponse.Success = true;
        //            statusUpdateResponse.Error = String.Empty;
        //            mockRanksProvider.Setup( m => m.RecordCompletionAsync( rankToUpdate, _userIdForStatuses ) ).Returns( Task.FromResult( statusUpdateResponse ) );
        //            var formParserTask = new Task( () =>
        //             {
        //                 System.Threading.Thread.Sleep( 3000 );
        //             } );

        //            mockMultipartReader.Setup( m => m.Read( unitUnderTest.Request, unitUnderTest.ModelState, new[] { ".png", ".jpg", ".gif", ".pdf" }, mockConfiguration.GetValue<long>( "FileSizeLimit" ), It.IsAny<Func<MultipartParsedRequest, Task>>() ) )
        //            .Callback<HttpRequest, ModelStateDictionary, string[], long, Func<MultipartParsedRequest, Task>>( ( r, msd, extentions, sizeLimit, lambda ) =>
        //             {
        //                 lambda.Invoke( parsedRequest );
        //             } )
        //            .Returns( formParserTask );

        //            //            unitUnderTest.Request.Properties.Add( HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() );

        //            // Act
        //            var result = await unitUnderTest.UploadProofOfCompletion();
        //            var contentResult = result as BadRequestObjectResult;

        //            // Assert
        //            Assert.IsNotNull( contentResult );
        //            Assert.AreEqual( "Request content was not form data", contentResult.Value as String );
        //        }

        //        [Test]
        //        [TestCase( null, null )]
        //        [TestCase( null, "D3F2AF90-6583-412D-B427-307D847EAF12" )]
        //        [TestCase( "7A5F168C-B3EE-4C56-9FE2-980A0910B8CC", null )]
        //        public async Task UploadProofOfCompletion_Given_FormData_does_not_contain_reqId_and_rankId_Throws_exception( Guid? reqId, Guid? rankId )
        //        {

        //            // Arrange
        //            //var unitUnderTest = AddClaimToContoller( CreateRankStatusController() );
        //            //unitUnderTest.Request = new HttpRequestMessage();

        //            //// Add the text keys/values
        //            //var formData = new MultipartParseResult();
        //            //if ( rankId.HasValue ) formData.FormData.Add( "rankId", rankId.ToString() );
        //            //if ( reqId.HasValue ) formData.FormData.Add( "reqId", reqId.ToString() );

        //            //unitUnderTest.Request.Content = MultipartFormDataContent( "testing" + Guid.NewGuid(), formData.FormData );
        //            //unitUnderTest.Request.Properties.Add( HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() );
        //            //mockUserProvider.Setup( m => m.ValidateUserId( USER_ID.ToString() ) ).Returns( Task.FromResult<string>( null ) );

        //            //var claim = new Claim( "id", USER_ID.ToString() );
        //            //this.mockIdentity.Setup( m => m.FindFirst( It.IsAny<String>() ) ).Returns( claim );
        //            //this.mockPrincipal.Setup( ip => ip.Identity ).Returns( mockIdentity.Object );
        //            //
        //            //var streamProvider =_fixture.Create<MultipartFormDataStreamProvider>();
        //            //mockMultipartReader.Setup( m => m.ParseDataAsync( unitUnderTest.Request, It.IsAny<Func<MultipartFormDataStreamProvider, Task<MultipartParseResult>>>() ) )
        //            //                    .Callback<HttpRequestMessage, Func<MultipartFormDataStreamProvider, Task<MultipartParseResult>>>( ( req, func ) => func.Invoke( streamProvider ) ).Returns( Task.FromResult( formData ) );

        //            //try
        //            //{
        //            //    // Act
        //            //    var result = await unitUnderTest.UploadProofOfCompletion();
        //            //}
        //            //catch ( Exception ex )
        //            //{
        //            //    Assert.Pass();
        //            //}
        //            Assert.Fail();
        //        }

        //        [Test]
        //        public async Task UploadProofOfCompletion_Given_FormData_contains_reqId_and_rankId_and_FileData_Returns_Ok()
        //        {
        //            var reqId = Guid.NewGuid();
        //            var rankId = Guid.NewGuid();
        //            
        //            _request = new Mock<HttpRequest>();
        //            var parsedRequest =_fixture.Create<MultipartParsedRequest>();
        //            var formFields = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>();
        //            parsedRequest.FormData.Add( "rankId", rankId.ToString( "D" ) );
        //            parsedRequest.FormData.Add( "reqId", reqId.ToString( "D" ) );
        //            var files = new FormFileCollection();
        //            //_mockContext.Request.Headers[ "X-Custom-Header" ] = "88-test-tcb";
        //            var formData = new FormCollection( formFields, files );
        //            _request.Setup( x => x.Form ).Returns( formData );
        //            //_request.Setup( x => x.Host ).Returns( HostString.FromUriComponent( "http://localhost:5080" ) );
        //            //_request.Setup( x => x.PathBase ).Returns( PathString.FromUriComponent( "/api" ) );
        //            //_request.Setup( x => x.Body ).Returns( new MemoryStream( new byte[ 0 ] ) );

        //            //Arrange
        //            var unitUnderTest = AddClaimToContoller( CreateRankStatusController() );

        //            var attachmentIds = new List<Guid>();
        //            IEnumerable<MultipartFileData> fileData = new List<MultipartFileData>();
        //            mockRecordCompletionProcess.Setup( m => m.UploadAttachmentsForRankReq( rankId, reqId, fileData, _userIdForStatuses ) ).Returns( Task.FromResult<List<Guid>>( attachmentIds ) );
        //            var rankToUpdate =_fixture.Create<RankStatusUpdateModel>();
        //            mockRankMapper.Setup( m => m.CreateRankStatusUpdateModel( rankId, reqId, new Guid[ 0 ], new Guid[ 0 ] ) ).Returns( rankToUpdate );
        //            var statusUpdateResponse =_fixture.Create<RecordCompletionResponse>();
        //            statusUpdateResponse.Success = true;
        //            statusUpdateResponse.Error = String.Empty;
        //            mockRanksProvider.Setup( m => m.RecordCompletionAsync( rankToUpdate, _userIdForStatuses ) ).Returns( Task.FromResult( statusUpdateResponse ) );
        //            var formParserTask = new Task( () =>
        //            {
        //                System.Threading.Thread.Sleep( 3000 );
        //            } );

        //            mockMultipartReader.Setup( m => m.Read( unitUnderTest.Request, unitUnderTest.ModelState, new[] { ".png", ".jpg", ".gif", ".pdf" }, mockConfiguration.GetValue<long>( "FileSizeLimit" ), It.IsAny<Func<MultipartParsedRequest, Task>>() ) )
        //            .Callback<HttpRequest, ModelStateDictionary, string[], long, Func<MultipartParsedRequest, Task>>( async ( r, msd, extentions, sizeLimit, lambda ) =>
        //            {
        //                await lambda( parsedRequest );
        //            } )
        //            .Returns( formParserTask );
        //            //            unitUnderTest.Request.Properties.Add( HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() );

        //            // Act
        //            var result = await unitUnderTest.UploadProofOfCompletion();
        //            var contentResult = result as CreatedResult;

        //            // Assert
        //            Assert.IsNotNull( contentResult );
        //            Assert.AreEqual( "Request content was not form data", contentResult.Value as String );
        //        }

        [Test]
        public async Task UploadProofOfCompletion_Given_Provider_RecordCompletion_has_error_message_Returns_BadRequest()
        {
            // Arrange
            //var unitUnderTest = AddClaimToContoller( CreateRankStatusController() );
            //unitUnderTest.Request = new HttpRequestMessage();

            //var rankId = Guid.Parse( "D3F2AF90-6583-412D-B427-307D847EAF12" );
            //var reqId = Guid.Parse( "7A5F168C-B3EE-4C56-9FE2-980A0910B8CC" );
            //// Add the text keys/values
            //var formData = new MultipartParseResult();
            //formData.FormData.Add( "rankId", rankId.ToString() );
            //formData.FormData.Add( "reqId", reqId.ToString() );

            //unitUnderTest.Request.Content = MultipartFormDataContent( "testingErrorMessage", formData.FormData );
            //unitUnderTest.Request.Properties.Add( HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() );
            //mockUserProvider.Setup( m => m.ValidateUserId( USER_ID.ToString() ) ).Returns( Task.FromResult<string>( null ) );

            //var claim = new Claim( "id", USER_ID.ToString() );
            //this.mockIdentity.Setup( m => m.FindFirst( It.IsAny<String>() ) ).Returns( claim );
            //this.mockPrincipal.Setup( ip => ip.Identity ).Returns( mockIdentity.Object );
            //
            //var streamProvider =_fixture.Create<MultipartFormDataStreamProvider>();
            //mockMultipartReader.Setup( m => m.ParseDataAsync( unitUnderTest.Request, It.IsAny<Func<MultipartFormDataStreamProvider, Task<MultipartParseResult>>>() ) )
            //                    .Callback<HttpRequestMessage, Func<MultipartFormDataStreamProvider, Task<MultipartParseResult>>>( ( req, func ) => func.Invoke( streamProvider ) ).Returns( Task.FromResult( formData ) );
            //mockMultipartReader.Setup( m => m.GetFormFields( streamProvider.FormData, It.Is<IEnumerable<String>>( s => s.SequenceEqual( new[] { "rankId", "reqId" } ) ) ) ).Returns( formData.FormData );


            //var fileData =_fixture.Build<MultipartFileData>().FromFactory( () => new MultipartFileData( unitUnderTest.Request.Content.Headers, "sadfds" ) ).Create();
            //var fileUploadData =_fixture.CreateMany<WarriorsGuild.Processes.Models.FileUploadData>( 5 );
            //mockMultipartReader.Setup( m => m.CreateFileUploadData( streamProvider.FileData ) ).Returns( fileUploadData );
            //List<Guid> attachmentIds =_fixture.CreateMany<Guid>( 2 ).ToList();
            //mockRecordCompletionProcess.Setup( m => m.UploadAttachmentsForRankReq( rankId, reqId, fileUploadData, USER_ID ) )
            //                            .Returns( Task.FromResult( attachmentIds ) );

            //var updateModel =_fixture.Create<RankStatusUpdateModel>();
            //mockRankMapper.Setup( m => m.CreateRankStatusUpdateModel( formData.FormData, It.Is<Guid[]>( s => !s.Any() ), It.Is<Guid[]>( s => !s.Any() ) ) ).Returns( updateModel );

            //var recordCompletionResponse = new RecordCompletionResponse()
            //{
            //    Error = "An error occurred",
            //    Success = false
            //};
            //mockRanksProvider.Setup( m => m.RecordCompletionAsync( updateModel, USER_ID ) ).Returns( Task.FromResult( recordCompletionResponse ) );

            //// Act
            //var result = await unitUnderTest.UploadProofOfCompletion();
            //var contentResult = result as BadRequestErrorMessageResult;

            //// Assert
            //Assert.AreEqual( recordCompletionResponse.Error, contentResult.Message );
            //Assert.Fail();
        }

        [Test]
        public async Task ApproveProgress_Given_modelState_is_not_valid_Returns_BadRequest()
        {
            // Arrange
            var unitUnderTest = CreateRankStatusController();
            unitUnderTest.ModelState.AddModelError( "UserId", "UserId cannot be null" );
            
            var approvalRecordId = Guid.NewGuid();

            // Act
            var result = await unitUnderTest.ApproveProgress( approvalRecordId );

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>( result );
            mockRankApprovalsProvider.Verify( m => m.ApproveProgressAsync( It.IsAny<Guid>(), It.IsAny<Guid>() ), Times.Never() );
        }

        [Test]
        public async Task ApproveProgress_Given_provider_returns_error_Returns_BadRequestResult()
        {
            // Arrange
            var unitUnderTest = AddClaimToContoller( CreateRankStatusController() );
            
            var approvalRecordId = Guid.NewGuid();
            var recordCompletionResponse = new RecordCompletionResponse()
            {
                Error = "No user was supplied for status update.",
                Success = false
            };
            mockUserProvider.Setup( m => m.GetMyUserId( unitUnderTest.HttpContext.User ) ).Returns( _userIdForStatuses );

            mockRankApprovalsProvider.Setup( m => m.ApproveProgressAsync( approvalRecordId, _userIdForStatuses ) ).Returns( Task.FromResult( recordCompletionResponse ) );

            // Act
            var result = await unitUnderTest.ApproveProgress(
                approvalRecordId );
            var contentResult = result as BadRequestObjectResult;

            // Assert
            Assert.AreEqual( recordCompletionResponse.Error, contentResult.Value );
        }

        [Test]
        public async Task ApproveProgress_Given_provider_returns_no_error_Returns_OkResult()
        {
            // Arrange
            var unitUnderTest = AddClaimToContoller( CreateRankStatusController() );
            
            var approvalRecordId = Guid.NewGuid();

            mockUserProvider.Setup( m => m.GetMyUserId( unitUnderTest.HttpContext.User ) ).Returns( _userIdForStatuses );

            var recordCompletionResponse = new RecordCompletionResponse();
            recordCompletionResponse.Success = true;

            mockRankApprovalsProvider.Setup( m => m.ApproveProgressAsync( approvalRecordId, _userIdForStatuses ) ).Returns( Task.FromResult( recordCompletionResponse ) );

            // Act
            var result = await unitUnderTest.ApproveProgress(
                approvalRecordId );

            // Assert
            Assert.IsInstanceOf<OkResult>( result );
        }

        [Test]
        public async Task PostRankStatus_When_ModelState_is_invalid_Returns_InvalidModelResponse()
        {
            // Arrange
            var unitUnderTest = CreateRankStatusController();
            unitUnderTest.ModelState.AddModelError( "sadfds", "asdfasdfsad" );
            
            var rankStatus =_fixture.Build<RankStatus>().Create();

            // Act
            var result = await unitUnderTest.PostRankStatus(
                rankStatus );

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>( result.Result );
        }

        [Test]
        public async Task PostRankStatus_When_successful_Returns_saved_RankStatus()
        {
            // Arrange
            var unitUnderTest = CreateRankStatusController();
            
            var rankStatus =_fixture.Build<RankStatus>().Create();
            var updatedStatus =_fixture.Build<RankStatus>().Create();
            mockRankStatusProvider.Setup( m => m.PostRankStatusAsync( rankStatus ) ).Returns( Task.FromResult( updatedStatus ) );

            // Act
            var result = await unitUnderTest.PostRankStatus(
                rankStatus );
            var contentResult = result.Result as CreatedAtRouteResult;
            // Assert
            Assert.IsInstanceOf<CreatedAtRouteResult>( contentResult );
            Assert.AreSame( updatedStatus, contentResult.Value );
        }

        //[Test]
        //public async Task DeleteRankStatus_When_no_RankStatus_found_for_id_Returns_NotFoundResult()
        //{
        //	// Arrange
        //	var unitUnderTest = CreateRankStatusController();
        //	int id = 456;
        //	mockRanksProvider.Setup( m => m.GetRankStatusesAsync( id ) ).Returns( Task.FromResult( default( RankStatus ) ) );

        //	// Act
        //	var result = await unitUnderTest.DeleteRankStatus(
        //		id );

        //	// Assert
        //	Assert.IsInstanceOf<NotFoundResult>( result );
        //}

        //[Test]
        //public async Task DeleteRankStatus_When_success_Returns_Ok()
        //{
        //	// Arrange
        //	var unitUnderTest = CreateRankStatusController();
        //	int id = 456;
        //	
        //	var rankStatus =_fixture.Build<RankStatus>().Create();
        //	mockRanksProvider.Setup( m => m.GetRankStatusesAsync( id ) ).Returns( Task.FromResult( rankStatus ) );
        //	mockRanksProvider.Setup( m => m.DeleteRankStatusAsync( rankStatus ) ).Returns( Task.CompletedTask );

        //	// Act
        //	var result = await unitUnderTest.DeleteRankStatus(
        //		id );
        //	var contentResult = result as ActionResult<RankStatus>;
        //	// Assert
        //	Assert.AreSame( rankStatus, contentResult.Content );
        //	mockRanksProvider.Verify( m => m.DeleteRankStatusAsync( It.IsAny<RankStatus>() ), Times.Once() );

        //}

        //        private static MultipartFormDataContent MultipartFormDataContent( string testFile, NameValueCollection formData )
        //        {
        //            var multiPartContent = new MultipartFormDataContent( "boundary=---011000010111000001101001" );
        //            using ( var outFile = new StreamWriter( testFile ) )
        //            {
        //                outFile.WriteLine( "bleh bleh bleh" );
        //            }

        //            var fileStream = new FileStream( testFile, FileMode.Open, FileAccess.Read );
        //            var streamContent = new StreamContent( fileStream );
        //            streamContent.Headers.ContentType = new MediaTypeHeaderValue( "multipart/form-data" );

        //            // Add the file key/value
        //            multiPartContent.Add( streamContent, "TheFormDataKeyForYourFile", testFile );

        //            foreach ( string key in formData )
        //            {
        //                multiPartContent.Add( new StringContent( formData[ key ] ), key );
        //            }
        //            return multiPartContent;
        //        }
    }
}
