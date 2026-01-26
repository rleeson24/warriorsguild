using AutoFixture;
using JasperFx.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using WarriorsGuild.Areas.Rings.Controllers;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Rings.Status;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Helpers.Utilities.Files;
using WarriorsGuild.Providers;
using WarriorsGuild.Rings;
using WarriorsGuild.Rings.Mappers;
using WarriorsGuild.Rings.Models.Status;
using static Org.BouncyCastle.Math.EC.ECCurve;
using static WarriorsGuild.Tests.TestHelpers;

namespace WarriorsGuild.Tests.Areas.Rings.Controllers
{
    [TestFixture]
    public class RingStatusControllerTests
    {
        protected Fixture _fixture = new Fixture();
        private Guid USER_ID = Guid.NewGuid();
        private MockRepository mockRepository;

        private Mock<IRingsProvider> mockRingsProvider;
        private Mock<IQueryableUserStore<ApplicationUser>> mockUserStore;
        private Mock<IUserProvider> mockUserProvider;
        private Mock<IRecordRingCompletion> mockRecordRingCompletionProcess;
        private Mock<IRingMapper> mockRingMapper;
        private FakeUserManager mockApplicationUserManager;
        private Mock<IMultipartFormReader> mockMultipartReader;
        private Mock<ClaimsIdentity> mockIdentity;
        private Mock<IPrincipal> mockPrincipal;
        private Mock<ISessionManager> _mockSession;
        private IConfiguration mockConfiguration;
        private Mock<IHttpContextAccessor> mockHttpContextAccessor;
        private Mock<ILogger<RingStatusController>> mockLogger;
        private Mock<IWebHostEnvironment> mockWebHostEnvironment;
        private Mock<HttpContext> _mockContext;
        private Mock<HttpRequest> _request;
        private const String _userIdForStatuses = "uidForStatus";
        private readonly Guid userId = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository( MockBehavior.Strict );

            this.mockRingsProvider = this.mockRepository.Create<IRingsProvider>();
            this.mockUserProvider = this.mockRepository.Create<IUserProvider>();
            this.mockRecordRingCompletionProcess = this.mockRepository.Create<IRecordRingCompletion>();
            this.mockRingMapper = this.mockRepository.Create<IRingMapper>();
            this.mockMultipartReader = this.mockRepository.Create<IMultipartFormReader>();
            //this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
            this.mockUserStore = this.mockRepository.Create<IQueryableUserStore<ApplicationUser>>();
            this.mockApplicationUserManager = new FakeUserManager( mockUserStore.Object );
            this.mockHttpContextAccessor = this.mockRepository.Create<IHttpContextAccessor>();
            this._mockSession = this.mockRepository.Create<ISessionManager>();
            this.mockLogger = this.mockRepository.Create<ILogger<RingStatusController>>();
            this.mockWebHostEnvironment = this.mockRepository.Create<IWebHostEnvironment>();
            //var mockConfigurationSection = new Mock<IConfigurationSection>();
            //this.mockConfiguration.Setup( m => m.GetSection( It.IsAny<string>() ) ).Returns( mockConfigurationSection.Object );

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

        private RingStatusController CreateRingStatusController( Stream content = null)
        {
            _request = new Mock<HttpRequest>();
            _request.Setup( x => x.Scheme ).Returns( "http" );
            _request.Setup( x => x.Host ).Returns( HostString.FromUriComponent( "http://localhost:8080" ) );
            _request.Setup( x => x.PathBase ).Returns( PathString.FromUriComponent( "/api" ) );            // Arrange
            _request.Setup( x => x.Body ).Returns( content );

            var httpContext = Mock.Of<HttpContext>( _ =>
                 _.Request == _request.Object
            );

            //Controller needs a controller context 
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            var controller = new RingStatusController(
                this.mockRingsProvider.Object,
                this.mockApplicationUserManager,
                this.mockUserProvider.Object,
                this.mockRecordRingCompletionProcess.Object,
                this.mockConfiguration,
                this.mockRingMapper.Object,
                this.mockHttpContextAccessor.Object,
                this._mockSession.Object,
                this.mockLogger.Object,
                this.mockWebHostEnvironment.Object,
                this.mockMultipartReader.Object )
            { ControllerContext = controllerContext };

            //HttpContextManager.SetCurrentContext( GetMockedHttpContext() );

            return controller;
        }

