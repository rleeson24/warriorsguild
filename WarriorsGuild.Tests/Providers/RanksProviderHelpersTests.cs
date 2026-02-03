using AutoFixture;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.Data.Models.Ranks.Status;
using WarriorsGuild.Data.Models.Rings.Status;
using WarriorsGuild.DataAccess;
using WarriorsGuild.DataAccess.Models;
using WarriorsGuild.Ranks;
using WarriorsGuild.Ranks.Models.Status;
using static WarriorsGuild.Tests.TestHelpers;

namespace WarriorsGuild.Tests.Providers
{
    [TestFixture]
    public class RanksProviderHelpersTests
    {
        protected Fixture _fixture = new Fixture();
        private MockRepository mockRepository;

        private Mock<IRankRepository> mockRankRepository;
        private Guid userId = Guid.NewGuid();
        protected Guid userIdForStatuses = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository( MockBehavior.Strict );

            this.mockRankRepository = this.mockRepository.Create<IRankRepository>();
        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        private RanksProviderHelpers CreateRanksProviderHelpers()
        {
            return new RanksProviderHelpers(
                this.mockRankRepository.Object );
        }

        [Test]
        [TestCase( 0 )]
        [TestCase( 1 )]
        [TestCase( 2 )]
        [TestCase( 3 )]
        [TestCase( 4 )]
        public async Task GetTotalCompletedPercent_StateUnderTest_ExpectedBehavior( Int32 requirementsCompleted )
        {
            // Arrange
            
            var ranksProviderHelpers = this.CreateRanksProviderHelpers();
            var rankId = Guid.NewGuid();
            mockRankRepository.Setup( m => m.GetTotalCompletedPercentAsync( rankId, userIdForStatuses ) ).ReturnsAsync( requirementsCompleted * 10 );

            // Act
            var result = await ranksProviderHelpers.GetTotalCompletedPercent(
                rankId,
                userIdForStatuses );

            // Assert
            Assert.AreEqual( requirementsCompleted * 10, result );
        }

        //[Test]
        //[TestCase( 0, 0 )]
        //[TestCase( 0, 1 )]
        //[TestCase( 1, 1 )]
        //[TestCase( 2, 1 )]
        //[TestCase( 0, 2 )]
        //[TestCase( 1, 2 )]
        //[TestCase( 2, 2 )]
        //public void QueueRingsAndCrossesForRequirementForSave_StateUnderTest_ExpectedBehavior( Int32 numberOfCrosses, Int32 numberOfRings )
        //{
            
        //    // Arrange
        //    var ranksProviderHelpers = this.CreateRanksProviderHelpers();
        //    var crosses =_fixture.CreateMany<Guid>( numberOfCrosses ).ToArray();
        //    var rings =_fixture.CreateMany<Guid>( numberOfRings ).ToArray();
        //    var updateModel =_fixture.Build<RankStatusUpdateModel>().With( r => r.Crosses, crosses )
        //                                                            .With( r => r.Rings, rings ).Create();
        //    var userIdForStatuses = Guid.NewGuid();

        //    var addedRings = 0;
        //    var addedCrosses = 0;
        //    if ( numberOfRings > 0 )
        //    {
        //        IEnumerable<RankStatusCompletedRing> existingRings =_fixture.CreateMany<RankStatusCompletedRing>( 1 );
        //        var dbRings = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankStatusCompletedRing>( existingRings ), false, true );
        //        mockGuildDbContext.Setup( m => m.RankStatusRings ).Returns( dbRings.Object );
        //        foreach ( var ur in updateModel.Rings )
        //        {
        //            dbRings.Setup( m => m.Add( It.Is<RankStatusCompletedRing>( r => r.RankId == updateModel.RankId
        //                                                                        && r.RankRequirementId == updateModel.RankRequirementId
        //                                                                        && r.UserId == userIdForStatuses
        //                                                                        && r.RingId == ur ) ) )
        //                                                        .Callback<RankStatusCompletedRing>( s => addedRings++ );
        //        }
        //    }

