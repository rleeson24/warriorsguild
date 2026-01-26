//using AutoFixture;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using NUnit.Framework;
//using System;
//using System.Collections.Specialized;
//using System.Security.Claims;
//using System.Security.Principal;
//using System.Threading.Tasks;
//using WarriorsGuild.Areas.Account;
//using WarriorsGuild.Areas.Account.Models;
//using WarriorsGuild.Controllers;
//using WarriorsGuild.DataAccess;
//using WarriorsGuild.Helpers.Utilities;
//using WarriorsGuild.Models.Account;
//using WarriorsGuild.Providers;
//using WarriorsGuild.Providers.Payments;
//using static WarriorsGuild.Tests.TestHelpers;

//namespace WarriorsGuild.Tests.Controllers
//{
//    [TestFixture]
//    public class AccountControllerTests
//    {
//        private MockRepository mockRepository;

//        private Mock<IUserRelationshipManager> mockUserRelationshipManager;
//        private Mock<IQueryableUserStore<ApplicationUser>> mockUserStore;
//        private FakeUserManager mockApplicationUserManager;
//        private FakeSignInManager mockApplicationSignInManager;
//        private Mock<SubscriptionManager> mockSubscriptionManager;
//        private Mock<IGuildDbContext> mockDatabase;
//        private Mock<IEmailProvider> mockEmailProvider;

//        //private Mock<ClaimsIdentity> mockIdentity;
//        //private Mock<IPrincipal> mockPrincipal;
//        private Mock<ISessionManager> _mockSession;
//        private Mock<ClaimsPrincipal> _mockedUser;
//        private Mock<IHttpContextAccessor> mockContextAccessor;
//        private bool _userIsAuthenticated;

//        [SetUp]
//        public void SetUp()
//        {
//            this.mockRepository = new MockRepository( MockBehavior.Strict );
//            var context = new Mock<HttpContext>();
//            mockContextAccessor = new Mock<IHttpContextAccessor>();
//            mockContextAccessor.Setup( x => x.HttpContext ).Returns( context.Object );

//            this.mockUserRelationshipManager = this.mockRepository.Create<IUserRelationshipManager>();
//            this.mockUserStore = this.mockRepository.Create<IQueryableUserStore<ApplicationUser>>();
//            this.mockApplicationUserManager = new FakeUserManager( mockUserStore.Object );
//            this.mockApplicationSignInManager = new FakeSignInManager( mockContextAccessor.Object, mockUserStore.Object ); ;
//            this.mockSubscriptionManager = this.mockRepository.Create<SubscriptionManager>( MockBehavior.Strict );
//            this.mockDatabase = this.mockRepository.Create<IGuildDbContext>();
//            this.mockEmailProvider = this.mockRepository.Create<IEmailProvider>();
//            this._mockSession = this.mockRepository.Create<ISessionManager>();
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            this.mockRepository.VerifyAll();
//        }

//        private AccountController CreateAccountController()
//        {
//            var ctrl = new AccountController(
//                this.mockUserRelationshipManager.Object,
//                this.mockApplicationUserManager,
//                this.mockEmailProvider.Object,
//                this.mockApplicationSignInManager,
//                this.mockSubscriptionManager.Object,
//                this.mockDatabase.Object,
//                this.mockContextAccessor.Object,
//                this._mockSession.Object,);
//            return ctrl;
//        }

//        //private HttpContextBase GetMockedHttpContext()
//        //{
//        //    var context = new Mock<HttpContextBase>();
//        //    var request = new Mock<HttpRequestBase>();
//        //    var response = new Mock<HttpResponseBase>();
//        //    _mockSession = mockRepository.Create<HttpSessionStateBase>();
//        //    var server = new Mock<HttpServerUtilityBase>();
//        //    _mockedUser = new Mock<ClaimsPrincipal>();
//        //    var identity = new Mock<IIdentity>();
//        //    var urlHelper = new Mock<UrlHelper>();

