//using AutoFixture;
//using Moq;
//using NUnit.Framework;
//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using WarriorsGuild.Areas.Rings.Models;
//using WarriorsGuild.DataAccess;
//using WarriorsGuild.DataAccess.Repositories;
//using static WarriorsGuild.Tests.TestHelpers;

//namespace WarriorsGuild.Tests.DataAccess.Repositories
//{
//	[TestFixture]
//	public class RingRepositoryTests
//	{
//		private Guid USER_ID = Guid.NewGuid();
//		private MockRepository mockRepository;

//		private Mock<IGuildDbContext> mockGuildDbContext;

//		[SetUp]
//		public void SetUp()
//		{
//			this.mockRepository = new MockRepository( MockBehavior.Strict );

//			this.mockGuildDbContext = this.mockRepository.Create<IGuildDbContext>();
//		}

//		[TearDown]
//		public void TearDown()
//		{
//			this.mockRepository.VerifyAll();
//		}

//		private RingRepository CreateRingRepository()
//		{
//			return new RingRepository(
//				this.mockGuildDbContext.Object );
//		}

//		[Test]
//		public async Task Get_StateUnderTest_ExpectedBehavior()
//		{
//			// Arrange
//			var unitUnderTest = this.CreateRingRepository();
//			// Arrange
//			
//			String userIdForStatuses = USER_ID.ToString();

//			var rings = new TestAsyncEnumerable<Ring>(_fixture.Build<Ring>().With( r => r.Requirements, new System.Collections.Generic.List<RingRequirement>() )
//											.With( r => r.Statuses, new System.Collections.Generic.List<RingStatus>() ).CreateMany() );
//			var ringForMatching = rings.Skip( 1 ).First();
//			var ringSet = TestHelpers.CreateDbSetMock( rings );

//			var ringRequirements = new TestAsyncEnumerable<RingRequirement>(_fixture.Build<RingRequirement>().With( rr => rr.RingId, ringForMatching.Id ).CreateMany() );
//			var ringRequirementSet = TestHelpers.CreateDbSetMock( ringRequirements );

//			var ringStatuses = new TestAsyncEnumerable<RingStatus>(_fixture.Build<RingStatus>().CreateMany() );
//			var ringStatusThatMatches = ringStatuses.Skip( 1 ).First();
//			ringStatusThatMatches.RingId = ringForMatching.Id;
//			ringStatusThatMatches.RingRequirementId = ringRequirements.Last().Id;
//			ringStatusThatMatches.UserId = userIdForStatuses;
//			var ringStatusSet = TestHelpers.CreateDbSetMock( ringStatuses );

//			mockGuildDbContext.Setup( m => m.Rings ).Returns( ringSet.Object );
//			mockGuildDbContext.Setup( m => m.RingRequirements ).Returns( ringRequirementSet.Object );
//			mockGuildDbContext.Setup( m => m.RingStatuses ).Returns( ringStatusSet.Object );

//			// Act
//			var result = await unitUnderTest.GetAsync( userIdForStatuses );
//			for ( var i = 0; i < result.Count(); i++ )
//			{
//				var expectedRing = rings.OrderBy( r => r.Index ).Skip( i ).First();
//				var expectedRequirements = ringRequirements.Where( rr => rr.RingId == expectedRing.Id );
//				var resultRing = result.OrderBy( r => r.Index ).Skip( i ).First();
//				Assert.AreEqual( expectedRing, resultRing );
//				Assert.True( expectedRequirements.SequenceEqual( resultRing.Requirements ) );
//				Assert.True( resultRing.Requirements.Select( r => r.Index ).SequenceEqual( resultRing.Requirements.Select( r => r.Index ).OrderBy( r => r ) ) );
//				Assert.True( (ringStatusThatMatches.RingId == expectedRing.Id ? new[] { ringStatusThatMatches } : new RingStatus[] { }).SequenceEqual( result.Skip( i ).First().Statuses ) );
//			}
//		}

//		//[Test]
//		//public void GetPublic_StateUnderTest_ExpectedBehavior()
//		//{
//		//	// Arrange
//		//	var unitUnderTest = this.CreateRingRepository();

//		//	// Act
//		//	var result = unitUnderTest.GetPublic();

//		//	// Assert
//		//	Assert.Fail();
//		//}

//		//[Test]
//		//public void Get_StateUnderTest_ExpectedBehavior1()
//		//{
//		//	// Arrange
//		//	var unitUnderTest = this.CreateRingRepository();
//		//	Guid id = TODO;
//		//	String userIdForStatuses = TODO;

//		//	// Act
//		//	var result = unitUnderTest.Get(
//		//		id,
//		//		userIdForStatuses );

//		//	// Assert
//		//	Assert.Fail();
//		//}

//		//[Test]
//		//public void Get_StateUnderTest_ExpectedBehavior2()
//		//{
//		//	// Arrange
//		//	var unitUnderTest = this.CreateRingRepository();
//		//	Guid id = TODO;

//		//	// Act
//		//	var result = unitUnderTest.Get(
//		//		id );

//		//	// Assert
//		//	Assert.Fail();
//		//}

//		//[Test]
//		//public async Task Update_StateUnderTest_ExpectedBehavior()
//		//{
//		//	// Arrange
//		//	var unitUnderTest = this.CreateRingRepository();
//		//	Guid id = TODO;
//		//	Ring ring = TODO;
//		//	Ring existingRing = TODO;