        //    if ( numberOfCrosses > 0 )
        //    {
        //        IEnumerable<RankStatusCompletedCross> existingCrosses =_fixture.CreateMany<RankStatusCompletedCross>( 1 );
        //        var dbCrosses = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankStatusCompletedCross>( existingCrosses ), false, true );
        //        mockGuildDbContext.Setup( m => m.RankStatusCrosses ).Returns( dbCrosses.Object );
        //        foreach ( var ur in updateModel.Crosses )
        //        {
        //            dbCrosses.Setup( m => m.Add( It.Is<RankStatusCompletedCross>( r => r.RankId == updateModel.RankId
        //                                                                        && r.RankRequirementId == updateModel.RankRequirementId
        //                                                                        && r.UserId == userIdForStatuses
        //                                                                        && r.CrossId == ur ) ) )
        //                                                        .Callback<RankStatusCompletedCross>( s => addedCrosses++ );
        //        }
        //    }

        //    // Act
        //    ranksProviderHelpers.QueueRingsAndCrossesForRequirementForSave(
        //        updateModel,
        //        userIdForStatuses );

        //    // Assert
        //    Assert.AreEqual( updateModel.Rings.Count(), addedRings );
        //    Assert.AreEqual( updateModel.Crosses.Count(), addedCrosses );
        //}

        //[Test]
        //public async Task GetRingsForRankStatusAsync_StateUnderTest_ExpectedBehavior()
        //{
        //	// Arrange
        //	var ranksProviderHelpers = this.CreateRanksProviderHelpers();
        //	Guid rankId = default( global::System.Guid );
        //	Guid requirementId = default( global::System.Guid );
        //	Guid userIdForStatuses = default( global::System.Guid );

        //	// Act
        //	var result = await ranksProviderHelpers.GetRingsForRankStatusAsync(
        //		rankId,
        //		requirementId,
        //		userIdForStatuses );

        //	// Assert
        //	Assert.Fail();
        //}

        //[Test]
        //public async Task GetCrossesForRankStatusAsync_StateUnderTest_ExpectedBehavior()
        //{
        //	// Arrange
        //	var ranksProviderHelpers = this.CreateRanksProviderHelpers();
        //	Guid rankId = default( global::System.Guid );
        //	Guid requirementId = default( global::System.Guid );
        //	Guid userIdForStatuses = default( global::System.Guid );

        //	// Act
        //	var result = await ranksProviderHelpers.GetCrossesForRankStatusAsync(
        //		rankId,
        //		requirementId,
        //		userIdForStatuses );

        //	// Assert
        //	Assert.Fail();
        //}

        [Test]
        [TestCase( new[] { 0, 1, 2, 0, 0, 0, 0, 3 }, new[] { 0, 0, 0, 2, 0, 0, 4, 0 } )]
        [TestCase( new[] { 0, 0, 0, 0, 0, 0, 0, 0 }, new[] { 0, 0, 0, 0, 0, 0, 0, 0 } )]
        [TestCase( new[] { 0, 0, 0, 0, 0, 0, 0, 3 }, new[] { 2, 0, 0, 0, 0, 0, 0, 0 } )]
        public async Task AllAssociatedRingsAndCrossesAreCompletedAndApprovedAsync_AllRingsCrossesAreApproved_ReturnsTrue( Int32[] numberOfRings, Int32[] numberOfCrosses )
        {
            
            if ( numberOfRings.Length != numberOfCrosses.Length ) Assert.Fail( "Invalid test parameter state" );
            var ringsRequired = !numberOfRings.All( r => r == 0 );
            var crossesRequired = !numberOfCrosses.All( r => r == 0 );
            // Arrange
            var ranksProviderHelpers = this.CreateRanksProviderHelpers();
            var rankId = Guid.NewGuid();
            var requirementIds =_fixture.CreateMany<Guid>( numberOfRings.Count() ).ToArray();
            var approvalRecordUserId = Guid.NewGuid();

            mockRankRepository.Setup( m => m.AllAssociatedRingsAndCrossesAreCompletedAndApprovedAsync( rankId, requirementIds, approvalRecordUserId ) ).ReturnsAsync( true );

            // Act
            var result = await ranksProviderHelpers.AllAssociatedRingsAndCrossesAreCompletedAndApprovedAsync(
                rankId,
                requirementIds,
                approvalRecordUserId );

            // Assert
            Assert.IsTrue( result );
        }

