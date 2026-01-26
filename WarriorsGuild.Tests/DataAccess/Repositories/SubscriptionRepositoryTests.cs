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
using WarriorsGuild.DataAccess;
using WarriorsGuild.Models.Payments;
using static WarriorsGuild.Tests.TestHelpers;

namespace WarriorsGuild.Tests.DataAccess.Repositories
{
    [TestFixture]
    public class SubscriptionRepositoryTests
    {
        protected Fixture _fixture = new Fixture();
        private Guid USER_ID = Guid.NewGuid();
        private MockRepository mockRepository;

        private Mock<IGuildDbContext> mockGuildDbContext;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository( MockBehavior.Strict );

            this.mockGuildDbContext = this.mockRepository.Create<IGuildDbContext>();
        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        private SubscriptionRepository CreateSubscriptionRepository()
        {
            return new SubscriptionRepository(
                this.mockGuildDbContext.Object );
        }

        [Test]
        public void GetMySubscription_When_only_one_subscription_with_one_billing_agreement_is_found_The_result_should_be_the_compilation_of_the_two()
        {
            
            // Arrange
            var unitUnderTest = CreateSubscriptionRepository();
            var userId = USER_ID;
            var ms =_fixture.Build<UserSubscription>().Create();
            ms.UserId = userId;
            ms.Revised = null;
            var mySubscriptionsSet = TestHelpers.CreateDbSetMock( new[] { ms } );
            var ba =_fixture.Build<BillingAgreement>().Create();
            ba.Id = ms.BillingAgreementId;
            ba.Cancelled = null;
            var billingAgreementSet = TestHelpers.CreateDbSetMock( new[] { ba } );

            mockGuildDbContext.Setup( m => m.BillingAgreements ).Returns( billingAgreementSet.Object );
            mockGuildDbContext.Setup( m => m.UserSubscriptions ).Returns( mySubscriptionsSet.Object );

            // Act
            var result = unitUnderTest.GetMySubscription(
                userId.ToString() );

            // Assert
            Assert.IsInstanceOf<MySubscription>( result );
            Assert.AreSame( ba, result.BillingAgreement );
            Assert.AreSame( ms, result.UserSubscription );
        }

        [Test]
        public void GetMySubscription_When_only_one_unrevised_subscription_with_non_cancelled_billing_agreement_is_found_The_result_should_be_the_compilation_of_the_two()
        {
            
            // Arrange
            var unitUnderTest = CreateSubscriptionRepository();
            var userId = USER_ID;

            var billingAgreements =_fixture.Build<BillingAgreement>().CreateMany();
            billingAgreements.Last().Cancelled = null;
            var billingAgreementSet = TestHelpers.CreateDbSetMock( billingAgreements );
            var mySubscriptions =_fixture.Build<UserSubscription>().With( s => s.UserId, userId ).With( s => s.BillingAgreementId, billingAgreements.Last().Id ).CreateMany();
            mySubscriptions.Last().Revised = null;
            var mySubscriptionsSet = TestHelpers.CreateDbSetMock( mySubscriptions );

            mockGuildDbContext.Setup( m => m.BillingAgreements ).Returns( billingAgreementSet.Object );
            mockGuildDbContext.Setup( m => m.UserSubscriptions ).Returns( mySubscriptionsSet.Object );

            // Act
            var result = unitUnderTest.GetMySubscription(
                userId.ToString() );

            // Assert
            Assert.IsInstanceOf<MySubscription>( result );
            Assert.AreSame( billingAgreements.Last(), result.BillingAgreement );
            Assert.AreSame( mySubscriptions.Last(), result.UserSubscription );
        }

