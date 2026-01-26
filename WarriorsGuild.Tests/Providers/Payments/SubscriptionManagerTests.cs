using AutoFixture;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarriorsGuild.Areas.Payments;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Models.Payments;
using WarriorsGuild.Providers.Payments;
using WarriorsGuild.Providers.Payments.Mappers;

namespace WarriorsGuild.Tests.Providers.Payments
{
    [TestFixture]
    public class SubscriptionManagerTests
    {
        protected Fixture _fixture = new Fixture();
        private Guid USER_ID = Guid.NewGuid();
        private MockRepository mockRepository;

        private Mock<ISubscriptionRepository> mockSubscriptionRepo;
        private Mock<IPriceOptionManager> mockPriceOptionManager;
        private Mock<IStripeSubscriptionProvider> mockStripeSubscriptionProvider;
        private Mock<IDatabaseObjectMapper> mockDatabaseObjectMapper;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository( MockBehavior.Strict );

            this.mockSubscriptionRepo = this.mockRepository.Create<ISubscriptionRepository>();
            this.mockPriceOptionManager = this.mockRepository.Create<IPriceOptionManager>();
            this.mockStripeSubscriptionProvider = this.mockRepository.Create<IStripeSubscriptionProvider>();
            this.mockDatabaseObjectMapper = this.mockRepository.Create<IDatabaseObjectMapper>();
        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        private SubscriptionManager CreateManager()
        {
            return new SubscriptionManager(
                this.mockSubscriptionRepo.Object,
                this.mockPriceOptionManager.Object,
                this.mockStripeSubscriptionProvider.Object,
                this.mockDatabaseObjectMapper.Object );
        }

        [Test]
        public async Task GetMySubscription_When_the_object_from_the_repo_is_null_Returns_null()
        {
            
            // Arrange
            var unitUnderTest = CreateManager();
            var userId = USER_ID;

            MySubscription mySub = null;
            mockSubscriptionRepo.Setup( m => m.GetMySubscriptionAsync( userId.ToString() ) ).Returns( Task.FromResult( mySub ) );

            // Act
            var result = await unitUnderTest.GetMySubscriptionAsync(
                userId.ToString() );

            // Assert
            Assert.AreSame( mySub, result );
        }

        [Test]
        public async Task GetMySubscription_When_billing_agreement_is_not_cancelled_The_object_from_the_repo_is_returned()
        {
            
            // Arrange
            var unitUnderTest = CreateManager();
            var userId = USER_ID;

            var mySub =_fixture.Build<MySubscription>().Create();
            mySub.BillingAgreement.Cancelled = null;
            mockSubscriptionRepo.Setup( m => m.GetMySubscriptionAsync( userId.ToString() ) ).Returns( Task.FromResult( mySub ) );

            // Act
            var result = await unitUnderTest.GetMySubscriptionAsync(
                userId.ToString() );

            // Assert
            Assert.IsInstanceOf<MySubscription>( result );
            Assert.AreSame( mySub, result );
        }

        [Test]
        public async Task GetMySubscription_When_billing_agreement_is_cancelled_Null_is_returned()
        {
            
            // Arrange
            var unitUnderTest = CreateManager();
            var userId = USER_ID;

            var mySub =_fixture.Build<MySubscription>().Create();
            mockSubscriptionRepo.Setup( m => m.GetMySubscriptionAsync( userId.ToString() ) ).Returns( Task.FromResult( mySub ) );

            // Act
            var result = await unitUnderTest.GetMySubscriptionAsync(
                userId.ToString() );

            // Assert
            Assert.IsNull( result );
        }

        //[Test]
        //public void GetAllBillingAgreements_StateUnderTest_ExpectedBehavior()
        //{
        //	// Arrange
        //	var unitUnderTest = CreateManager();

        //	// Act
        //	var result = unitUnderTest.GetAllBillingAgreements();

        //	// Assert
        //	Assert.Fail();
        //}

        //[Test]
        //public async Task GetBillingAgreement_StateUnderTest_ExpectedBehavior()
        //{
        //	
        //	// Arrange
        //	var unitUnderTest = CreateManager();
        //	var id = new Guid();
        //	var billingAgreements =_fixture.Build<BillingAgreement>().CreateMany();
        //	var billingAgreementSet = TestHelpers.CreateDbSetMock(billingAgreements);

        //	// Act
        //	var result = await unitUnderTest.GetBillingAgreement(
        //		id );

        //	// Assert
        //	Assert.Fail();
        //}