        //private HttpContextBase GetMockedHttpContext()
        //{
        //    var context = new Mock<HttpContextBase>();
        //    var request = new Mock<HttpRequestBase>();
        //    var response = new Mock<HttpResponseBase>();
        //    _mockSession = mockRepository.Create<HttpSessionStateBase>();
        //    var server = new Mock<HttpServerUtilityBase>();
        //    var user = new Mock<IPrincipal>();
        //    var identity = new Mock<IIdentity>();
        //    var urlHelper = new Mock<UrlHelper>();

        //    var routes = new RouteCollection();
        //    var requestContext = new Mock<RequestContext>();
        //    requestContext.Setup( x => x.HttpContext ).Returns( context.Object );
        //    context.Setup( ctx => ctx.Request ).Returns( request.Object );
        //    context.Setup( ctx => ctx.Response ).Returns( response.Object );
        //    context.Setup( ctx => ctx.Session ).Returns( _mockSession.Object );
        //    context.Setup( ctx => ctx.Server ).Returns( server.Object );
        //    context.Setup( ctx => ctx.User ).Returns( user.Object );
        //    user.Setup( ctx => ctx.Identity ).Returns( identity.Object );
        //    identity.Setup( id => id.IsAuthenticated ).Returns( true );
        //    identity.Setup( id => id.Name ).Returns( "test" );
        //    request.Setup( req => req.Url ).Returns( new Uri( "http://www.google.com" ) );
        //    request.Setup( req => req.RequestContext ).Returns( requestContext.Object );
        //    requestContext.Setup( x => x.RouteData ).Returns( new RouteData() );
        //    request.SetupGet( req => req.Headers ).Returns( new NameValueCollection() );
        //    //var nvc = new NameValueCollection();
        //    //_mockSession.Setup( m => m.Keys ).Returns( nvc.Keys );

        //    return context.Object;
        //}

        private RingStatusController AddClaimToContoller( RingStatusController controller )
        {
            return AddClaimToContoller( controller, true );
        }

        private RingStatusController AddClaimToContoller( RingStatusController controller, bool includeUserId )
        {
            _mockContext = new Mock<HttpContext>( MockBehavior.Strict );
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = _mockContext.Object
            };
            ClaimsPrincipal identity = null;
            if ( includeUserId )
            {
                identity = new ClaimsPrincipal( new ClaimsIdentity( new Claim[] {
                                        new Claim(type: "sub", USER_ID.ToString()),
                                        new Claim(ClaimTypes.Name, "gunnar@somecompany.com")
                                        // other required and custom claims
                                   }, "TestAuthentication" ) );
            }
            else
            {
                identity = new ClaimsPrincipal( new ClaimsIdentity( new Claim[] {
                                   }, "TestAuthentication" ) );
            }
            _mockContext.Setup( m => m.User ).Returns( identity );
            //var claim = new Claim( "id", USER_ID.ToString() );
            //this.mockIdentity = this.mockRepository.Create<ClaimsIdentity>();
            //this.mockIdentity.Setup( m => m.FindFirst( It.IsAny<String>() ) ).Returns( claim );
            //this.mockPrincipal = this.mockRepository.Create<IPrincipal>();
            //this.mockPrincipal.Setup( ip => ip.Identity ).Returns( mockIdentity.Object );
            //controller.User = mockPrincipal.Object;
            return controller;
        }

        private RingStatusController AddRoles( RingStatusController controller )
        {
            mockPrincipal.Setup( p => p.IsInRole( "Warrior" ) ).Returns( true );
            //mockPrincipal.Setup( p => p.IsInRole( "Guardian" ) ).Returns( false );
            return controller;
        }

        [Test]
        public void GetRingStatus_Returns_GetRingStatus_from_provider()
        {
            // Arrange
            var unitUnderTest = CreateRingStatusController();
            var expectedResult = new[] { new RingStatus() }.AsQueryable();
            mockRingsProvider.Setup( m => m.GetRingStatus() ).Returns( expectedResult );
            // Act
            var result = unitUnderTest.GetRingStatus();

            // Assert
            Assert.AreSame( expectedResult, result );
        }