        [Test]
        public void GetMySubscription_When_unrevised_subscription_with_cancelled_billing_agreement_is_found_The_result_should_be_the_null()
        {
            
            // Arrange
            var unitUnderTest = CreateSubscriptionRepository();
            var userId = USER_ID;

            var billingAgreements =_fixture.Build<BillingAgreement>().CreateMany();
            var billingAgreementSet = TestHelpers.CreateDbSetMock( billingAgreements );
            var mySubscriptions =_fixture.Build<UserSubscription>().With( s => s.UserId, userId ).With( s => s.BillingAgreementId, billingAgreements.Last().Id ).CreateMany();
            mySubscriptions.Last().Revised = null;
            var mySubscriptionsSet = TestHelpers.CreateDbSetMock( mySubscriptions );

            mockGuildDbContext.Setup( m => m.BillingAgreements ).Returns( billingAgreementSet.Object );
            mockGuildDbContext.Setup( m => m.UserSubscriptions ).Returns( mySubscriptionsSet.Object );

            // Act
            var result = unitUnderTest.GetMySubscription(
                userId.ToString() );

            // Assert
            Assert.IsInstanceOf<MySubscription>( result );
            Assert.AreSame( billingAgreements.Last(), result.BillingAgreement );
            Assert.AreSame( mySubscriptions.Last(), result.UserSubscription );
        }

        [Test]
        public void GetMySubscription_When_revised_subscription_with_non_cancelled_billing_agreement_is_found_The_result_should_be_the_null()
        {
            
            // Arrange
            var unitUnderTest = CreateSubscriptionRepository();
            var userId = USER_ID;

            var billingAgreements =_fixture.Build<BillingAgreement>().CreateMany();
            billingAgreements.Last().Cancelled = null;
            var billingAgreementSet = TestHelpers.CreateDbSetMock( billingAgreements );
            var mySubscriptions =_fixture.Build<UserSubscription>().With( s => s.UserId, userId ).With( s => s.BillingAgreementId, billingAgreements.Last().Id ).CreateMany();
            var mySubscriptionsSet = TestHelpers.CreateDbSetMock( mySubscriptions );

            mockGuildDbContext.Setup( m => m.BillingAgreements ).Returns( billingAgreementSet.Object );
            mockGuildDbContext.Setup( m => m.UserSubscriptions ).Returns( mySubscriptionsSet.Object );

            // Act
            var result = unitUnderTest.GetMySubscription(
                userId.ToString() );

            // Assert
            Assert.IsNull( result );
        }

        //[Test]
        //public async Task GetBillingAgreement()
        //{
        //	// Arrange
        //	
        //	var unitUnderTest = this.CreateSubscriptionRepository();
        //	var id = Guid.NewGuid();
        //	var billingAgreements =_fixture.Build<BillingAgreement>().CreateMany();
        //	billingAgreements.Skip( 1 ).First().Id = id;
        //	var billingAgreementSet = TestHelpers.CreateDbSetMock( billingAgreements );
        //	mockGuildDbContext.Setup( m => m.BillingAgreements ).Returns( billingAgreementSet.Object );

        //	// Act
        //	var result = await unitUnderTest.GetBillingAgreement(
        //		id );

        //	// Assert
        //	Assert.AreSame( billingAgreements.Skip( 1 ).First(), result );
        //}

        [Test]
        public async Task GetUsersOnSubscription()
        {
            // Arrange
            
            var unitUnderTest = this.CreateSubscriptionRepository();
            var billingAgreementId = Guid.NewGuid();
            var mySubscriptions =_fixture.Build<UserSubscription>().With( s => s.BillingAgreementId, billingAgreementId ).With( s => s.Revised, (DateTime?)null ).CreateMany( 7 );
            var subToExclude = mySubscriptions.Skip( 2 ).First();
            subToExclude.BillingAgreementId = Guid.NewGuid();
            var subToExclude2 = mySubscriptions.Skip( 4 ).First();
            subToExclude2.Revised = DateTime.UtcNow;
            var mySubscriptionsSet = TestHelpers.CreateDbSetMock( mySubscriptions );
            mockGuildDbContext.Setup( m => m.UserSubscriptions ).Returns( mySubscriptionsSet.Object );

            var myUsers = new List<ApplicationUser>
            {
                new ApplicationUser() { Id = mySubscriptions.ElementAt(0).UserId.ToString()},
                new ApplicationUser() { Id = mySubscriptions.ElementAt(1).UserId.ToString()},
                new ApplicationUser() { Id = mySubscriptions.ElementAt(3).UserId.ToString()},
                new ApplicationUser() { Id = mySubscriptions.ElementAt(5).UserId.ToString()},
                new ApplicationUser() { Id = mySubscriptions.ElementAt(6).UserId.ToString()}
            };
            var myUsersSet = TestHelpers.CreateDbSetMock( myUsers.AsEnumerable() );
            mockGuildDbContext.Setup( m => m.Users ).Returns( myUsersSet.Object );

            // Act
            var result = await unitUnderTest.GetUsersOnSubscriptionAsync(
                billingAgreementId );

            // Assert
            Assert.True( mySubscriptions.Except( new[] { subToExclude, subToExclude2 } ).SequenceEqual( result.ToArray() ) );
        }

