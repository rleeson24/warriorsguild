using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarriorsGuild.Areas.Payments.Controllers;
using WarriorsGuild.Areas.Payments.Mappers;
using WarriorsGuild.Data.Models.Payments;
using WarriorsGuild.Models.Payments;
using WarriorsGuild.Providers.Payments;

namespace WarriorsGuild.Tests.Areas.Payments.Controllers
{
    [TestFixture]
    public class PriceOptionsControllerTests
    {
        protected Fixture _fixture = new Fixture();
        private MockRepository mockRepository;

        private Mock<IPriceOptionManager> mockPriceOptionManager;
        private Mock<IStripePlanProvider> mockStripePlanProvider;
        private Mock<IPriceOptionMapper> mockPriceOptionMapper;
        private Mock<IBillingPlanRequestMapper> mockBillingPlanRequestMapper;
        private Mock<IBillingPlanManager> mockBillingPlanManager;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository( MockBehavior.Strict );

            this.mockPriceOptionManager = this.mockRepository.Create<IPriceOptionManager>();
            this.mockStripePlanProvider = this.mockRepository.Create<IStripePlanProvider>();
            this.mockPriceOptionMapper = this.mockRepository.Create<IPriceOptionMapper>();
            this.mockBillingPlanRequestMapper = this.mockRepository.Create<IBillingPlanRequestMapper>();
            this.mockBillingPlanManager = this.mockRepository.Create<IBillingPlanManager>();
        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        private PriceOptionsController CreatePriceOptionsController()
        {
            var controller = new PriceOptionsController(
                this.mockPriceOptionManager.Object,
                this.mockStripePlanProvider.Object,
                this.mockPriceOptionMapper.Object,
                this.mockBillingPlanRequestMapper.Object,
                this.mockBillingPlanManager.Object );
            //controller.Request = new HttpRequestMessage();
            //controller.Configuration = new HttpConfiguration();
            return controller;
        }

        [Test]
        public async Task GetManageablePriceOptions_No_Price_options_available_Should_return_an_empty_list()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();
            mockPriceOptionManager.Setup( m => m.ListPriceOptions() ).Returns( Task.FromResult( new PriceOption[] { }.AsEnumerable() ) );
            // Act
            var result = await unitUnderTest.GetManageablePriceOptions();
            var contentResult = result.Result as OkObjectResult;

            // Assert
            Assert.That( !(contentResult.Value as IEnumerable<ManageablePriceOption>).Any() );
            mockPriceOptionMapper.Verify( service => service.MapToManageablePriceOption( It.IsAny<PriceOption>() ), Times.Never() );
        }

        [Test]
        public async Task GetManageablePriceOptions_Price_options_available_Should_return_a_list_of_the_mapped_PriceOptions()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();

            var priceOptions = new List<PriceOption>
            {
				//fixture.Build<PriceOption>().With(u => u.AccountLocked, false).Create(),
				_fixture.Build<PriceOption>().Create(),
               _fixture.Build<PriceOption>().Create()
            };
            var mappedPriceOptions = new List<ManageablePriceOption>
            {
				//fixture.Build<PriceOption>().With(u => u.AccountLocked, false).Create(),
				_fixture.Build<ManageablePriceOption>().Create(),
               _fixture.Build<ManageablePriceOption>().Create()
            };

            mockPriceOptionManager.Setup( m => m.ListPriceOptions() ).Returns( Task.FromResult( priceOptions.AsEnumerable() ) );
            mockPriceOptionMapper.Setup( m => m.MapToManageablePriceOption( priceOptions[ 0 ] ) ).Returns( mappedPriceOptions[ 0 ] );
            mockPriceOptionMapper.Setup( m => m.MapToManageablePriceOption( priceOptions[ 1 ] ) ).Returns( mappedPriceOptions[ 1 ] );

            // Act
            var result = await unitUnderTest.GetManageablePriceOptions();
            var contentResult = result.Result as OkObjectResult;

