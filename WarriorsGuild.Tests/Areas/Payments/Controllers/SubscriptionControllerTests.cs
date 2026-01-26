using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using WarriorsGuild.Areas.Payments;
using WarriorsGuild.Areas.Payments.Controllers;
using WarriorsGuild.Areas.Payments.Mappers;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Email;
using WarriorsGuild.Models.Payments;
using WarriorsGuild.Providers;
using WarriorsGuild.Providers.Payments;
using static WarriorsGuild.Tests.TestHelpers;

namespace WarriorsGuild.Tests.Areas.Payments.Controllers
{
    [TestFixture]
    public class SubscriptionControllerTests
    {
        protected Fixture _fixture = new Fixture();
        private Guid USER_ID = Guid.NewGuid();
        private MockRepository mockRepository;
        private Mock<IQueryableUserStore<ApplicationUser>> mockUserStore;
        private FakeUserManager mockApplicationUserManager;
        private Mock<IPriceOptionManager> mockPriceOptionManager;
        private Mock<ISubscriptionManager> mockSubscriptionManager;
        private Mock<ISubscriptionMapper> mockSubscriptionMapper;
        private Mock<IEmailProvider> mockEmailProvider;
        private Mock<ClaimsIdentity> mockIdentity;
        private Mock<IPrincipal> mockPrincipal;
        private Mock<HttpContext> _mockContext;
        private ClaimsPrincipal _identity;
        private Mock<IUserProvider> _mockUserProvider;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository( MockBehavior.Strict );

            this.mockUserStore = this.mockRepository.Create<IQueryableUserStore<ApplicationUser>>();
            this.mockApplicationUserManager = new FakeUserManager( mockUserStore.Object );
            this.mockPriceOptionManager = this.mockRepository.Create<IPriceOptionManager>();
            this.mockSubscriptionManager = this.mockRepository.Create<ISubscriptionManager>();
            this.mockSubscriptionMapper = this.mockRepository.Create<ISubscriptionMapper>();
            this.mockEmailProvider = this.mockRepository.Create<IEmailProvider>();
            this._mockUserProvider = this.mockRepository.Create<IUserProvider>();
        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        private SubscriptionController CreateSubscriptionController()
        {
            return new SubscriptionController(
                this.mockApplicationUserManager,
                this.mockPriceOptionManager.Object,
                this.mockSubscriptionManager.Object,
                this.mockSubscriptionMapper.Object,
                this.mockEmailProvider.Object, _mockUserProvider.Object );
        }

        private SubscriptionController AddClaimToContoller( SubscriptionController controller )
        {
            _mockContext = new Mock<HttpContext>( MockBehavior.Strict );
            _identity = new ClaimsPrincipal( new ClaimsIdentity( new Claim[] {
                                        new Claim(type: "sub", USER_ID.ToString()),
                                        new Claim(ClaimTypes.Name, "gunnar@somecompany.com")
                                        // other required and custom claims
                                   }, "TestAuthentication" ) );
            _mockContext.Setup( m => m.User ).Returns( _identity );
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = _mockContext.Object
            };
            _mockUserProvider.Setup( m => m.GetMyUserId( _identity ) ).Returns( USER_ID );
            return controller;
        }

        private SubscriptionController AddRoles( SubscriptionController controller )
        {
            mockPrincipal.Setup( p => p.IsInRole( "Warrior" ) ).Returns( true );
            mockPrincipal.Setup( p => p.IsInRole( "Guardian" ) ).Returns( true );
            return controller;
        }

        [Test]
        public async Task GetBillingAgreement_When_BillingAgreement_not_found_Returns_NotFoundResult()
        {
            // Arrange
            var unitUnderTest = CreateSubscriptionController();
            var id = Guid.NewGuid();
            mockSubscriptionManager.Setup( m => m.GetBillingAgreement( id ) ).Returns( Task.FromResult( default( BillingAgreement ) ) );

            // Act
            var result = await unitUnderTest.GetBillingAgreement( id );

            // Assert
            Assert.IsInstanceOf<NotFoundResult>( result.Result );
        }