        [Test]
        public async Task GetRingStatus_Given_no_status_found_returns_NotFoundResult()
        {
            // Arrange
            var unitUnderTest = CreateRingStatusController();
            Int32 id = 54;
            mockRingsProvider.Setup( m => m.GetRingStatusAsync( id ) ).Returns( Task.FromResult( default( RingStatus ) ) );

            // Act
            var result = await unitUnderTest.GetRingStatus( id );

            // Assert
            Assert.IsInstanceOf<NotFoundResult>( result.Result );
        }

        [Test]
        public async Task GetRingStatus_Given_no_status_found_returns_OkResult_with_ringStatus()
        {
            // Arrange
            var unitUnderTest = CreateRingStatusController();
            Int32 id = 54;
            var status = _fixture.Build<RingStatus>().Create();
            mockRingsProvider.Setup( m => m.GetRingStatusAsync( id ) ).Returns( Task.FromResult( status ) );

            // Act
            var result = await unitUnderTest.GetRingStatus( id );
            var contentResult = result.Result as OkObjectResult;

            // Assert
            Assert.AreSame( status, contentResult.Value );
        }

        [Test]
        public async Task RecordCompletion_Given_ModelState_invalid_Returns_BadRequest()
        {
            // Arrange
            var unitUnderTest = CreateRingStatusController();
            unitUnderTest.ModelState.AddModelError( "sadfds", "asdfasdfsad" );
            
            var ringToUpdate =_fixture.Build<RingStatusUpdateModel>().Create();

            // Act
            var result = await unitUnderTest.RecordCompletion(
                ringToUpdate );

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>( result );
        }

        [Test]
        public async Task RecordCompletion_Given_Provider_RecordCompletion_Success_Returns_Ok()
        {
            // Arrange
            var unitUnderTest = AddClaimToContoller( CreateRingStatusController() );
            var ringToUpdate = _fixture.Build<RingStatusUpdateModel>().Create();
            var recordCompletionResponse = new RecordRingCompletionResponse()
            {
                Error = String.Empty,
                Success = true
            };
            ApplicationUser user = _fixture.Build<ApplicationUser>().With( u => u.ChildUsers, new List<ApplicationUser>() ).Create();
            mockUserProvider.Setup( m => m.GetMyUserId( unitUnderTest.HttpContext.User ) ).Returns( USER_ID );
            mockRingsProvider.Setup( m => m.RecordCompletionAsync( ringToUpdate, USER_ID ) ).Returns( Task.FromResult( recordCompletionResponse ) );

            // Act
            var result = await unitUnderTest.RecordCompletion( ringToUpdate );

            // Assert
            Assert.IsInstanceOf<OkResult>( result );
        }

        [Test]
        public async Task RecordCompletion_Given_Provider_RecordCompletion_has_error_message_Returns_BadRequest()
        {
            // Arrange
            var unitUnderTest = AddClaimToContoller( CreateRingStatusController(), false );

            var ringToUpdate = _fixture.Build<RingStatusUpdateModel>().Create();
            var recordCompletionResponse = new RecordRingCompletionResponse()
            {
                Error = "No user was supplied for status update.",
                Success = false
            };
            ApplicationUser user = _fixture.Build<ApplicationUser>().With( u => u.ChildUsers, new List<ApplicationUser>() ).Create();
            //mockApplicationUserManager.Setup( m => m.FindByIdAsync( USER_ID.ToString() ) ).Returns( Task.FromResult( user ) );

            mockUserProvider.Setup( m => m.GetMyUserId( unitUnderTest.HttpContext.User ) ).Returns( USER_ID );
            mockRingsProvider.Setup( m => m.RecordCompletionAsync( ringToUpdate, USER_ID ) ).ReturnsAsync( recordCompletionResponse );
            // Act
            var result = await unitUnderTest.RecordCompletion( ringToUpdate );
            var contentResult = result as BadRequestObjectResult;

            // Assert
            Assert.AreEqual( recordCompletionResponse.Error, contentResult.Value );
        }