        //[Test]
        //public void GetUsersOnSubscription_StateUnderTest_ExpectedBehavior()
        //{
        //	// Arrange
        //	var unitUnderTest = CreateManager();
        //	Guid billingAgreementId = TODO;

        //	// Act
        //	var result = unitUnderTest.GetUsersOnSubscription(
        //		billingAgreementId );

        //	// Assert
        //	Assert.Fail();
        //}

        [Test]
        public async Task CreateSubscription_Given_the_user_has_an_active_subscription()
        {
            
            // Arrange
            var unitUnderTest = CreateManager();
            var request =_fixture.Build<BillingViewModel>().With( p => p.BasePlanId, Guid.NewGuid().ToString() ).Create();
            String userId = USER_ID.ToString();
            var childUsers =_fixture.Build<ApplicationUser>().With( u => u.ChildUsers, (List<ApplicationUser>)null ).CreateMany( 5 ).ToArray();
            var user =_fixture.Build<ApplicationUser>().With( u => u.Id, userId ).With( u => u.ChildUsers, childUsers ).Create();
            var subscriptionId = "asdfasfssadfsdf";

            var po =_fixture.Build<PriceOption>().With( p => p.StripePlanId, Guid.NewGuid().ToString() )
                                                            .With( p => p.AdditionalGuardianPlan,_fixture.Build<AddOnPriceOption>().With( p => p.StripePlanId, Guid.NewGuid().ToString() ).Create() )
                                                            .With( p => p.AdditionalWarriorPlan,_fixture.Build<AddOnPriceOption>().With( p => p.StripePlanId, Guid.NewGuid().ToString() ).Create() ).Create();
            mockPriceOptionManager.Setup( m => m.GetPriceOption( It.Is<Guid>( g => g.ToString() == request.BasePlanId ) ) ).Returns( Task.FromResult( po ) );

            var pricePlanListDefinition = new Dictionary<string, int>();
            mockDatabaseObjectMapper.Setup( m => m.BuildPlanList( po.StripePlanId,
                                                                    po.AdditionalGuardianPlan.StripePlanId,
                                                                    po.AdditionalWarriorPlan.StripePlanId,
                                                                    request.NumberOfAdditionalGuardians,
                                                                    request.NumberOfAdditionalWarriors ) ).Returns( pricePlanListDefinition );
            mockStripeSubscriptionProvider.Setup( m => m.Create( user, request.StripePaymentToken, pricePlanListDefinition, po.SetupFee ) ).Returns( Task.FromResult( subscriptionId ) );


            var ba =_fixture.Build<BillingAgreement>().Create();
            mockDatabaseObjectMapper.Setup( m => m.CreateBillingAgreement( po.Frequency, po, request.NumberOfAdditionalGuardians, request.NumberOfAdditionalWarriors, PaymentMethod.Stripe, subscriptionId ) ).Returns( ba );

            var mySub =_fixture.Build<MySubscription>().With( m => m.BillingAgreement,_fixture.Build<BillingAgreement>().With( b => b.Cancelled, (DateTime?)null ).Create() ).Create();
            mockSubscriptionRepo.Setup( m => m.GetMySubscriptionAsync( userId ) ).Returns( Task.FromResult( mySub ) );
            mockStripeSubscriptionProvider.Setup( m => m.Unsubscribe( mySub.BillingAgreement.StripeSubscriptionId ) ).Returns( Task.CompletedTask );
            mockSubscriptionRepo.Setup( m => m.Unsubscribe( mySub.BillingAgreement, userId ) ).Returns( Task.CompletedTask );
            var associatedSubscriptions =_fixture.Build<UserSubscription>().With( s => s.Revised, (DateTime?)null ).CreateMany( 8 );
            mockSubscriptionRepo.Setup( m => m.GetUsersOnSubscriptionAsync( mySub.BillingAgreement.Id ) ).Returns( Task.FromResult( associatedSubscriptions ) );
            var newSubscriptions =_fixture.Build<UserSubscription>().CreateMany( 5 );
            newSubscriptions.Skip( 2 ).First().UserId = USER_ID;
            mockDatabaseObjectMapper.Setup( m => m.UpdateBillingAgreementId( ba.Id, associatedSubscriptions ) ).Returns( newSubscriptions );
            mockSubscriptionRepo.Setup( m => m.CreateSubscription( ba, newSubscriptions ) ).Returns( Task.CompletedTask );

            // Act
            var result = await unitUnderTest.CreateSubscription(
                request,
                user );

            // Asse
            mockStripeSubscriptionProvider.Verify( m => m.Unsubscribe( mySub.BillingAgreement.StripeSubscriptionId ), Times.Once() );
            Assert.AreSame( ba, result.BillingAgreement );
            Assert.AreSame( newSubscriptions.Skip( 2 ).First(), result.UserSubscription );
        }