        [Test]
        public async Task GetBillingAgreement_When_BillingAgreement_found_Returns_OkContent_Result()
        {
            // Arrange
            var unitUnderTest = CreateSubscriptionController();
            var id = Guid.NewGuid();
            
            var agreement =_fixture.Build<BillingAgreement>().Create();
            mockSubscriptionManager.Setup( m => m.GetBillingAgreement( id ) ).Returns( Task.FromResult( agreement ) );

            // Act
            var result = await unitUnderTest.GetBillingAgreement( id );
            var contentResult = result.Result as OkObjectResult;

            // Assert
            Assert.AreSame( agreement, contentResult.Value );
        }

        [Test]
        public async Task GetMyBillingAgreement_When_no_subscription_found_A_log_email_is_sent_and_returns_NotFoundResult()
        {
            // Arrange
            var unitUnderTest = AddClaimToContoller( CreateSubscriptionController() );
            mockSubscriptionManager.Setup( m => m.GetMySubscriptionAsync( USER_ID.ToString() ) ).Returns( Task.FromResult( default( MySubscription ) ) );
            //mockApplicationUserManager.Setup( m => m.Users ).Returns( users );

            // Act
            var result = await unitUnderTest.GetMyBillingAgreement();

            // Assert
            Assert.IsInstanceOf<NotFoundResult>( result.Result );
            //mockEmailProvider.Verify( m => m.SendAsync( "No subscription found", String.Empty, EmailProvider.EmailView.Generic ) );
        }

        [Test]
        public async Task GetMyBillingAgreement_When_subscription_is_found_The_created_viewModel_is_returned()
        {
            // Arrange
            var unitUnderTest = AddClaimToContoller( CreateSubscriptionController() );
            
            var subscription =_fixture.Build<MySubscription>().Create();
            subscription.BillingAgreement.Cancelled = null;
            var userWithMyId =_fixture.Build<ApplicationUser>().With( u => u.ChildUsers, new List<ApplicationUser>() ).Create();
            userWithMyId.Id = USER_ID.ToString();
            IQueryable<ApplicationUser> users = new List<ApplicationUser>()
            {
               _fixture.Build<ApplicationUser>().With( u => u.Id, Guid.NewGuid().ToString() ).With( u => u.ChildUsers, new List<ApplicationUser>() ).Create(),
                userWithMyId,
               _fixture.Build<ApplicationUser>().With( u => u.Id, Guid.NewGuid().ToString() ).With( u => u.ChildUsers, new List<ApplicationUser>() ).Create()
            }.AsQueryable();
            var userSubscriptions = new List<UserSubscription>()
            {
               _fixture.Build<UserSubscription>().With( u => u.UserId, new Guid(users.First().Id) ).Create(),
               _fixture.Build<UserSubscription>().With( u => u.UserId, new Guid(users.Skip(1).First().Id) ).Create(),
               _fixture.Build<UserSubscription>().With( u => u.UserId, new Guid(users.Skip(2).First().Id) ).Create()
            };
            var expectedUsers = new List<SubscriptionUser>();
            var vm =_fixture.Build<BillingAgreementViewModel>().Create();
            //mockApplicationUserManager.Setup( m => m.Users ).Returns( users );
            mockSubscriptionManager.Setup( m => m.GetMySubscriptionAsync( USER_ID.ToString() ) ).Returns( Task.FromResult( subscription ) );
            mockSubscriptionManager.Setup( m => m.GetUsersOnSubscriptionAsync( subscription.BillingAgreement.Id ) ).Returns( Task.FromResult( userSubscriptions.AsEnumerable() ) );
            var i = 0;
            foreach ( var us in userSubscriptions )
            {
                var isGuardian = i % 2 == 0;
                var isWarrior = i % 2 != 0;
                var su = new SubscriptionUser()
                {
                    FirstName = $"FN{us.SubscriptionId.ToString()}",
                    LastName = $"LN{us.SubscriptionId.ToString()}",
                    Id = $"ID{us.SubscriptionId.ToString()}"
                };
                expectedUsers.Add( su );
                //mockApplicationUserManager
                //_mockContext.Setup( hc => hc.User.IsInRole( "Guardian" ) ).Returns( isGuardian );
                //_mockContext.Setup( hc => hc.User.IsInRole( "Warrior" ) ).Returns( isWarrior );
                var relatedUser = users.Single( u => u.Id == us.UserId.ToString() );
                mockSubscriptionMapper.Setup( m => m.MapToSubscriptionUser( relatedUser, us, isGuardian, isWarrior ) ).Returns( su );
                i++;
            }
            mockSubscriptionMapper.Setup( m => m.CreateViewModel( subscription, It.Is<IEnumerable<SubscriptionUser>>( uss => uss.SequenceEqual( expectedUsers ) ) ) ).Returns( vm );

            // Act
            var result = await unitUnderTest.GetMyBillingAgreement();
            var contentResult = result.Result as OkObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>( contentResult );
            Assert.AreSame( vm, contentResult.Value );
        }