//        //    var routes = new RouteCollection();
//        //    var requestContext = new Mock<RequestContext>();
//        //    requestContext.Setup( x => x.HttpContext ).Returns( context.Object );
//        //    context.Setup( ctx => ctx.Request ).Returns( request.Object );
//        //    context.Setup( ctx => ctx.Response ).Returns( response.Object );
//        //    context.Setup( ctx => ctx.Session ).Returns( _mockSession.Object );
//        //    context.Setup( ctx => ctx.Server ).Returns( server.Object );
//        //    context.Setup( ctx => ctx.User ).Returns( _mockedUser.Object );
//        //    _mockedUser.Setup( ctx => ctx.Identity ).Returns( identity.Object );
//        //    identity.Setup( id => id.IsAuthenticated ).Returns( _userIsAuthenticated );
//        //    identity.Setup( id => id.Name ).Returns( "test" );
//        //    request.Setup( req => req.Url ).Returns( new Uri( "http://www.google.com" ) );
//        //    request.Setup( req => req.RequestContext ).Returns( requestContext.Object );
//        //    requestContext.Setup( x => x.RouteData ).Returns( new RouteData() );
//        //    request.SetupGet( req => req.Headers ).Returns( new NameValueCollection() );
//        //    //var nvc = new NameValueCollection();
//        //    //_mockSession.Setup( m => m.Keys ).Returns( nvc.Keys );

//        //    return context.Object;
//        //}

//        [Test]
//        [TestCase( "url" )]
//        [TestCase( "url2" )]
//        public void Login_When_the_user_is_not_authenticated_Returns_the_view_with_the_ReturnUrl_on_the_ViewBag( String returnUrl )
//        {
//            // Arrange
//            var unitUnderTest = CreateAccountController();

//            // Act
//            var result = unitUnderTest.Login( returnUrl );

//            // Assert
//            Assert.IsInstanceOf<ViewResult>( result );
//            Assert.AreEqual( returnUrl, unitUnderTest.ViewBag.ReturnUrl );
//        }

//        [Test]
//        [TestCase( "url" )]
//        [TestCase( "url2" )]
//        public void Login_When_user_isAuthenticated_Returns_the_Home_view( String returnUrl )
//        {
//            _userIsAuthenticated = true;
//            // Arrange
//            var unitUnderTest = CreateAccountController();

//            // Act
//            var result = unitUnderTest.Login( returnUrl );

//            // Assert
//            Assert.IsInstanceOf<ViewResult>( result );
//            Assert.AreEqual( returnUrl, unitUnderTest.ViewBag.ReturnUrl );
//        }

//        [Test]
//        public async Task Login_StateUnderTest_ExpectedBehavior1()
//        {
//            // Arrange
//            var unitUnderTest = CreateAccountController();
//            
//            var model =_fixture.Build<LoginViewModel>().Create();
//            string returnUrl = "returnUrl";
//            unitUnderTest.ModelState.AddModelError( "whoops", "test error" );
//            _mockSession.Setup( m => m.UserIdForStatuses ).Verifiable();
//            _mockSession.Setup( m => m.LastRetrievedWarriors ).Verifiable();
//            _mockSession.Setup( m => m.Warriors ).Verifiable();

//            // Act
//            var result = await unitUnderTest.Login(
//                model,
//                returnUrl );
//            var viewResult = result as ViewResult;

//            // Assert
//            Assert.IsNotNull( viewResult );
//            Assert.AreSame( model, viewResult.Model );
//        }

//        //[Test]
//        //[TestCase( true )]
//        //[TestCase( false )]
//        //public async Task VerifyCode_StateUnderTest_ExpectedBehavior( Boolean rememberMe )
//        //{
//        //	
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();
//        //	string provider = "prov";
//        //	string returnUrl = "returnUrl";
//        //	var authResult = new AuthenticateResult( new ClaimsIdentity(), new AuthenticationProperties(), new AuthenticationDescription() );
//        //	this.mockAuthenticationManager.Setup( m => m.AuthenticateAsync( "TwoFactorCookie" ) ).Returns( Task.FromResult( authResult ) );

//        //	// Act
//        //	var result = await unitUnderTest.VerifyCode(
//        //		provider,
//        //		returnUrl,
//        //		rememberMe );

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public async Task VerifyCode_StateUnderTest_ExpectedBehavior1()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();
//        //	VerifyCodeViewModel model = TODO;

//        //	// Act
//        //	var result = await unitUnderTest.VerifyCode(
//        //		model );

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public void Register_StateUnderTest_ExpectedBehavior()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();

