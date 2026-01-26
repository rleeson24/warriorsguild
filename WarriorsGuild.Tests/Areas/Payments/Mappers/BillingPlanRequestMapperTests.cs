using Moq;
using NUnit.Framework;
using System;
using WarriorsGuild.Areas.Payments.Mappers;
using WarriorsGuild.Data.Models.Payments;

namespace WarriorsGuild.Tests.Areas.Payments.Mappers
{
    [TestFixture]
    public class BillingPlanRequestMapperTests
    {
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

        private BillingPlanRequestMapper CreateBillingPlanRequestMapper()
        {
            return new BillingPlanRequestMapper();
        }

        [Test]
        public void CreateSaveBillingPlanRequest_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var unitUnderTest = CreateBillingPlanRequestMapper();
            Frequency frequency = Frequency.Yearly;
            String planName = "pn";
            Decimal charge = 4.32m;
            String currency = "curr";
            Decimal setupFee = 2.00m;
            Int32? trialPeriodLength = null;

            // Act
            var result = unitUnderTest.CreateSaveBillingPlanRequest(
                frequency,
                planName,
                charge,
                currency,
                setupFee,
                trialPeriodLength );

            // Assert
            Assert.AreEqual( frequency, result.PlanFrequency );
            Assert.AreEqual( planName, result.PlanName );
            Assert.AreEqual( charge, result.RegularPlanPrice );
            Assert.AreEqual( currency, result.Currency );
            Assert.AreEqual( setupFee, result.SetupFee );
            Assert.AreEqual( trialPeriodLength, result.TrialPlanLength );
        }

        [Test]
        [TestCase( BillingPlanState.Active )]
        [TestCase( BillingPlanState.Created )]
        [TestCase( BillingPlanState.Deleted )]
        [TestCase( BillingPlanState.Inactive )]
        [TestCase( BillingPlanState.Incomplete )]
        public void CreateUpdateBillingPlanRequest_The_properties_should_be_mapped_correctly( BillingPlanState stateParm )
        {
            // Arrange
            var unitUnderTest = CreateBillingPlanRequestMapper();
            String planId = "planId";
            String productId = "productId";
            BillingPlanState state = stateParm;

            // Act
            var result = unitUnderTest.CreateUpdateBillingPlanRequest(
                planId,
                productId,
                state );

            // Assert
            Assert.AreEqual( planId, result.PlanId );
            Assert.AreEqual( productId, result.ProductId );
            Assert.AreEqual( stateParm, result.Status );
        }
    }
}