            // Assert
            var resultValue = contentResult.Value as IEnumerable<ManageablePriceOption>;
            Assert.AreEqual( 2, resultValue.Count() );
            Assert.AreSame( mappedPriceOptions[ 0 ], resultValue.FirstOrDefault() );
            Assert.AreSame( mappedPriceOptions[ 1 ], resultValue.Skip( 1 ).FirstOrDefault() );
        }

        [Test]
        public async Task GetSuscribeablePriceOptions_No_Price_options_available_Should_return_an_empty_list()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();
            mockPriceOptionManager.Setup( m => m.ListPriceOptions() ).Returns( Task.FromResult( new PriceOption[] { }.AsEnumerable() ) );
            // Act
            var result = await unitUnderTest.GetSuscribeablePriceOptions();
            var contentResult = result.Result as OkObjectResult;

            // Assert
            Assert.That( !(contentResult.Value as IEnumerable<SubscribeablePriceOption>).Any() );
            mockPriceOptionMapper.Verify( service => service.MapToSubscribeablePriceOption( It.IsAny<PriceOption>() ), Times.Never() );
        }

        [Test]
        public async Task GetSuscribeablePriceOptions_Price_options_available_Should_return_a_list_of_the_mapped_active_PriceOptions()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();

            var stripeDetail1 = _fixture.Create<StripeDetail>();
            stripeDetail1.Active = false;
            var stripeDetail2 = _fixture.Create<StripeDetail>();
            stripeDetail2.Active = true;
            var priceOptions = new List<PriceOption>
            {
                new PriceOption() { Stripe =  stripeDetail1 },
                new PriceOption() { Stripe = stripeDetail2 }
            };
            var mappedPriceOptions = new List<SubscribeablePriceOption>
            {
               _fixture.Build<SubscribeablePriceOption>().Create()
            };

            mockPriceOptionManager.Setup( m => m.ListPriceOptions() ).Returns( Task.FromResult( priceOptions.AsEnumerable() ) );
            mockPriceOptionMapper.Setup( m => m.MapToSubscribeablePriceOption( priceOptions[ 1 ] ) ).Returns( mappedPriceOptions[ 0 ] );

            // Act
            var result = await unitUnderTest.GetSuscribeablePriceOptions();
            var contentResult = result.Result as OkObjectResult;

            // Assert
            var resultValue = contentResult.Value as IEnumerable<SubscribeablePriceOption>;
            Assert.True( resultValue.Count() == 1 );
            Assert.AreSame( mappedPriceOptions[ 0 ], resultValue.FirstOrDefault() );
            mockPriceOptionMapper.Verify( m => m.MapToSubscribeablePriceOption( priceOptions[ 0 ] ), Times.Never() );
        }

        [Test]
        public async Task GetSimplePriceOptions_No_Price_options_available_Should_return_an_empty_list()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();
            mockPriceOptionManager.Setup( m => m.ListPriceOptions() ).Returns( Task.FromResult( new PriceOption[] { }.AsEnumerable() ) );
            // Act
            var result = await unitUnderTest.GetSimplePriceOptions();
            var contentResult = result.Result as OkObjectResult;

            // Assert
            var resultValue = contentResult.Value as IEnumerable<SimplePriceOption>;
            Assert.That( !resultValue.Any() );
            mockPriceOptionMapper.Verify( service => service.MapToSimplePriceOption( It.IsAny<PriceOption>() ), Times.Never() );
        }

        [Test]
        public async Task GetSimplePriceOptions_Price_options_available_Should_return_a_list_of_the_mapped_active_PriceOptions()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();

            var stripeDetail1 = _fixture.Create<StripeDetail>();
            stripeDetail1.Active = false;
            var stripeDetail2 = _fixture.Create<StripeDetail>();
            stripeDetail2.Active = true;
            var priceOptions = new List<PriceOption>
            {
                new PriceOption() { Stripe =  stripeDetail1 },
                new PriceOption() { Stripe = stripeDetail2 }
            };
            var mappedPriceOptions = new List<SimplePriceOption>
            {
               _fixture.Build<SimplePriceOption>().Create()
            };

            mockPriceOptionManager.Setup( m => m.ListPriceOptions() ).Returns( Task.FromResult( priceOptions.AsEnumerable() ) );
            mockPriceOptionMapper.Setup( m => m.MapToSimplePriceOption( priceOptions[ 1 ] ) ).Returns( mappedPriceOptions[ 0 ] );

            // Act
            var result = await unitUnderTest.GetSimplePriceOptions();
            var contentResult = result.Result as OkObjectResult;

            // Assert
            var resultValue = contentResult.Value as IEnumerable<SimplePriceOption>;
            Assert.True( resultValue.Count() == 1 );
            Assert.AreSame( mappedPriceOptions[ 0 ], resultValue.FirstOrDefault() );
            mockPriceOptionMapper.Verify( m => m.MapToSimplePriceOption( priceOptions[ 0 ] ), Times.Never() );
        }

        [Test]
        public async Task GetPriceOption_Invalid_id_format_returns_NotFound()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();
            var id = "abc";

            // Act
            var result = await unitUnderTest.GetPriceOption( id );
            var contentResult = result.Result as NotFoundResult;

            // Assert
            Assert.True( contentResult is NotFoundResult );
            mockPriceOptionManager.Verify( m => m.GetPriceOption( It.IsAny<Guid>() ), Times.Never() );
        }

        [Test]
        public async Task GetPriceOption_Price_option_not_found_returns_NotFound()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();
            var id = Guid.NewGuid().ToString();

            var expectedPo = _fixture.Build<PriceOption>().Create();
            mockPriceOptionManager.Setup( m => m.GetPriceOption( GuidParameterEqualToId( id ) ) ).Returns( Task.FromResult( default( PriceOption ) ) );

            // Act
            var result = await unitUnderTest.GetPriceOption( id );
            var contentResult = result.Result as NotFoundResult;

            // Assert
            Assert.True( contentResult is NotFoundResult );
        }

        [Test]
        public async Task GetPriceOption_Price_option_found_returns_the_price_option()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();
            var id = Guid.NewGuid().ToString();

            var expectedPo = _fixture.Build<PriceOption>().Create();
            mockPriceOptionManager.Setup( m => m.GetPriceOption( GuidParameterEqualToId( id ) ) ).Returns( Task.FromResult( expectedPo ) );

            // Act
            var result = await unitUnderTest.GetPriceOption( id );
            var contentResult = result.Result as OkObjectResult;

            // Assert
            Assert.AreEqual( expectedPo, contentResult.Value );
        }

        [Test]
        public async Task PutPriceOption_When_ModelState_is_invalid_Returns_InvalidModelStateResult()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();
            var id = new Guid().ToString();

            var request = _fixture.Build<SavePriceOptionRequest>().Create();
            unitUnderTest.ModelState.AddModelError( "Description", "Description must not be empty" );

            // Act
            var result = await unitUnderTest.PutPriceOption(
                id,
                request );
            var contentResult = result;

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>( contentResult );
        }

        [Test]
        public async Task PutPriceOption_When_id_does_not_match_the_id_on_the_request_Returns_BadRequest()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();
            var id = new Guid().ToString();

            var request = _fixture.Build<SavePriceOptionRequest>().Create();

            // Act
            var result = await unitUnderTest.PutPriceOption(
                id,
                request );
            var contentResult = result as BadRequestResult;

            // Assert
            Assert.True( contentResult is BadRequestResult );
        }

        [Test]
        public async Task PutPriceOption_When_update_is_successful_Returns_BadRequest()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();

            var request = _fixture.Build<SavePriceOptionRequest>().Create();
            var id = request.Id.ToString();
            mockPriceOptionManager.Setup( m => m.Update( request ) ).Returns( Task.CompletedTask );

            // Act
            var result = await unitUnderTest.PutPriceOption(
                id,
                request );
            var contentResult = result as OkResult;

            // Assert
            Assert.True( contentResult is OkResult );
        }

        [Test]
        public async Task PostPriceOption_When_ModelState_is_invalid_Returns_InvalidModelStateResult()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();
            var id = new Guid().ToString();

            var request = _fixture.Build<SavePriceOptionRequest>().Create();
            unitUnderTest.ModelState.AddModelError( "Description", "Description must not be empty" );

            // Act
            var result = await unitUnderTest.PostPriceOption( request );

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>( result.Result );
        }

        [Test]
        public async Task PostPriceOption_When_Process_is_successful_Returns_CreatedAtRouteResponse()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();

            var request = _fixture.Build<SavePriceOptionRequest>().Create();
            var savedPriceOption = _fixture.Build<PriceOption>().Create();
            var createdBillingPlanResponse = _fixture.Build<CreateBillingPlanResponse>().Create();
            var mPriceOption = _fixture.Build<ManageablePriceOption>().Create();
            mockBillingPlanManager.Setup( m => m.CreateBillingPlan( request ) ).Returns( Task.FromResult( createdBillingPlanResponse ) );
            mockPriceOptionManager.Setup( m => m.Create( request, createdBillingPlanResponse ) ).Returns( Task.FromResult( savedPriceOption ) );
            mockPriceOptionMapper.Setup( m => m.MapToManageablePriceOption( savedPriceOption ) ).Returns( mPriceOption );
            // Act
            var result = await unitUnderTest.PostPriceOption( request );
            var contentResult = result.Result as CreatedAtActionResult;

            // Assert
            Assert.IsInstanceOf<CreatedAtActionResult>( result.Result );
            Assert.AreSame( mPriceOption, contentResult.Value );
        }

        [Test]
        public async Task DeletePriceOption_PriceOption_not_found_Returns_NotFoundResult()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();
            var id = new Guid();
            mockPriceOptionManager.Setup( m => m.GetPriceOption( GuidParameterEqualToId( id.ToString() ) ) ).Returns( Task.FromResult( default( PriceOption ) ) );

            // Act
            var result = await unitUnderTest.DeletePriceOption( id );
            var contentResult = result.Result as NotFoundResult;

            // Assert
            Assert.IsInstanceOf<NotFoundResult>( result.Result );
            mockPriceOptionManager.Verify( m => m.Delete( It.IsAny<PriceOption>() ), Times.Never() );
        }

        [Test]
        public async Task DeletePriceOption_PriceOption_is_found_Returns_OkResult_with_the_deleted_PriceOption()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();
            var id = new Guid();

            var priceOption = _fixture.Build<PriceOption>().Create();
            mockPriceOptionManager.Setup( m => m.GetPriceOption( GuidParameterEqualToId( id.ToString() ) ) ).Returns( Task.FromResult( priceOption ) );
            mockPriceOptionManager.Setup( m => m.Delete( priceOption ) ).Returns( Task.CompletedTask );
            // Act
            var result = await unitUnderTest.DeletePriceOption( id );
            var contentResult = result.Result as OkObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>( result.Result );
            Assert.AreSame( priceOption, contentResult.Value );
        }

        //[Test]
        //public async Task Activate_Id_is_not_a_Guid_Returns_BadRequest()
        //{
        //    // Arrange
        //    var unitUnderTest = CreatePriceOptionsController();
        //    var id = new Guid();

        //    // Act
        //    var result = await unitUnderTest.Activate( id );

        //    // Assert
        //    Assert.IsInstanceOf<NotFoundResult>( result.Result );
        //}

        [Test]
        public async Task Activate_PriceOption_not_found_Returns_BadRequest()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();
            var id = new Guid();
            mockPriceOptionManager.Setup( m => m.GetPriceOption( GuidParameterEqualToId( id.ToString() ) ) ).Returns( Task.FromResult( default( PriceOption ) ) );

            // Act
            var result = await unitUnderTest.Activate( id );

            // Assert
            Assert.IsInstanceOf<NotFoundResult>( result.Result );
        }

        [Test]
        public async Task Activate_AdditionalGuardianPlan_and_AdditionalWarriorPlan_are_null_Only_calls_StripeProvider_for_the_base_plan()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();
            var id = new Guid();

            var priceOption = _fixture.Build<PriceOption>().Create();
            var basePlanRequest = _fixture.Build<UpdateBillingPlanRequest>().Create();
            var guardianPlanRequest = _fixture.Build<UpdateBillingPlanRequest>().Create();
            var warriorPlanRequest = _fixture.Build<UpdateBillingPlanRequest>().Create();
            var updateBasePlanResult = _fixture.Build<UpdatePlanResponse>().Create();
            priceOption.AdditionalGuardianPlan = null;
            priceOption.AdditionalWarriorPlan = null;

            mockPriceOptionManager.Setup( m => m.GetPriceOption( GuidParameterEqualToId( id.ToString() ) ) ).Returns( Task.FromResult( priceOption ) );
            mockBillingPlanRequestMapper.Setup( m => m.CreateUpdateBillingPlanRequest( priceOption.StripePlanId, priceOption.StripeProductId, BillingPlanState.Active ) ).Returns( basePlanRequest );
            mockStripePlanProvider.Setup( m => m.Update( basePlanRequest ) ).Returns( Task.FromResult( updateBasePlanResult ) );

            // Act
            var result = await unitUnderTest.Activate( id );
            var contentResult = result.Result as OkObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>( contentResult );
            Assert.AreSame( priceOption, contentResult.Value );
            mockStripePlanProvider.Verify( m => m.Update( It.IsAny<UpdateBillingPlanRequest>() ), Times.Once() );
            mockBillingPlanRequestMapper.Verify( m => m.CreateUpdateBillingPlanRequest( It.IsAny<String>(), It.IsAny<String>(), It.IsAny<BillingPlanState>() ), Times.Once() );
            mockBillingPlanRequestMapper.Verify( m => m.CreateUpdateBillingPlanRequest( priceOption.StripePlanId, priceOption.StripeProductId, BillingPlanState.Active ), Times.Once() );
            mockStripePlanProvider.Verify( m => m.Update( basePlanRequest ), Times.Once() );
        }

        [Test]
        public async Task Activate_AdditionalGuardianPlan_is_null_Updates_the_base_plan_and_warrior_plan_and_returns_OkResult()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();
            var id = new Guid();

            var priceOption = _fixture.Build<PriceOption>().Create();
            var basePlanRequest = _fixture.Build<UpdateBillingPlanRequest>().Create();
            var guardianPlanRequest = _fixture.Build<UpdateBillingPlanRequest>().Create();
            var warriorPlanRequest = _fixture.Build<UpdateBillingPlanRequest>().Create();
            var updateBasePlanResult = _fixture.Build<UpdatePlanResponse>().Create();
            var updateGuardianPlanResult = _fixture.Build<UpdatePlanResponse>().Create();
            var updateWarriorPlanResult = _fixture.Build<UpdatePlanResponse>().Create();
            priceOption.AdditionalGuardianPlan = null;

            mockPriceOptionManager.Setup( m => m.GetPriceOption( GuidParameterEqualToId( id.ToString() ) ) ).Returns( Task.FromResult( priceOption ) );
            mockBillingPlanRequestMapper.Setup( m => m.CreateUpdateBillingPlanRequest( priceOption.StripePlanId, priceOption.StripeProductId, BillingPlanState.Active ) ).Returns( basePlanRequest );
            mockStripePlanProvider.Setup( m => m.Update( basePlanRequest ) ).Returns( Task.FromResult( updateBasePlanResult ) );
            mockBillingPlanRequestMapper.Setup( m => m.CreateUpdateBillingPlanRequest( priceOption.AdditionalWarriorPlan.StripePlanId, priceOption.AdditionalWarriorPlan.StripeProductId, BillingPlanState.Active ) ).Returns( warriorPlanRequest );
            mockStripePlanProvider.Setup( m => m.Update( warriorPlanRequest ) ).Returns( Task.FromResult( updateWarriorPlanResult ) );

            // Act
            var result = await unitUnderTest.Activate( id );
            var contentResult = result.Result as OkObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>( result.Result );
            Assert.AreSame( priceOption, contentResult.Value );
            mockStripePlanProvider.Verify( m => m.Update( It.IsAny<UpdateBillingPlanRequest>() ), Times.Exactly( 2 ) );
            mockBillingPlanRequestMapper.Verify( m => m.CreateUpdateBillingPlanRequest( It.IsAny<String>(), It.IsAny<String>(), It.IsAny<BillingPlanState>() ), Times.Exactly( 2 ) );
            mockBillingPlanRequestMapper.Verify( m => m.CreateUpdateBillingPlanRequest( priceOption.StripePlanId, priceOption.StripeProductId, BillingPlanState.Active ), Times.Once() );
            mockStripePlanProvider.Verify( m => m.Update( basePlanRequest ), Times.Once() );
            mockBillingPlanRequestMapper.Verify( m => m.CreateUpdateBillingPlanRequest( priceOption.AdditionalWarriorPlan.StripePlanId, priceOption.AdditionalWarriorPlan.StripeProductId, BillingPlanState.Active ), Times.Once() );
            mockStripePlanProvider.Verify( m => m.Update( warriorPlanRequest ), Times.Once() );
        }

        [Test]
        public async Task Activate_AdditionalWarriorPlan_is_null_Updates_the_base_plan_and_guardian_plan_and_returns_OkResult()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();
            var id = new Guid();

            var priceOption = _fixture.Build<PriceOption>().Create();
            var basePlanRequest = _fixture.Build<UpdateBillingPlanRequest>().Create();
            var guardianPlanRequest = _fixture.Build<UpdateBillingPlanRequest>().Create();
            var warriorPlanRequest = _fixture.Build<UpdateBillingPlanRequest>().Create();
            var updateBasePlanResult = _fixture.Build<UpdatePlanResponse>().Create();
            var updateGuardianPlanResult = _fixture.Build<UpdatePlanResponse>().Create();
            var updateWarriorPlanResult = _fixture.Build<UpdatePlanResponse>().Create();
            priceOption.AdditionalWarriorPlan = null;

            mockPriceOptionManager.Setup( m => m.GetPriceOption( GuidParameterEqualToId( id.ToString() ) ) ).Returns( Task.FromResult( priceOption ) );
            mockBillingPlanRequestMapper.Setup( m => m.CreateUpdateBillingPlanRequest( priceOption.StripePlanId, priceOption.StripeProductId, BillingPlanState.Active ) ).Returns( basePlanRequest );
            mockStripePlanProvider.Setup( m => m.Update( basePlanRequest ) ).Returns( Task.FromResult( updateBasePlanResult ) );
            mockBillingPlanRequestMapper.Setup( m => m.CreateUpdateBillingPlanRequest( priceOption.AdditionalGuardianPlan.StripePlanId, priceOption.AdditionalGuardianPlan.StripeProductId, BillingPlanState.Active ) ).Returns( guardianPlanRequest );
            mockStripePlanProvider.Setup( m => m.Update( guardianPlanRequest ) ).Returns( Task.FromResult( updateGuardianPlanResult ) );

            // Act
            var result = await unitUnderTest.Activate( id );
            var contentResult = result.Result as OkObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>( result.Result );
            Assert.AreSame( priceOption, contentResult.Value );
            mockStripePlanProvider.Verify( m => m.Update( It.IsAny<UpdateBillingPlanRequest>() ), Times.Exactly( 2 ) );
            mockBillingPlanRequestMapper.Verify( m => m.CreateUpdateBillingPlanRequest( It.IsAny<String>(), It.IsAny<String>(), It.IsAny<BillingPlanState>() ), Times.Exactly( 2 ) );
            mockBillingPlanRequestMapper.Verify( m => m.CreateUpdateBillingPlanRequest( priceOption.StripePlanId, priceOption.StripeProductId, BillingPlanState.Active ), Times.Once() );
            mockStripePlanProvider.Verify( m => m.Update( basePlanRequest ), Times.Once() );
            mockBillingPlanRequestMapper.Verify( m => m.CreateUpdateBillingPlanRequest( priceOption.AdditionalGuardianPlan.StripePlanId, priceOption.AdditionalGuardianPlan.StripeProductId, BillingPlanState.Active ), Times.Once() );
            mockStripePlanProvider.Verify( m => m.Update( guardianPlanRequest ), Times.Once() );
        }

        [Test]
        public async Task Deactivate_AdditionalGuardianPlan_and_AdditionalWarriorPlan_are_null_Only_calls_StripeProvider_for_the_base_plan()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();
            var id = new Guid();

            var priceOption = _fixture.Build<PriceOption>().Create();
            var basePlanRequest = _fixture.Build<UpdateBillingPlanRequest>().Create();
            var guardianPlanRequest = _fixture.Build<UpdateBillingPlanRequest>().Create();
            var warriorPlanRequest = _fixture.Build<UpdateBillingPlanRequest>().Create();
            var updateBasePlanResult = _fixture.Build<UpdatePlanResponse>().Create();
            priceOption.AdditionalGuardianPlan = null;
            priceOption.AdditionalWarriorPlan = null;

            mockPriceOptionManager.Setup( m => m.GetPriceOption( GuidParameterEqualToId( id.ToString() ) ) ).Returns( Task.FromResult( priceOption ) );
            mockBillingPlanRequestMapper.Setup( m => m.CreateUpdateBillingPlanRequest( priceOption.StripePlanId, priceOption.StripeProductId, BillingPlanState.Inactive ) ).Returns( basePlanRequest );
            mockStripePlanProvider.Setup( m => m.Update( basePlanRequest ) ).Returns( Task.FromResult( updateBasePlanResult ) );

            // Act
            var result = await unitUnderTest.Deactivate( id );
            var contentResult = result.Result as OkObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>( result.Result );
            Assert.AreSame( priceOption, contentResult.Value );
            mockStripePlanProvider.Verify( m => m.Update( It.IsAny<UpdateBillingPlanRequest>() ), Times.Once() );
            mockBillingPlanRequestMapper.Verify( m => m.CreateUpdateBillingPlanRequest( It.IsAny<String>(), It.IsAny<String>(), It.IsAny<BillingPlanState>() ), Times.Once() );
            mockBillingPlanRequestMapper.Verify( m => m.CreateUpdateBillingPlanRequest( priceOption.StripePlanId, priceOption.StripeProductId, BillingPlanState.Inactive ), Times.Once() );
            mockStripePlanProvider.Verify( m => m.Update( basePlanRequest ), Times.Once() );
        }

        [Test]
        public async Task Deactivate_AdditionalGuardianPlan_is_null_Updates_the_base_plan_and_warrior_plan_and_returns_OkResult()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();
            var id = new Guid();

            var priceOption = _fixture.Build<PriceOption>().Create();
            var basePlanRequest = _fixture.Build<UpdateBillingPlanRequest>().Create();
            var guardianPlanRequest = _fixture.Build<UpdateBillingPlanRequest>().Create();
            var warriorPlanRequest = _fixture.Build<UpdateBillingPlanRequest>().Create();
            var updateBasePlanResult = _fixture.Build<UpdatePlanResponse>().Create();
            var updateGuardianPlanResult = _fixture.Build<UpdatePlanResponse>().Create();
            var updateWarriorPlanResult = _fixture.Build<UpdatePlanResponse>().Create();
            priceOption.AdditionalGuardianPlan = null;

            mockPriceOptionManager.Setup( m => m.GetPriceOption( GuidParameterEqualToId( id.ToString() ) ) ).Returns( Task.FromResult( priceOption ) );
            mockBillingPlanRequestMapper.Setup( m => m.CreateUpdateBillingPlanRequest( priceOption.StripePlanId, priceOption.StripeProductId, BillingPlanState.Inactive ) ).Returns( basePlanRequest );
            mockStripePlanProvider.Setup( m => m.Update( basePlanRequest ) ).Returns( Task.FromResult( updateBasePlanResult ) );
            mockBillingPlanRequestMapper.Setup( m => m.CreateUpdateBillingPlanRequest( priceOption.AdditionalWarriorPlan.StripePlanId, priceOption.AdditionalWarriorPlan.StripeProductId, BillingPlanState.Inactive ) ).Returns( warriorPlanRequest );
            mockStripePlanProvider.Setup( m => m.Update( warriorPlanRequest ) ).Returns( Task.FromResult( updateWarriorPlanResult ) );

            // Act
            var result = await unitUnderTest.Deactivate( id );
            var contentResult = result.Result as OkObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>( result.Result );
            Assert.AreSame( priceOption, contentResult.Value );
            mockStripePlanProvider.Verify( m => m.Update( It.IsAny<UpdateBillingPlanRequest>() ), Times.Exactly( 2 ) );
            mockBillingPlanRequestMapper.Verify( m => m.CreateUpdateBillingPlanRequest( It.IsAny<String>(), It.IsAny<String>(), It.IsAny<BillingPlanState>() ), Times.Exactly( 2 ) );
            mockBillingPlanRequestMapper.Verify( m => m.CreateUpdateBillingPlanRequest( priceOption.StripePlanId, priceOption.StripeProductId, BillingPlanState.Inactive ), Times.Once() );
            mockStripePlanProvider.Verify( m => m.Update( basePlanRequest ), Times.Once() );
            mockBillingPlanRequestMapper.Verify( m => m.CreateUpdateBillingPlanRequest( priceOption.AdditionalWarriorPlan.StripePlanId, priceOption.AdditionalWarriorPlan.StripeProductId, BillingPlanState.Inactive ), Times.Once() );
            mockStripePlanProvider.Verify( m => m.Update( warriorPlanRequest ), Times.Once() );
        }

        [Test]
        public async Task Deactivate_AdditionalWarriorPlan_is_null_Updates_the_base_plan_and_guardian_plan_and_returns_OkResult()
        {
            // Arrange
            var unitUnderTest = CreatePriceOptionsController();
            var id = new Guid();

            var priceOption = _fixture.Build<PriceOption>().Create();
            var basePlanRequest = _fixture.Build<UpdateBillingPlanRequest>().Create();
            var guardianPlanRequest = _fixture.Build<UpdateBillingPlanRequest>().Create();
            var warriorPlanRequest = _fixture.Build<UpdateBillingPlanRequest>().Create();
            var updateBasePlanResult = _fixture.Build<UpdatePlanResponse>().Create();
            var updateGuardianPlanResult = _fixture.Build<UpdatePlanResponse>().Create();
            var updateWarriorPlanResult = _fixture.Build<UpdatePlanResponse>().Create();
            priceOption.AdditionalWarriorPlan = null;

            mockPriceOptionManager.Setup( m => m.GetPriceOption( GuidParameterEqualToId( id.ToString() ) ) ).Returns( Task.FromResult( priceOption ) );
            mockBillingPlanRequestMapper.Setup( m => m.CreateUpdateBillingPlanRequest( priceOption.StripePlanId, priceOption.StripeProductId, BillingPlanState.Inactive ) ).Returns( basePlanRequest );
            mockStripePlanProvider.Setup( m => m.Update( basePlanRequest ) ).Returns( Task.FromResult( updateBasePlanResult ) );
            mockBillingPlanRequestMapper.Setup( m => m.CreateUpdateBillingPlanRequest( priceOption.AdditionalGuardianPlan.StripePlanId, priceOption.AdditionalGuardianPlan.StripeProductId, BillingPlanState.Inactive ) ).Returns( guardianPlanRequest );
            mockStripePlanProvider.Setup( m => m.Update( guardianPlanRequest ) ).Returns( Task.FromResult( updateGuardianPlanResult ) );

            // Act
            var result = await unitUnderTest.Deactivate( id );
            var contentResult = result.Result as OkObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>( result.Result );
            Assert.AreSame( priceOption, contentResult.Value );
            mockStripePlanProvider.Verify( m => m.Update( It.IsAny<UpdateBillingPlanRequest>() ), Times.Exactly( 2 ) );
            mockBillingPlanRequestMapper.Verify( m => m.CreateUpdateBillingPlanRequest( It.IsAny<String>(), It.IsAny<String>(), It.IsAny<BillingPlanState>() ), Times.Exactly( 2 ) );
            mockBillingPlanRequestMapper.Verify( m => m.CreateUpdateBillingPlanRequest( priceOption.StripePlanId, priceOption.StripeProductId, BillingPlanState.Inactive ), Times.Once() );
            mockStripePlanProvider.Verify( m => m.Update( basePlanRequest ), Times.Once() );
            mockBillingPlanRequestMapper.Verify( m => m.CreateUpdateBillingPlanRequest( priceOption.AdditionalGuardianPlan.StripePlanId, priceOption.AdditionalGuardianPlan.StripeProductId, BillingPlanState.Inactive ), Times.Once() );
            mockStripePlanProvider.Verify( m => m.Update( guardianPlanRequest ), Times.Once() );
        }

        private static Guid GuidParameterEqualToId( string id )
        {
            return It.Is<Guid>( g => g.ToString() == id );
        }
    }
}