//        //	// Act
//        //	var result = unitUnderTest.Register();

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public async Task Register_StateUnderTest_ExpectedBehavior1()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();
//        //	RegisterViewModel model = TODO;

//        //	// Act
//        //	var result = await unitUnderTest.Register(
//        //		model );

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public async Task ConfirmEmail_StateUnderTest_ExpectedBehavior()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();
//        //	string userId = TODO;
//        //	string code = TODO;

//        //	// Act
//        //	var result = await unitUnderTest.ConfirmEmail(
//        //		userId,
//        //		code );

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public void ForgotPassword_StateUnderTest_ExpectedBehavior()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();

//        //	// Act
//        //	var result = unitUnderTest.ForgotPassword();

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public async Task ForgotPassword_StateUnderTest_ExpectedBehavior1()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();
//        //	ForgotPasswordViewModel model = TODO;

//        //	// Act
//        //	var result = await unitUnderTest.ForgotPassword(
//        //		model );

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public void ForgotPasswordConfirmation_StateUnderTest_ExpectedBehavior()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();

//        //	// Act
//        //	var result = unitUnderTest.ForgotPasswordConfirmation();

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public void ForgotUserName_StateUnderTest_ExpectedBehavior()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();

//        //	// Act
//        //	var result = unitUnderTest.ForgotUserName();

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public async Task ForgotUserName_StateUnderTest_ExpectedBehavior1()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();
//        //	ForgotPasswordViewModel model = TODO;

//        //	// Act
//        //	var result = await unitUnderTest.ForgotUserName(
//        //		model );

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public void ForgotUserNameConfirmation_StateUnderTest_ExpectedBehavior()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();

//        //	// Act
//        //	var result = unitUnderTest.ForgotUserNameConfirmation();

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public void ResetPassword_StateUnderTest_ExpectedBehavior()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();
//        //	string code = TODO;

//        //	// Act
//        //	var result = unitUnderTest.ResetPassword(
//        //		code );

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public async Task ResetPassword_StateUnderTest_ExpectedBehavior1()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();
//        //	ResetPasswordViewModel model = TODO;

//        //	// Act
//        //	var result = await unitUnderTest.ResetPassword(
//        //		model );

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public void ResetPasswordConfirmation_StateUnderTest_ExpectedBehavior()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();

//        //	// Act
//        //	var result = unitUnderTest.ResetPasswordConfirmation();

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public void ExternalLogin_StateUnderTest_ExpectedBehavior()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();
//        //	string provider = TODO;
//        //	string returnUrl = TODO;

//        //	// Act
//        //	var result = unitUnderTest.ExternalLogin(
//        //		provider,
//        //		returnUrl );

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public async Task SendCode_StateUnderTest_ExpectedBehavior()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();
//        //	string returnUrl = TODO;
//        //	bool rememberMe = TODO;

//        //	// Act
//        //	var result = await unitUnderTest.SendCode(
//        //		returnUrl,
//        //		rememberMe );

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public async Task SendCode_StateUnderTest_ExpectedBehavior1()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();
//        //	SendCodeViewModel model = TODO;

//        //	// Act
//        //	var result = await unitUnderTest.SendCode(
//        //		model );

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public async Task ExternalLoginCallback_StateUnderTest_ExpectedBehavior()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();
//        //	string returnUrl = TODO;

//        //	// Act
//        //	var result = await unitUnderTest.ExternalLoginCallback(
//        //		returnUrl );

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public async Task ExternalLoginConfirmation_StateUnderTest_ExpectedBehavior()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();
//        //	ExternalLoginConfirmationViewModel model = TODO;
//        //	string returnUrl = TODO;

//        //	// Act
//        //	var result = await unitUnderTest.ExternalLoginConfirmation(
//        //		model,
//        //		returnUrl );

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public void LogOff_StateUnderTest_ExpectedBehavior()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();

//        //	// Act
//        //	var result = unitUnderTest.LogOff();

//        //	// Assert
//        //	Assert.Fail();
//        //}

//        //[Test]
//        //public void ExternalLoginFailure_StateUnderTest_ExpectedBehavior()
//        //{
//        //	// Arrange
//        //	var unitUnderTest = CreateAccountController();

//        //	// Act
//        //	var result = unitUnderTest.ExternalLoginFailure();

//        //	// Assert
//        //	Assert.Fail();
//        //}
//    }
//}