        [Test]
        public async Task UploadProofOfCompletion_Given_Content_is_not_Multipart_Returns_BadRequest()
        {
            var unitUnderTest = CreateRingStatusController( new MemoryStream( new byte[ 0 ] ) );//, new { new KeyValuePair<>( HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration())} );
            Guid reqId = Guid.NewGuid();
            Guid ringId = Guid.NewGuid();
            var parsedRequest = _fixture.Create<MultipartParsedRequest>();

            mockUserProvider.Setup( x => x.GetMyUserId( unitUnderTest.User ) ).Returns( userId );
            mockMultipartReader.Setup( x => x.Read( _request.Object, unitUnderTest.ModelState, It.Is<string[]>( i => i.SequenceEqual( new string[] { ".png", ".jpg", ".gif", ".pdf" } ) ), mockConfiguration.GetValue<long>( "FileSizeLimit" ), It.IsAny<Func<MultipartParsedRequest, Task>>() )).Returns(Task.CompletedTask)
                .Callback( ( HttpRequest req, ModelStateDictionary modelState, string[] allowedExtensions, long fileSizeLimit, Func<MultipartParsedRequest, Task>  func) => {
                    //frmReader.Invoke(parsedRequest).GetAwaiter().GetResult();
                    //return Task.CompletedTask;
                    unitUnderTest.ModelState.AddModelError( "Request", "Any error" );
                    } );
            // Act
            var result = await unitUnderTest.UploadProofOfCompletion();
            var contentResult = result.Result as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull( contentResult );
            //Assert.IsInstanceOf<ModelStateDictionary>( contentResult.Value );
        }

        [Test]
        [TestCase( null, null )]
        [TestCase( null, "D3F2AF90-6583-412D-B427-307D847EAF12" )]
        [TestCase( "7A5F168C-B3EE-4C56-9FE2-980A0910B8CC", null )]
        public async Task UploadProofOfCompletion_Given_FormData_does_not_contain_reqId_and_ringId_Throws_exception( Guid? reqId, Guid? ringId )
        {

            // Arrange
            //var unitUnderTest = AddClaimToContoller( CreateRingStatusController() );
            //unitUnderTest.Request = new HttpRequestMessage();

            //// Add the text keys/values
            //var formData = new MultipartParseResult();
            //if ( ringId.HasValue ) formData.FormData.Add( "ringId", ringId.ToString() );
            //if ( reqId.HasValue ) formData.FormData.Add( "reqId", reqId.ToString() );

            //unitUnderTest.Request.Content = MultipartFormDataContent( "testing" + Guid.NewGuid(), formData.FormData );
            //unitUnderTest.Request.Properties.Add( HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() );
            //mockUserProvider.Setup( m => m.ValidateUserId( USER_ID.ToString() ) ).Returns( Task.FromResult<string>( null ) );

            //var claim = new Claim( "id", USER_ID.ToString() );
            //this.mockIdentity.Setup( m => m.FindFirst( It.IsAny<String>() ) ).Returns( claim );
            //this.mockPrincipal.Setup( ip => ip.Identity ).Returns( mockIdentity.Object );
            //
            //var streamProvider =_fixture.Create<MultipartFormDataStreamProvider>();
            //mockMultipartReader.Setup( m => m.ParseDataAsync( unitUnderTest.Request, It.IsAny<Func<MultipartFormDataStreamProvider, Task<MultipartParseResult>>>() ) )
            //                    .Callback<HttpRequestMessage, Func<MultipartFormDataStreamProvider, Task<MultipartParseResult>>>( ( req, func ) => func.Invoke( streamProvider ) ).Returns( Task.FromResult( formData ) );

            //try
            //{
            //    // Act
            //    var result = await unitUnderTest.UploadProofOfCompletion();
            //}
            //catch ( Exception ex )
            //{
            //    Assert.Pass();
            //}
            Assert.Fail();
        }