        [Test]
        [TestCase( new[] { 0, 1, 2, 0, 0, 6, 0, 3 }, new[] { 0, 0, 0, 2, 0, 0, 4, 0 } )]
        [TestCase( new[] { 0, 0, 0, 0, 0, 0, 9, 3 }, new[] { 2, 0, 0, 0, 6, 0, 0, 0 } )]
        public async Task AllAssociatedRingsAndCrossesAreCompletedAndApprovedAsync_NotAllRingsCrossesAreApproved_ReturnsFalse( Int32[] numberOfRings, Int32[] numberOfCrosses )
        {
            
            if ( numberOfRings.Length != numberOfCrosses.Length ) Assert.Fail( "Invalid test parameter state" );
            var ringsRequired = !numberOfRings.All( r => r == 0 );
            var crossesRequired = !numberOfCrosses.All( r => r == 0 );
            // Arrange
            var ranksProviderHelpers = this.CreateRanksProviderHelpers();
            var rankId = Guid.NewGuid();
            var requirementIds =_fixture.CreateMany<Guid>( numberOfRings.Count() ).ToArray();
            var approvalRecordUserId = Guid.NewGuid();

            mockRankRepository.Setup( m => m.AllAssociatedRingsAndCrossesAreCompletedAndApprovedAsync( rankId, requirementIds, approvalRecordUserId ) ).ReturnsAsync( false );

            // Act
            var result = await ranksProviderHelpers.AllAssociatedRingsAndCrossesAreCompletedAndApprovedAsync(
                rankId,
                requirementIds,
                approvalRecordUserId );

            // Assert
            Assert.IsFalse( result );
        }


        [Test]
        [TestCase( 1, 1 )]
        [TestCase( 2, 2 )]
        [TestCase( 4, 4 )]
        public async Task AllPreviousRanksCompleteAsync_NotAllPreviousRanksAreApproved_ReturnsFalse( Int32 numberOfApprovalRecords, Int32 rankIndex )
        {
            var ranksProviderHelpers = this.CreateRanksProviderHelpers();

            var ranks = new List<Rank>();
            ranks.AddRange(_fixture.CreateMany<Rank>( 8 ) );
            var rankId = ranks[ rankIndex ].Id;
            for ( var i = 0; i < ranks.Count(); i++ ) ranks[ i ].Index = i;

            mockRankRepository.Setup( m => m.AllPreviousRanksCompleteAsync( rankId, userId ) ).ReturnsAsync( false );

            // Act
            var result = await ranksProviderHelpers.AllPreviousRanksComplete( rankId, userId );

            // Assert
            Assert.IsFalse( result );
        }


        [Test]
        [TestCase( 0, 0 )]
        [TestCase( 1, 1 )]
        [TestCase( 3, 3 )]
        [TestCase( 4, 4 )]
        public async Task AllPreviousRanksComplete_AllPreviousRanksAreApproved_ReturnsTrue( Int32 numberOfApprovalRecords, Int32 rankIdIndex )
        {
            var ranksProviderHelpers = this.CreateRanksProviderHelpers();

            var ranks = new List<Rank>();
            ranks.AddRange(_fixture.CreateMany<Rank>( 8 ) );
            var rankId = ranks[ rankIdIndex ].Id;
            for ( var i = 0; i < ranks.Count(); i++ ) ranks[ i ].Index = i;

            mockRankRepository.Setup( m => m.AllPreviousRanksCompleteAsync( rankId, userIdForStatuses ) ).ReturnsAsync( true );

            // Act
            var result = await ranksProviderHelpers.AllPreviousRanksComplete( rankId, userIdForStatuses );

            // Assert
            Assert.IsTrue( result );
        }
    }
}