        [Test]
        public async Task CreateSubscription_Given_the_user_has_a_canceled_subscription()
        {
            
            // Arrange
            var unitUnderTest = CreateManager();
            var request =_fixture.Build<BillingViewModel>().With( p => p.BasePlanId, Guid.NewGuid().ToString() ).Create();
            String userId = USER_ID.ToString();
            var childUsers =_fixture.Build<ApplicationUser>().With( u => u.ChildUsers, (List<ApplicationUser>)null ).CreateMany( 5 ).ToArray();
            var user =_fixture.Build<ApplicationUser>().With( u => u.Id, userId ).With( u => u.ChildUsers, childUsers ).Create();
            var subscriptionId = "asdfasfssadfsdf";

            var po =_fixture.Build<PriceOption>().With( p => p.StripePlanId, Guid.NewGuid().ToString() )
                                                            .With( p => p.AdditionalGuardianPlan,_fixture.Build<AddOnPriceOption>().With( p => p.StripePlanId, Guid.NewGuid().ToString() ).Create() )
                                                            .With( p => p.AdditionalWarriorPlan,_fixture.Build<AddOnPriceOption>().With( p => p.StripePlanId, Guid.NewGuid().ToString() ).Create() ).Create();
            mockPriceOptionManager.Setup( m => m.GetPriceOption( It.Is<Guid>( g => g.ToString() == request.BasePlanId ) ) ).Returns( Task.FromResult( po ) );

            var pricePlanListDefinition = new Dictionary<string, int>();
            mockDatabaseObjectMapper.Setup( m => m.BuildPlanList( po.StripePlanId,
                                                                    po.AdditionalGuardianPlan.StripePlanId,
                                                                    po.AdditionalWarriorPlan.StripePlanId,
                                                                    request.NumberOfAdditionalGuardians,
                                                                    request.NumberOfAdditionalWarriors ) ).Returns( pricePlanListDefinition );
            mockStripeSubscriptionProvider.Setup( m => m.Create( user, request.StripePaymentToken, pricePlanListDefinition, po.SetupFee ) ).Returns( Task.FromResult( subscriptionId ) );


            var ba =_fixture.Build<BillingAgreement>().Create();
            mockDatabaseObjectMapper.Setup( m => m.CreateBillingAgreement( po.Frequency, po, request.NumberOfAdditionalGuardians, request.NumberOfAdditionalWarriors, PaymentMethod.Stripe, subscriptionId ) ).Returns( ba );

            var mySub =_fixture.Build<MySubscription>().Create();
            mockSubscriptionRepo.Setup( m => m.GetMySubscriptionAsync( userId ) ).Returns( Task.FromResult( mySub ) );
            var associatedSubscriptions =_fixture.Build<UserSubscription>().With( s => s.Revised, (DateTime?)null ).CreateMany( 8 );
            mockSubscriptionRepo.Setup( m => m.GetUsersOnSubscriptionAsync( mySub.BillingAgreement.Id ) ).Returns( Task.FromResult( associatedSubscriptions ) );
            var newSubscriptions =_fixture.Build<UserSubscription>().CreateMany( 5 );
            newSubscriptions.Skip( 2 ).First().UserId = USER_ID;
            mockDatabaseObjectMapper.Setup( m => m.UpdateBillingAgreementId( ba.Id, associatedSubscriptions ) ).Returns( newSubscriptions );
            mockSubscriptionRepo.Setup( m => m.CreateSubscription( ba, newSubscriptions ) ).Returns( Task.CompletedTask );

            // Act
            var result = await unitUnderTest.CreateSubscription(
                request,
                user );

            // Asse
            mockStripeSubscriptionProvider.Verify( m => m.Unsubscribe( It.IsAny<String>() ), Times.Never() );
            Assert.AreSame( ba, result.BillingAgreement );
            Assert.AreSame( newSubscriptions.Skip( 2 ).First(), result.UserSubscription );
        }

