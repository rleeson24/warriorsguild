using AutoFixture;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using WarriorsGuild.Areas.Payments.Mappers;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Models.Payments;

namespace WarriorsGuild.Tests.Areas.Payments.Mappers
{
    [TestFixture]
    public class SubscriptionMapperTests
    {
        protected Fixture _fixture = new Fixture();
        private MockRepository mockRepository;



        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository( MockBehavior.Strict );


        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        private SubscriptionMapper CreateSubscriptionMapper()
        {
            return new SubscriptionMapper();
        }

        [Test]
        public void CreateViewModel_Mapped_properties_should_correspond_to_the_BillingAgreement_and_UserSubscription_properties()
        {
            // Arrange
            var unitUnderTest = CreateSubscriptionMapper();
            
            MySubscription subscriptionAndAgreement =_fixture.Build<MySubscription>().Create();
            IEnumerable<SubscriptionUser> usersOnSubscription =_fixture.Build<List<SubscriptionUser>>().Create();

            // Act
            var result = unitUnderTest.CreateViewModel(
                subscriptionAndAgreement,
                usersOnSubscription );

            // Assert
            Assert.AreEqual( usersOnSubscription, result.Users );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.PriceOption.AdditionalGuardianPlan.Charge, result.AdditionalCostPerGuardian );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.PriceOption.AdditionalWarriorPlan.Charge, result.AdditionalCostPerWarrior );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.AdditionalGuardians, result.AdditionalGuardians );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.AdditionalWarriors, result.AdditionalWarriors );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.PriceOption.Charge, result.Charge );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.PriceOption.Currency, result.Currency );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.Created, result.DateCreated );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.PriceOption.Description, result.Description );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.PriceOption.Frequency, result.Frequency );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.PriceOption.HasTrialPeriod, result.HasTrialPeriod );
            Assert.AreEqual( subscriptionAndAgreement.UserSubscription.IsPayingParty, result.IsPayingParty );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.LastPaid, result.LastPaid );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.NextPaymentDue, result.NextPaymentDue );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.PriceOption.NumberOfGuardians, result.NumberOfGuardians );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.PriceOption.NumberOfWarriors, result.NumberOfWarriors );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.PaymentMethod, result.PaymentMethod );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.PriceOption.Perks, result.Perks );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.PriceOption.Id, result.PriceOptionId );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.PriceOption.SetupFee, result.SetupFee );
            Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.PriceOption.TrialPeriodLength, result.TrialPeriodLength );
            //Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.Cancelled, subscriptionAndAgreement.UserSubscription. );
            //Assert.AreEqual( subscriptionAndAgreement.BillingAgreement, null );
            //Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.PriceOption.Show, result.Show );
            //Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.PriceOption.StripeStatus, result.Status );
            //Assert.AreEqual( subscriptionAndAgreement.BillingAgreement, result.StripeSubscriptionId );
            //Assert.AreEqual( subscriptionAndAgreement.BillingAgreement.PriceOption.Voided, result.Voided );
        }

        [Test]
        [TestCase( true, false )]
        [TestCase( true, true )]
        [TestCase( false, false )]
        public void MapToSubscriptionUser_The_properties_should_Be_mapped( Boolean isGuardian, Boolean isWarrior )
        {
            // Arrange
            var unitUnderTest = CreateSubscriptionMapper();
            
            ApplicationUser user =_fixture.Build<ApplicationUser>().With( u => u.ChildUsers, (ICollection<ApplicationUser>)null ).Create();
            UserSubscription subscription =_fixture.Build<UserSubscription>().Create();

            // Act
            var result = unitUnderTest.MapToSubscriptionUser(
                user,
                subscription,
                isGuardian,
                isWarrior );

            // Assert
            Assert.AreEqual( user.Id, result.Id );
            Assert.AreEqual( user.FirstName, result.FirstName );
            Assert.AreEqual( user.LastName, result.LastName );
            Assert.AreEqual( isWarrior, result.IsWarrior );
            Assert.AreEqual( isGuardian, result.IsGuardian );
            Assert.AreEqual( subscription.IsPayingParty, result.IsPayingParty );
        }
    }
}