//		//	// Act
//		//	await unitUnderTest.Update(
//		//		id,
//		//		ring,
//		//		existingRing );

//		//	// Assert
//		//	Assert.Fail();
//		//}

//		//[Test]
//		//public async Task Add_StateUnderTest_ExpectedBehavior()
//		//{
//		//	// Arrange
//		//	var unitUnderTest = this.CreateRingRepository();
//		//	Ring ring = TODO;

//		//	// Act
//		//	var result = await unitUnderTest.Add(
//		//		ring );

//		//	// Assert
//		//	Assert.Fail();
//		//}

//		//[Test]
//		//public async Task SetHasImage_StateUnderTest_ExpectedBehavior()
//		//{
//		//	// Arrange
//		//	var unitUnderTest = this.CreateRingRepository();
//		//	Guid ringId = TODO;
//		//	Boolean hasImage = TODO;

//		//	// Act
//		//	await unitUnderTest.SetHasImage(
//		//		ringId,
//		//		hasImage );

//		//	// Assert
//		//	Assert.Fail();
//		//}

//		//[Test]
//		//public async Task DeleteRing_StateUnderTest_ExpectedBehavior()
//		//{
//		//	// Arrange
//		//	var unitUnderTest = this.CreateRingRepository();
//		//	Guid id = TODO;

//		//	// Act
//		//	var result = await unitUnderTest.DeleteRing(
//		//		id );

//		//	// Assert
//		//	Assert.Fail();
//		//}

//		//[Test]
//		//public async Task Pin_StateUnderTest_ExpectedBehavior()
//		//{
//		//	// Arrange
//		//	var unitUnderTest = this.CreateRingRepository();
//		//	Guid id = TODO;

//		//	// Act
//		//	var result = await unitUnderTest.Pin(
//		//		id );

//		//	// Assert
//		//	Assert.Fail();
//		//}

//		//[Test]
//		//public void GetRingStatus_StateUnderTest_ExpectedBehavior()
//		//{
//		//	// Arrange
//		//	var unitUnderTest = this.CreateRingRepository();

//		//	// Act
//		//	var result = unitUnderTest.GetRingStatus();

//		//	// Assert
//		//	Assert.Fail();
//		//}

//		//[Test]
//		//public async Task GetRingStatus_StateUnderTest_ExpectedBehavior1()
//		//{
//		//	// Arrange
//		//	var unitUnderTest = this.CreateRingRepository();
//		//	int id = TODO;

//		//	// Act
//		//	var result = await unitUnderTest.GetRingStatus(
//		//		id );

//		//	// Assert
//		//	Assert.Fail();
//		//}

//		//[Test]
//		//public void GetRingStatus_StateUnderTest_ExpectedBehavior2()
//		//{
//		//	// Arrange
//		//	var unitUnderTest = this.CreateRingRepository();
//		//	Guid ringId = TODO;
//		//	Guid requirementId = TODO;
//		//	String userId = TODO;

//		//	// Act
//		//	var result = unitUnderTest.GetRingStatus(
//		//		ringId,
//		//		requirementId,
//		//		userId );

//		//	// Assert
//		//	Assert.Fail();
//		//}

//		//[Test]
//		//public async Task AddStatusEntry_StateUnderTest_ExpectedBehavior()
//		//{
//		//	// Arrange
//		//	var unitUnderTest = this.CreateRingRepository();
//		//	RingStatusUpdateModel newValues = TODO;
//		//	string userIdForStatuses = TODO;

//		//	// Act
//		//	await unitUnderTest.AddStatusEntry(
//		//		newValues,
//		//		userIdForStatuses );

//		//	// Assert
//		//	Assert.Fail();
//		//}

//		//[Test]
//		//public async Task MarkGuardianReviewed_StateUnderTest_ExpectedBehavior()
//		//{
//		//	// Arrange
//		//	var unitUnderTest = this.CreateRingRepository();
//		//	RingStatus existingChild = TODO;

//		//	// Act
//		//	await unitUnderTest.MarkGuardianReviewed(
//		//		existingChild );

//		//	// Assert
//		//	Assert.Fail();
//		//}

//		//[Test]
//		//public async Task PostRingStatus_StateUnderTest_ExpectedBehavior()
//		//{
//		//	// Arrange
//		//	var unitUnderTest = this.CreateRingRepository();
//		//	RingStatus ringStatus = TODO;

//		//	// Act
//		//	var result = await unitUnderTest.PostRingStatus(
//		//		ringStatus );

//		//	// Assert
//		//	Assert.Fail();
//		//}

//		//[Test]
//		//public async Task DeleteRingStatus_StateUnderTest_ExpectedBehavior()
//		//{
//		//	// Arrange
//		//	var unitUnderTest = this.CreateRingRepository();
//		//	RingStatus ringStatus = TODO;

//		//	// Act
//		//	await unitUnderTest.DeleteRingStatus(
//		//		ringStatus );

//		//	// Assert
//		//	Assert.Fail();
//		//}

//		//[Test]
//		//public void Dispose_StateUnderTest_ExpectedBehavior()
//		//{
//		//	// Arrange
//		//	var unitUnderTest = this.CreateRingRepository();

//		//	// Act
//		//	unitUnderTest.Dispose();

//		//	// Assert
//		//	Assert.Fail();
//		//}
//	}
//}