        [Test]
        public async Task CreateSubscription_Given_the_user_has_no_past_subscription()
        {
            
            // Arrange
            var unitUnderTest = CreateManager();
            var request =_fixture.Build<BillingViewModel>().With( p => p.BasePlanId, Guid.NewGuid().ToString() ).Create();
            String userId = USER_ID.ToString();
            var childUsers =_fixture.Build<ApplicationUser>().With( u => u.ChildUsers, (List<ApplicationUser>)null ).CreateMany( 5 ).ToArray();
            foreach ( var cu in childUsers )
            {
                cu.Id = Guid.NewGuid().ToString();
            }
            var user =_fixture.Build<ApplicationUser>().With( u => u.Id, userId ).With( u => u.ChildUsers, childUsers ).Create();
            var subscriptionId = "asdfasfssadfsdf";

            var po =_fixture.Build<PriceOption>().With( p => p.StripePlanId, Guid.NewGuid().ToString() )
                                                            .With( p => p.AdditionalGuardianPlan,_fixture.Build<AddOnPriceOption>().With( p => p.StripePlanId, Guid.NewGuid().ToString() ).Create() )
                                                            .With( p => p.AdditionalWarriorPlan,_fixture.Build<AddOnPriceOption>().With( p => p.StripePlanId, Guid.NewGuid().ToString() ).Create() ).Create();
            mockPriceOptionManager.Setup( m => m.GetPriceOption( It.Is<Guid>( g => g.ToString() == request.BasePlanId ) ) ).Returns( Task.FromResult( po ) );

            var pricePlanListDefinition = new Dictionary<string, int>();
            mockDatabaseObjectMapper.Setup( m => m.BuildPlanList( po.StripePlanId,
                                                                    po.AdditionalGuardianPlan.StripePlanId,
                                                                    po.AdditionalWarriorPlan.StripePlanId,
                                                                    request.NumberOfAdditionalGuardians,
                                                                    request.NumberOfAdditionalWarriors ) ).Returns( pricePlanListDefinition );
            mockStripeSubscriptionProvider.Setup( m => m.Create( user, request.StripePaymentToken, pricePlanListDefinition, po.SetupFee ) ).Returns( Task.FromResult( subscriptionId ) );


            var ba =_fixture.Build<BillingAgreement>().Create();
            mockDatabaseObjectMapper.Setup( m => m.CreateBillingAgreement( po.Frequency, po, request.NumberOfAdditionalGuardians, request.NumberOfAdditionalWarriors, PaymentMethod.Stripe, subscriptionId ) ).Returns( ba );

            MySubscription mySub = null;
            mockSubscriptionRepo.Setup( m => m.GetMySubscriptionAsync( userId ) ).Returns( Task.FromResult( mySub ) );

            var myNewSub =_fixture.Build<UserSubscription>().With( us => us.UserId, USER_ID ).Create();
            mockDatabaseObjectMapper.Setup( m => m.CreateUserSubscription( USER_ID.ToString(), ba.Id, true, UserRole.Guardian ) ).Returns( myNewSub );
            var expectedSubs = new List<UserSubscription>()
            {
                myNewSub
            };
            foreach ( var u in childUsers )
            {
                var uSub =_fixture.Build<UserSubscription>().With( us => us.UserId, new Guid( u.Id ) ).Create();
                mockDatabaseObjectMapper.Setup( m => m.CreateUserSubscription( u.Id, ba.Id, false, UserRole.Warrior ) ).Returns( uSub );
                expectedSubs.Add( uSub );
            }
            mockSubscriptionRepo.Setup( m => m.CreateSubscription( ba, It.Is<IEnumerable<UserSubscription>>( us => us.SequenceEqual( expectedSubs ) ) ) ).Returns( Task.CompletedTask );

            // Act
            var result = await unitUnderTest.CreateSubscription(
                request,
                user );

            // Asse
            mockStripeSubscriptionProvider.Verify( m => m.Unsubscribe( It.IsAny<String>() ), Times.Never() );
            mockSubscriptionRepo.Verify( m => m.GetUsersOnSubscriptionAsync( It.IsAny<Guid>() ), Times.Never() );
            mockDatabaseObjectMapper.Verify( m => m.UpdateBillingAgreementId( It.IsAny<Guid>(), It.IsAny<IEnumerable<UserSubscription>>() ), Times.Never() );
            mockSubscriptionRepo.Verify( m => m.CreateSubscription( ba, It.Is<IEnumerable<UserSubscription>>( us => us.SequenceEqual( expectedSubs ) ) ), Times.Once() );
            Assert.AreSame( ba, result.BillingAgreement );
            Assert.AreSame( myNewSub, result.UserSubscription );
        }