        [Test]
        public async Task PostBillingAgreement_When_ModelState_IsValid_and_user_is_the_paying_party_on_a_subscription()
        {
            // Arrange
            var unitUnderTest = this.AddClaimToContoller( CreateSubscriptionController() );
            
            var request =_fixture.Build<BillingViewModel>().Create();
            var mySub =_fixture.Build<MySubscription>().Create();
            mySub.UserSubscription.IsPayingParty = true;
            mockSubscriptionManager.Setup( m => m.GetMySubscriptionAsync( USER_ID.ToString() ) ).Returns( Task.FromResult( mySub ) );

            var newSub =_fixture.Build<MySubscription>().Create();

            var user =_fixture.Build<ApplicationUser>().With( u => u.Id, USER_ID.ToString() ).With( u => u.ChildUsers, new List<ApplicationUser>() ).Create();
            //mockApplicationUserManager.Setup( m => m.FindByIdAsync( USER_ID ) ).Returns( Task.FromResult( user ) );
            var userSubscriptions =_fixture.Build<UserSubscription>().CreateMany();
            var expectedUsers = new List<SubscriptionUser>();
            mockSubscriptionManager.Setup( m => m.CreateSubscription( request, user ) ).Returns( Task.FromResult( newSub ) );
            mockSubscriptionManager.Setup( m => m.GetUsersOnSubscriptionAsync( newSub.BillingAgreement.Id ) ).Returns( Task.FromResult( userSubscriptions ) );
            var i = 0;
            foreach ( var us in userSubscriptions )
            {
                var isGuardian = i % 2 == 0;
                var isWarrior = i % 2 != 0;
                var su = new SubscriptionUser()
                {
                    FirstName = $"FN{us.SubscriptionId.ToString()}",
                    LastName = $"LN{us.SubscriptionId.ToString()}",
                    Id = $"ID{us.SubscriptionId.ToString()}"
                };
                expectedUsers.Add( su );
                _mockContext.Setup( hc => hc.User.IsInRole( "Guardian" ) ).Returns( isGuardian );
                _mockContext.Setup( hc => hc.User.IsInRole( "Warrior" ) ).Returns( isWarrior );
                mockSubscriptionMapper.Setup( m => m.MapToSubscriptionUser( user, us, isGuardian, isWarrior ) ).Returns( su );
                i++;
            }
            var vm =_fixture.Build<BillingAgreementViewModel>().Create();
            mockSubscriptionMapper.Setup( m => m.CreateViewModel( newSub, It.Is<IEnumerable<SubscriptionUser>>( uss => uss.SequenceEqual( expectedUsers ) ) ) ).Returns( vm );

            // Act
            var result = await unitUnderTest.PostBillingAgreement( request );

            // Assert

            var contentResult = result.Result as OkObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>( contentResult );
            Assert.AreSame( vm, contentResult.Value );
            mockSubscriptionManager.Verify( m => m.CreateSubscription( request, user ), Times.Once() );
            mockSubscriptionManager.Verify( m => m.CreateSubscription( It.IsAny<BillingViewModel>(), It.IsAny<ApplicationUser>() ), Times.Once() );
        }