        [Test]
        public async Task UploadProofOfCompletion_Given_FormData_contains_reqId_and_ringId_Returns_Ok()
        {
            //
            //// Arrange
            //var unitUnderTest = AddClaimToContoller( CreateRingStatusController() );
            //unitUnderTest.Request = new HttpRequestMessage();

            //var ringId = Guid.Parse( "D3F2AF90-6583-412D-B427-307D847EAF12" );
            //var reqId = Guid.Parse( "7A5F168C-B3EE-4C56-9FE2-980A0910B8CC" );
            //// Add the text keys/values
            //var formData = new MultipartParseResult();
            //formData.FormData.Add( "ringId", ringId.ToString() );
            //formData.FormData.Add( "reqId", reqId.ToString() );
            //List<Guid> attachmentIds =_fixture.CreateMany<Guid>( 2 ).ToList();
            //formData.AttachmentIds = attachmentIds;

            //unitUnderTest.Request.Content = MultipartFormDataContent( "testingSuccess", formData.FormData );
            //unitUnderTest.Request.Properties.Add( HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() );
            //mockUserProvider.Setup( m => m.ValidateUserId( USER_ID.ToString() ) ).Returns( Task.FromResult<string>( null ) );

            //var claim = new Claim( "id", USER_ID.ToString() );
            //this.mockIdentity.Setup( m => m.FindFirst( It.IsAny<String>() ) ).Returns( claim );
            //this.mockPrincipal.Setup( ip => ip.Identity ).Returns( mockIdentity.Object );
            //var streamProvider =_fixture.Create<MultipartFormDataStreamProvider>();
            //mockMultipartReader.Setup( m => m.ParseDataAsync( unitUnderTest.Request, It.IsAny<Func<MultipartFormDataStreamProvider, Task<MultipartParseResult>>>() ) )
            //                    .Callback<HttpRequestMessage, Func<MultipartFormDataStreamProvider, Task<MultipartParseResult>>>( ( req, func ) => func.Invoke( streamProvider ) ).Returns( Task.FromResult( formData ) );
            //mockMultipartReader.Setup( m => m.GetFormFields( streamProvider.FormData, It.Is<IEnumerable<String>>( s => s.SequenceEqual( new[] { "ringId", "reqId" } ) ) ) ).Returns( formData.FormData );


            //var fileData =_fixture.Build<MultipartFileData>().FromFactory( () => new MultipartFileData() ).Create();
            //var fileUploadData =_fixture.CreateMany<WarriorsGuild.Processes.Models.FileUploadData>( 5 );
            //mockMultipartReader.Setup( m => m.CreateFileUploadData( streamProvider.FileData ) ).Returns( fileUploadData );
            //mockRecordCompletionProcess.Setup( m => m.UploadAttachmentsForRingReq( ringId, reqId, fileUploadData, USER_ID ) )
            //                            .Returns( Task.FromResult( attachmentIds ) );

            //var updateModel =_fixture.Create<RingStatusUpdateModel>();
            //mockRingMapper.Setup( m => m.CreateRingStatusUpdateModel( formData.FormData ) ).Returns( updateModel );

            //var recordCompletionResponse = new RecordCompletionResponse()
            //{
            //    Error = String.Empty,
            //    Success = true
            //};
            //mockRingsProvider.Setup( m => m.RecordCompletionAsync( updateModel, USER_ID ) ).Returns( Task.FromResult( recordCompletionResponse ) );

            //// Act
            //var result = await unitUnderTest.UploadProofOfCompletion();
            //var contentResult = result as ActionResult<IEnumerable<Guid>>;
            //// Assert
            //Assert.IsNotNull( contentResult );
            //Assert.AreSame( attachmentIds, contentResult.Value );
            Assert.Fail();
        }