        //[Test]
        //public async Task CreateSubscription_Given_the_user_has_no_active_subscription()
        //{
        //	
        //	// Arrange
        //	var unitUnderTest = CreateManager();
        //	var request =_fixture.Build<BillingViewModel>().With( p => p.BasePlanId, Guid.NewGuid().ToString() ).Create();
        //	String userId = USER_ID.ToString();
        //	var subscriptionId = "asdfasfssadfsdf";

        //	var po =_fixture.Build<PriceOption>().With( p => p.StripePlanId, Guid.NewGuid().ToString() )
        //													.With( p => p.AdditionalGuardianPlan,_fixture.Build<AddOnPriceOption>().With( p => p.StripePlanId, Guid.NewGuid().ToString() ).Create() )
        //													.With( p => p.AdditionalWarriorPlan,_fixture.Build<AddOnPriceOption>().With( p => p.StripePlanId, Guid.NewGuid().ToString() ).Create() ).Create();
        //	mockPriceOptionManager.Setup( m => m.GetPriceOption( It.Is<Guid>( g => g.ToString() == request.BasePlanId ) ) ).Returns( Task.FromResult( po ) );

        //	var pricePlanListDefinition = new Dictionary<string, int>();
        //	mockDatabaseObjectMapper.Setup( m => m.BuildPlanList( po.StripePlanId,
        //															po.AdditionalGuardianPlan.StripePlanId,
        //															po.AdditionalWarriorPlan.StripePlanId,
        //															request.NumberOfAdditionalGuardians,
        //															request.NumberOfAdditionalWarriors ) ).Returns( pricePlanListDefinition );
        //	mockStripeSubscriptionProvider.Setup( m => m.Create( userId, request.StripePaymentToken, pricePlanListDefinition, po.SetupFee ) ).Returns( Task.FromResult( subscriptionId ) );


        //	var ba =_fixture.Build<BillingAgreement>().Create();
        //	mockDatabaseObjectMapper.Setup( m => m.CreateBillingAgreement( po.Frequency, po, request.NumberOfAdditionalGuardians, request.NumberOfAdditionalWarriors, PaymentMethod.Stripe, subscriptionId ) ).Returns( ba );

        //	MySubscription mySub = null;
        //	mockSubscriptionRepo.Setup( m => m.GetMySubscription( userId ) ).Returns( mySub );
        //	var associatedSubscriptions =_fixture.Build<UserSubscription>().With( s => s.Revised, (DateTime?)null ).CreateMany( 8 );
        //	mockSubscriptionRepo.Setup( m => m.GetUsersOnSubscription( mySub.BillingAgreement.Id ) ).Returns( associatedSubscriptions );
        //	var newSubscriptions =_fixture.Build<UserSubscription>().CreateMany( 5 );
        //	newSubscriptions.Skip( 2 ).First().UserId = USER_ID;
        //	mockDatabaseObjectMapper.Setup( m => m.UpdateBillingAgreementId( ba.Id, associatedSubscriptions ) ).Returns( newSubscriptions );
        //	mockSubscriptionRepo.Setup( m => m.CreateSubscription( ba, newSubscriptions ) ).Returns( Task.CompletedTask );

        //	// Act
        //	var result = await unitUnderTest.CreateSubscription(
        //		request,
        //		userId );

        //	// Asse
        //	mockStripeSubscriptionProvider.Verify( m => m.Unsubscribe( mySub.BillingAgreement.StripeSubscriptionId ), Times.Once() );
        //	Assert.AreSame( ba, result.BillingAgreement );
        //	Assert.AreSame( newSubscriptions.Skip( 2 ).First(), result.UserSubscription );
        //}

        //[Test]
        //public async Task Unsubscribe_StateUnderTest_ExpectedBehavior()
        //{
        //	// Arrange
        //	var unitUnderTest = CreateManager();
        //	BillingAgreement billingAgreement = TODO;
        //	String userId = TODO;

        //	// Act
        //	await unitUnderTest.Unsubscribe(
        //		billingAgreement,
        //		userId );

        //	// Assert
        //	Assert.Fail();
        //}

        //[Test]
        //public void Dispose_StateUnderTest_ExpectedBehavior()
        //{
        //	// Arrange
        //	var unitUnderTest = CreateManager();

        //	// Act
        //	unitUnderTest.Dispose();

        //	// Assert
        //	Assert.Fail();
        //}
    }
}