        [Test]
        public async Task PostBillingAgreement_When_ModelState_IsValid_and_user_has_no_subscription()
        {
            // Arrange
            var unitUnderTest = this.AddClaimToContoller( CreateSubscriptionController() );
            
            var request =_fixture.Build<BillingViewModel>().Create();
            mockSubscriptionManager.Setup( m => m.GetMySubscriptionAsync( USER_ID.ToString() ) ).Returns( Task.FromResult( (MySubscription)null ) );

            var newSub =_fixture.Build<MySubscription>().Create();

            var user =_fixture.Build<ApplicationUser>().With( u => u.Id, USER_ID.ToString() ).With( u => u.ChildUsers, new List<ApplicationUser>() ).Create();

            //mockApplicationUserManager.Setup( m => m.FindByIdAsync( USER_ID ) ).Returns( Task.FromResult( user ) );
            var userSubscriptions =_fixture.Build<UserSubscription>().CreateMany();
            var expectedUsers = new List<SubscriptionUser>();
            mockSubscriptionManager.Setup( m => m.CreateSubscription( request, user ) ).Returns( Task.FromResult( newSub ) );
            mockSubscriptionManager.Setup( m => m.GetUsersOnSubscriptionAsync( newSub.BillingAgreement.Id ) ).Returns( Task.FromResult( userSubscriptions ) );
            var i = 0;
            foreach ( var us in userSubscriptions )
            {
                var isGuardian = i % 2 == 0;
                var isWarrior = i % 2 != 0;
                var su = new SubscriptionUser()
                {
                    FirstName = $"FN{us.SubscriptionId.ToString()}",
                    LastName = $"LN{us.SubscriptionId.ToString()}",
                    Id = $"ID{us.SubscriptionId.ToString()}"
                };
                expectedUsers.Add( su );
                //(_identity.Identity as ClaimsIdentity).Claims.Add("Roles".IsInRole( "Guardian" ) ).Returns( isGuardian );
                _mockContext.Setup( hc => hc.User.IsInRole( "Warrior" ) ).Returns( isWarrior );
                mockSubscriptionMapper.Setup( m => m.MapToSubscriptionUser( user, us, isGuardian, isWarrior ) ).Returns( su );
                i++;
            }
            var vm =_fixture.Build<BillingAgreementViewModel>().Create();
            mockSubscriptionMapper.Setup( m => m.CreateViewModel( newSub, It.Is<IEnumerable<SubscriptionUser>>( uss => uss.SequenceEqual( expectedUsers ) ) ) ).Returns( vm );

            // Act
            var result = await unitUnderTest.PostBillingAgreement( request );

            // Assert

            var contentResult = result.Result as OkObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>( contentResult );
            Assert.AreSame( vm, contentResult.Value );
            mockSubscriptionManager.Verify( m => m.CreateSubscription( request, user ), Times.Once() );
            mockSubscriptionManager.Verify( m => m.CreateSubscription( It.IsAny<BillingViewModel>(), It.IsAny<ApplicationUser>() ), Times.Once() );
        }

        [Test]
        public async Task PostBillingAgreement_When_ModelState_Is_not_Valid()
        {
            // Arrange
            var unitUnderTest = CreateSubscriptionController();
            unitUnderTest.ModelState.AddModelError( "BasePlanId", "Something didn't pass" );
            
            var request =_fixture.Build<BillingViewModel>().Create();

            // Act
            var result = await unitUnderTest.PostBillingAgreement( request );

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>( result.Result );
        }

        [Test]
        public async Task PostBillingAgreement_When_ModelState_IsValid_but_user_is_not_the_paying_party()
        {
            // Arrange
            var unitUnderTest = this.AddClaimToContoller( CreateSubscriptionController() );
            
            var request =_fixture.Build<BillingViewModel>().Create();
            var mySub =_fixture.Build<MySubscription>().Create();
            mySub.UserSubscription.IsPayingParty = false;

            var userSubscriptions =_fixture.Build<UserSubscription>().CreateMany();
            mockSubscriptionManager.Setup( m => m.GetMySubscriptionAsync( USER_ID.ToString() ) ).Returns( Task.FromResult( mySub ) );

            // Act
            var result = await unitUnderTest.PostBillingAgreement( request );
            var contentResult = result.Result as BadRequestObjectResult;

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>( result.Result );
            Assert.AreEqual( "Only the paying party or an unsubscribed user can modify a subscription.", contentResult.Value as string );
        }

        [Test]
        public async Task DeleteBillingAgreement_BillingAgreement_does_not_exist_Returns_NotFound()
        {
            // Arrange
            var unitUnderTest = this.AddClaimToContoller( CreateSubscriptionController() );
            var id = Guid.NewGuid();
            mockSubscriptionManager.Setup( m => m.GetBillingAgreement( id ) ).Returns( Task.FromResult( default( BillingAgreement ) ) );

            // Act
            var result = await unitUnderTest.DeleteBillingAgreement( id );

            // Assert
            Assert.IsInstanceOf<NotFoundResult>( result.Result );
        }
    }
}