        [Test]
        public async Task UploadProofOfCompletion_Given_Provider_RecordCompletion_has_error_message_Returns_BadRequest()
        {
            // Arrange
            //var unitUnderTest = AddClaimToContoller( CreateRingStatusController() );
            //unitUnderTest.Request = new HttpRequestMessage();

            //var ringId = Guid.Parse( "D3F2AF90-6583-412D-B427-307D847EAF12" );
            //var reqId = Guid.Parse( "7A5F168C-B3EE-4C56-9FE2-980A0910B8CC" );
            //// Add the text keys/values
            //var formData = new MultipartParseResult();
            //formData.FormData.Add( "ringId", ringId.ToString() );
            //formData.FormData.Add( "reqId", reqId.ToString() );

            //unitUnderTest.Request.Content = MultipartFormDataContent( "testingErrorMessage" + DateTime.UtcNow.ToString( "HHmmssffffff" ), formData.FormData );
            //unitUnderTest.Request.Properties.Add( HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() );
            //mockUserProvider.Setup( m => m.ValidateUserId( USER_ID.ToString() ) ).Returns( Task.FromResult<string>( null ) );

            //var claim = new Claim( "id", USER_ID.ToString() );
            //this.mockIdentity.Setup( m => m.FindFirst( It.IsAny<String>() ) ).Returns( claim );
            //this.mockPrincipal.Setup( ip => ip.Identity ).Returns( mockIdentity.Object );
            //
            //var streamProvider =_fixture.Create<MultipartFormDataStreamProvider>();
            //mockMultipartReader.Setup( m => m.ParseDataAsync( unitUnderTest.Request, It.IsAny<Func<MultipartFormDataStreamProvider, Task<MultipartParseResult>>>() ) )
            //                    .Callback<HttpRequestMessage, Func<MultipartFormDataStreamProvider, Task<MultipartParseResult>>>( ( req, func ) => func.Invoke( streamProvider ) ).Returns( Task.FromResult( formData ) );
            //mockMultipartReader.Setup( m => m.GetFormFields( streamProvider.FormData, It.Is<IEnumerable<String>>( s => s.SequenceEqual( new[] { "ringId", "reqId" } ) ) ) ).Returns( formData.FormData );


            //var fileData =_fixture.Build<MultipartFileData>().FromFactory( () => new MultipartFileData( unitUnderTest.Request.Content.Headers, "sadfds" ) ).Create();
            //var fileUploadData =_fixture.CreateMany<WarriorsGuild.Processes.Models.FileUploadData>( 5 );
            //mockMultipartReader.Setup( m => m.CreateFileUploadData( streamProvider.FileData ) ).Returns( fileUploadData );
            //List<Guid> attachmentIds =_fixture.CreateMany<Guid>( 2 ).ToList();
            //mockRecordCompletionProcess.Setup( m => m.UploadAttachmentsForRingReq( ringId, reqId, fileUploadData, USER_ID ) )
            //                            .Returns( Task.FromResult( attachmentIds ) );

            //var updateModel =_fixture.Create<RingStatusUpdateModel>();
            //mockRingMapper.Setup( m => m.CreateRingStatusUpdateModel( formData.FormData ) ).Returns( updateModel );

            //var recordCompletionResponse = new RecordCompletionResponse()
            //{
            //    Error = "An error occurred",
            //    Success = false
            //};
            //mockRingsProvider.Setup( m => m.RecordCompletionAsync( updateModel, USER_ID ) ).Returns( Task.FromResult( recordCompletionResponse ) );

            //// Act
            //var result = await unitUnderTest.UploadProofOfCompletion();
            //var contentResult = result as BadRequestErrorMessageResult;

            //// Assert
            //Assert.AreEqual( recordCompletionResponse.Error, contentResult.Message );

            Assert.Fail();
        }

        [Test]
        public async Task ApproveProgress_Given_modelState_is_not_valid_Returns_BadRequest()
        {
            // Arrange
            var unitUnderTest = CreateRingStatusController();
            unitUnderTest.ModelState.AddModelError( "UserId", "UserId cannot be null" );
            

            // Act
            var result = await unitUnderTest.ApproveProgress( Guid.NewGuid() );

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>( result );
            mockRingsProvider.Verify( m => m.ApproveProgressAsync( It.IsAny<Guid>(), It.IsAny<Guid>() ), Times.Never() );
        }

        [Test]
        public async Task ApproveProgress_Given_provider_returns_error_Returns_BadRequestResult()
        {
            // Arrange
            var unitUnderTest = AddClaimToContoller( CreateRingStatusController() );
            
            var ringToUpdate = Guid.NewGuid();
            var recordCompletionResponse = new ApproveProgressResponse()
            {
                Error = "No user was supplied for status update.",
                Success = false
            };

            mockUserProvider.Setup( m => m.GetMyUserId( unitUnderTest.HttpContext.User ) ).Returns( USER_ID );
            mockRingsProvider.Setup( m => m.ApproveProgressAsync( ringToUpdate, USER_ID ) ).Returns( Task.FromResult( recordCompletionResponse ) );

            // Act
            var result = await unitUnderTest.ApproveProgress(
                ringToUpdate );
            var contentResult = result as BadRequestObjectResult;

            // Assert
            Assert.AreEqual( recordCompletionResponse.Error, contentResult.Value );
        }

