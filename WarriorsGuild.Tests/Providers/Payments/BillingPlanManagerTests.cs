using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using WarriorsGuild.Areas.Payments.Mappers;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Providers.Payments;
using WarriorsGuild.Providers.Payments.Models;

namespace WarriorsGuild.Tests.Providers.Payments
{
    [TestFixture]
    public class BillingPlanManagerTests
    {
        private MockRepository mockRepository;

        private Mock<IGuildDbContext> mockGuildDbContext;
        private Mock<IStripePlanProvider> mockStripePlanProvider;
        private Mock<IPriceOptionMapper> mockPriceOptionMapper;
        private Mock<IBillingPlanRequestMapper> mockBillingPlanRequestMapper;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository( MockBehavior.Strict );

            this.mockGuildDbContext = this.mockRepository.Create<IGuildDbContext>();
            this.mockStripePlanProvider = this.mockRepository.Create<IStripePlanProvider>();
            this.mockPriceOptionMapper = this.mockRepository.Create<IPriceOptionMapper>();
            this.mockBillingPlanRequestMapper = this.mockRepository.Create<IBillingPlanRequestMapper>();
        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        private BillingPlanManager CreateManager()
        {
            return new BillingPlanManager(
                this.mockGuildDbContext.Object,
                this.mockStripePlanProvider.Object,
                this.mockPriceOptionMapper.Object,
                this.mockBillingPlanRequestMapper.Object );
        }

        [Test]
        public async Task CreateBillingPlan_StateUnderTest_ExpectedBehavior()
        {
            //// Arrange
            //var unitUnderTest = CreateManager();
            //SavePriceOptionRequest request = TODO;

            //// Act
            //var result = await unitUnderTest.CreateBillingPlan(
            //    request );

            // Assert
            Assert.Fail();
        }

        [Test]
        public async Task CreateStripeBillingPlan_StateUnderTest_ExpectedBehavior()
        {
            //// Arrange
            //var unitUnderTest = CreateManager();
            //SavePriceOptionRequest request = TODO;

            //// Act
            //var result = await unitUnderTest.CreateStripeBillingPlan(
            //    request );

            // Assert
            Assert.Fail();
        }
    }
}