        [Test]
        public async Task CreateSubscription()
        {
            // Arrange
            
            var unitUnderTest = this.CreateSubscriptionRepository();
            var ba =_fixture.Build<BillingAgreement>().Create();
            var newSubscriptions =_fixture.Build<UserSubscription>().CreateMany();
            var billingAgreements =_fixture.Build<BillingAgreement>().With( b => b.Cancelled, (DateTime?)null ).CreateMany();
            var billingAgreementSet = TestHelpers.CreateDbSetMock( billingAgreements, false, true );
            mockGuildDbContext.Setup( m => m.BillingAgreements ).Returns( billingAgreementSet.Object );

            var mySubscriptionsSet = TestHelpers.CreateDbSetMock( newSubscriptions, false, true );
            mockGuildDbContext.Setup( m => m.UserSubscriptions ).Returns( mySubscriptionsSet.Object );

            var callOrder = 0;
            billingAgreementSet.Setup( m => m.Add( ba ) ).Callback( () => Assert.That( callOrder++, Is.EqualTo( 0 ) ) );
            mySubscriptionsSet.Setup( m => m.AddRange( It.Is<IEnumerable<UserSubscription>>( uss => uss.SequenceEqual( newSubscriptions ) ) ) ).Callback( () => Assert.That( callOrder++, Is.EqualTo( 1 ) ) );

            mockGuildDbContext.Setup( m => m.SaveChangesAsync() ).Returns( Task.FromResult( 0 ) ).Callback( () => Assert.That( callOrder++, Is.EqualTo( 2 ) ) );

            // Act
            await unitUnderTest.CreateSubscription(
                ba,
                newSubscriptions );

            //Verify
            billingAgreementSet.Verify( m => m.Add( ba ) );
            mySubscriptionsSet.Verify( m => m.AddRange( It.Is<IEnumerable<UserSubscription>>( uss => uss.SequenceEqual( newSubscriptions ) ) ) );
            mockGuildDbContext.Verify( m => m.SaveChangesAsync() );
        }

        //[Test]
        //public async Task Unsubscribe()
        //{
        //	// Arrange
        //	
        //	var unitUnderTest = this.CreateSubscriptionRepository();
        //	var billingAgreement =_fixture.Build<BillingAgreement>().Create();
        //	var userId = USER_ID.ToString();
        //	var dbObjectToChange =_fixture.Build<DbEntityEntry<BillingAgreement>>().Create();

        //	var callOrder = 0;
        //	mockGuildDbContext.Setup( m => m.Entry( billingAgreement ) ).Returns( dbObjectToChange ).Callback( () => Assert.That( callOrder++, Is.EqualTo( 0 ) ) );
        //	mockGuildDbContext.Setup( m => m.SaveChangesAsync() ).Returns( Task.FromResult( 0 ) ).Callback( () => Assert.That( callOrder++, Is.EqualTo( 2 ) ) );

        //	// Act
        //	await unitUnderTest.Unsubscribe(
        //		billingAgreement,
        //		userId );

        //	//Verify
        //	mockGuildDbContext.Verify( m => m.SaveChangesAsync() );

        //}

        //[Test]
        //public void Dispose_StateUnderTest_ExpectedBehavior()
        //{
        //	// Arrange
        //	var unitUnderTest = this.CreateSubscriptionRepository();

        //	// Act
        //	unitUnderTest.Dispose();

        //	// Assert
        //	mockGuildDbContext.Verify( m => m.Dispose() );
        //}
    }
}