        [Test]
        public async Task ApproveProgress_Given_provider_returns_no_error_Returns_OkResult()
        {
            // Arrange
            var unitUnderTest = AddClaimToContoller( CreateRingStatusController() );
            
            var ringToUpdate = Guid.NewGuid();

            var recordCompletionResponse = new ApproveProgressResponse();
            recordCompletionResponse.Success = true;

            mockUserProvider.Setup( m => m.GetMyUserId( unitUnderTest.HttpContext.User ) ).Returns( USER_ID );
            mockRingsProvider.Setup( m => m.ApproveProgressAsync( ringToUpdate, USER_ID ) ).Returns( Task.FromResult( recordCompletionResponse ) );

            // Act
            var result = await unitUnderTest.ApproveProgress(
                ringToUpdate );

            // Assert
            Assert.IsInstanceOf<OkResult>( result );
        }

        [Test]
        public async Task PostRingStatus_When_ModelState_is_invalid_Returns_InvalidModelResponse()
        {
            // Arrange
            var unitUnderTest = CreateRingStatusController();
            unitUnderTest.ModelState.AddModelError( "sadfds", "asdfasdfsad" );
            
            var ringStatus =_fixture.Build<RingStatus>().Create();

            // Act
            var result = await unitUnderTest.PostRingStatus(
                ringStatus );

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>( result.Result );
        }

        [Test]
        public async Task PostRingStatus_When_successful_Returns_saved_RingStatus()
        {
            // Arrange
            var unitUnderTest = CreateRingStatusController();
            
            var ringStatus =_fixture.Build<RingStatus>().Create();
            var updatedStatus =_fixture.Build<RingStatus>().Create();
            mockRingsProvider.Setup( m => m.PostRingStatusAsync( ringStatus ) ).Returns( Task.FromResult( updatedStatus ) );

            // Act
            var result = await unitUnderTest.PostRingStatus(
                ringStatus );

            // Assert
            var contentResult = result.Result as CreatedAtRouteResult;
            Assert.IsInstanceOf<CreatedAtRouteResult>( result.Result );
            Assert.AreSame( updatedStatus, contentResult.Value );
        }

        //[Test]
        //public async Task DeleteRingStatus_When_no_RingStatus_found_for_id_Returns_NotFoundResult()
        //{
        //	// Arrange
        //	var unitUnderTest = CreateRingStatusController();
        //	int id = 456;
        //	mockRingsProvider.Setup( m => m.GetRingStatusAsync( id ) ).Returns( Task.FromResult( default( RingStatus ) ) );

        //	// Act
        //	var result = await unitUnderTest.DeleteRingStatus(
        //		id );

        //	// Assert
        //	Assert.IsInstanceOf<NotFoundResult>( result );
        //}

        //[Test]
        //public async Task DeleteRingStatus_When_success_Returns_Ok()
        //{
        //	// Arrange
        //	var unitUnderTest = CreateRingStatusController();
        //	int id = 456;
        //	
        //	var ringStatus =_fixture.Build<RingStatus>().Create();
        //	mockRingsProvider.Setup( m => m.GetRingStatusAsync( id ) ).Returns( Task.FromResult( ringStatus ) );
        //	mockRingsProvider.Setup( m => m.DeleteRingStatusAsync( ringStatus ) ).Returns( Task.CompletedTask );

        //	// Act
        //	var result = await unitUnderTest.DeleteRingStatus(
        //		id );
        //	var contentResult = result as ActionResult<RingStatus>;
        //	// Assert
        //	Assert.AreSame( ringStatus, contentResult.Content );
        //	mockRingsProvider.Verify( m => m.DeleteRingStatusAsync( It.IsAny<RingStatus>() ), Times.Once() );

        //}

        private static MultipartFormDataContent MultipartFormDataContent( string testFile, NameValueCollection formData )
        {
            var multiPartContent = new MultipartFormDataContent( "boundary=---011000010111000001101001" );
            using ( var outFile = new StreamWriter( testFile ) )
            {
                outFile.WriteLine( "bleh bleh bleh" );
            }

            var fileStream = new FileStream( testFile, FileMode.Open, FileAccess.Read );
            var streamContent = new StreamContent( fileStream );
            streamContent.Headers.ContentType = new MediaTypeHeaderValue( "multipart/form-data" );

            // Add the file key/value
            multiPartContent.Add( streamContent, "TheFormDataKeyForYourFile", testFile );

            foreach ( string key in formData )
            {
                multiPartContent.Add( new StringContent( formData[ key ] ), key );
            }
            return multiPartContent;
        }
    }
}
