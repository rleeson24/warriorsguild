using AutoFixture;
using Moq;
using NUnit.Framework;
using System;
using WarriorsGuild.Areas.Payments.Mappers;
using WarriorsGuild.Data.Models.Payments;

namespace WarriorsGuild.Tests.Areas.Payments.Mappers
{
    [TestFixture]
    public class PriceOptionMapperTests
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

        private PriceOptionMapper CreatePriceOptionMapper()
        {
            return new PriceOptionMapper();
        }

        [Test]
        [TestCase( true )]
        [TestCase( false )]
        public void MapToManageablePriceOption_StateUnderTest_ExpectedBehavior( Boolean active )
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionMapper();
            
            PriceOption r =_fixture.Build<PriceOption>().Create();
            r.Stripe.Active = active;

            // Act
            var result = unitUnderTest.MapToManageablePriceOption( r );

            // Assert
            Assert.AreEqual( result.AdditionalGuardianCharge, r.AdditionalGuardianPlan.Charge );
            Assert.AreEqual( result.AdditionalWarriorCharge, r.AdditionalWarriorPlan.Charge );
            Assert.AreEqual( result.Charge, r.Charge );
            Assert.AreEqual( result.Description, r.Description );
            Assert.AreEqual( result.Frequency, r.Frequency );
            Assert.AreEqual( result.HasTrialPeriod, r.HasTrialPeriod );
            Assert.AreEqual( result.Id, r.Id );
            Assert.AreEqual( result.Key, r.Key );
            Assert.AreEqual( result.NumberOfGuardians, r.NumberOfGuardians );
            Assert.AreEqual( result.NumberOfWarriors, r.NumberOfWarriors );
            Assert.AreEqual( result.Perks, r.Perks );
            Assert.AreEqual( result.SetupFee, r.SetupFee );
            Assert.AreEqual( result.Show, r.Show );
            Assert.AreEqual( result.StripePlanId, r.StripePlanId );
            Assert.AreEqual( result.StripeStatus, active ? BillingPlanState.Active : BillingPlanState.Inactive );
            Assert.AreEqual( result.TrialPeriodLength, r.TrialPeriodLength );
        }

        [Test]
        [TestCase( true )]
        [TestCase( false )]
        public void MapToSubscribeablePriceOption_StateUnderTest_ExpectedBehavior( Boolean active )
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionMapper();
            
            PriceOption r =_fixture.Build<PriceOption>().Create();
            r.Stripe.Active = active;

            // Act
            var result = unitUnderTest.MapToSubscribeablePriceOption( r );

            // Assert
            Assert.AreEqual( result.AdditionalGuardianPlan, r.AdditionalGuardianPlan );
            Assert.AreEqual( result.AdditionalWarriorPlan, r.AdditionalWarriorPlan );
            Assert.AreEqual( result.Charge, r.Charge );
            Assert.AreEqual( result.Description, r.Description );
            Assert.AreEqual( result.Frequency, r.Frequency );
            Assert.AreEqual( result.HasTrialPeriod, r.HasTrialPeriod );
            Assert.AreEqual( result.Id, r.Id );
            Assert.AreEqual( result.NumberOfGuardians, r.NumberOfGuardians );
            Assert.AreEqual( result.NumberOfWarriors, r.NumberOfWarriors );
            Assert.AreEqual( result.Perks, r.Perks );
            Assert.AreEqual( result.SetupFee, r.SetupFee );
            Assert.AreEqual( result.StripePlanId, r.StripePlanId );
            Assert.AreEqual( result.StripeStatus, active ? BillingPlanState.Active : BillingPlanState.Inactive );
            Assert.AreEqual( result.TrialPeriodLength, r.TrialPeriodLength );
        }

        [Test]
        public void MapToSimplePriceOption_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionMapper();
            
            PriceOption r =_fixture.Build<PriceOption>().Create();

            // Act
            var result = unitUnderTest.MapToSimplePriceOption( r );

            // Assert
            Assert.AreEqual( result.Charge, r.Charge );
            Assert.AreEqual( result.Frequency, r.Frequency );
            Assert.AreEqual( result.Id, r.Id );
            Assert.AreEqual( result.NumberOfGuardians, r.NumberOfGuardians );
            Assert.AreEqual( result.NumberOfWarriors, r.NumberOfWarriors );
            Assert.AreEqual( result.Perks, r.Perks );
        }

        [Test]
        [TestCase( Frequency.Monthly, 0.32, "usd", 1, 3, "sadfds", "sadfasd" )]
        [TestCase( Frequency.Yearly, 5.5, "can", 3, 12, "sddadfds", "sa7dfasd" )]
        public void CreateGuardianAddOnPriceOption_StateUnderTest_ExpectedBehavior( Frequency frequency, Decimal charge, String currency,
                                                                                    Int32 numberOfGuardians, Int32? trialPeriodLength, String stripePlanId, String stripeProductId )
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionMapper();

            // Act
            var result = unitUnderTest.CreateGuardianAddOnPriceOption(
                frequency,
                charge,
                currency,
                numberOfGuardians,
                trialPeriodLength,
                stripePlanId,
                stripeProductId );

            // Assert
            Assert.AreEqual( result.Charge, charge );
            Assert.AreEqual( result.Frequency, frequency );
            Assert.AreEqual( result.Currency, currency );
            Assert.AreEqual( result.NumberOfGuardians, numberOfGuardians );
            Assert.AreEqual( result.NumberOfWarriors, 0 );
            Assert.AreEqual( result.TrialPeriodLength, trialPeriodLength );
            Assert.AreEqual( result.StripePlanId, stripePlanId );
            Assert.AreEqual( result.StripeProductId, stripeProductId );
        }

        [Test]
        [TestCase( Frequency.Monthly, 0.32, "usd", 1, 3, "sadfds", "sadfasd" )]
        [TestCase( Frequency.Yearly, 5.5, "can", 3, 12, "sddadfds", "sa7dfasd" )]
        public void CreateWarriorAddOnPriceOption_StateUnderTest_ExpectedBehavior( Frequency frequency, Decimal charge, String currency,
                                                                                    Int32 numberOfWarriors, Int32? trialPeriodLength, String stripePlanId, String stripeProductId )
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionMapper();

            // Act
            var result = unitUnderTest.CreateWarriorAddOnPriceOption(
                frequency,
                charge,
                currency,
                numberOfWarriors,
                trialPeriodLength,
                stripePlanId,
                stripeProductId );

            // Assert
            Assert.AreEqual( result.Charge, charge );
            Assert.AreEqual( result.Frequency, frequency );
            Assert.AreEqual( result.Currency, currency );
            Assert.AreEqual( result.NumberOfGuardians, 0 );
            Assert.AreEqual( result.NumberOfWarriors, numberOfWarriors );
            Assert.AreEqual( result.TrialPeriodLength, trialPeriodLength );
            Assert.AreEqual( result.StripePlanId, stripePlanId );
            Assert.AreEqual( result.StripeProductId, stripeProductId );
        }
    }
}
