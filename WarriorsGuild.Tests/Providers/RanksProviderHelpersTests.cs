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

        private Mock<IGuildDbContext> mockGuildDbContext;
        private Guid userId = Guid.NewGuid();
        protected Guid userIdForStatuses = Guid.NewGuid();

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

        private RanksProviderHelpers CreateRanksProviderHelpers()
        {
            return new RanksProviderHelpers(
                this.mockGuildDbContext.Object );
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
            var rankRequirements =_fixture.Build<RankRequirement>().With( rs => rs.Weight, 10 ).With( rs => rs.RankId, rankId ).CreateMany( 10 );
            var rankRequirementsDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankRequirement>( rankRequirements ) );
            mockGuildDbContext.Setup( m => m.RankRequirements ).Returns( rankRequirementsDbSet.Object );

            var rankStatuses = rankRequirements.Take( requirementsCompleted ).Select( rr => new RankStatus() { RankId = rr.RankId, RankRequirementId = rr.Id, UserId = userIdForStatuses, WarriorCompleted = DateTime.UtcNow } );
            var rankStatusesDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankStatus>( rankStatuses ) );
            mockGuildDbContext.Setup( m => m.RankStatuses ).Returns( rankStatusesDbSet.Object );

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
            var requirementIds =_fixture.CreateMany<Guid>( numberOfRings.Count() );
            var approvalRecordUserId = Guid.NewGuid();

            var reqs = new List<RankRequirement>();
            var j = 0;
            foreach ( var numRings in numberOfRings )
            {
                reqs.Add(_fixture.Build<RankRequirement>().With( rr => rr.RequireRing, numberOfRings[ j ] > 0 ).With( rr => rr.Id, requirementIds.Skip( j ).First() )
                                                            .With( rr => rr.RequiredRingCount, numberOfRings[ j ] == 0 ? (Int32?)null : numberOfRings[ j ] )
                                                            .With( rr => rr.RequireCross, numberOfCrosses[ j ] > 0 )
                                                            .With( rr => rr.RequiredCrossCount, numberOfCrosses[ j ] == 0 ? (Int32?)null : numberOfCrosses[ j ] )
                                                            .With( rr => rr.RankId, rankId ).Create() );
                j++;
            }
            mockGuildDbContext.Setup( m => m.RankRequirements ).Returns( TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankRequirement>( reqs ) ).Object );

            var reqRings = new List<RankStatusCompletedRing>();
            var reqCrosses = new List<RankStatusCompletedCross>();
            var i = 0;
            foreach ( var reqId in requirementIds )
            {
                reqRings.AddRange(_fixture.Build<RankStatusCompletedRing>().With( r => r.RankId, rankId ).With( r => r.RankRequirementId, reqId ).With( r => r.UserId, approvalRecordUserId ).CreateMany( numberOfRings[ i ] ) );
                reqCrosses.AddRange(_fixture.Build<RankStatusCompletedCross>().With( r => r.RankId, rankId ).With( r => r.RankRequirementId, reqId ).With( r => r.UserId, approvalRecordUserId ).CreateMany( numberOfCrosses[ i ] ) );
                i++;
            }

            var dbRings = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankStatusCompletedRing>( reqRings ) );
            var dbCrosses = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankStatusCompletedCross>( reqCrosses ) );
            if ( ringsRequired ) mockGuildDbContext.Setup( m => m.RankStatusRings ).Returns( dbRings.Object );
            if ( crossesRequired ) mockGuildDbContext.Setup( m => m.RankStatusCrosses ).Returns( dbCrosses.Object );

            var ringApprovals = new List<RingApproval>();
            foreach ( var ring in reqRings )
            {
                ringApprovals.Add(_fixture.Build<RingApproval>().Without( r => r.RecalledByWarriorTs ).Without( r => r.ReturnedTs ).Without( r => r.ReturnedReason ).With( r => r.RingId, ring.RingId ).Create() );
            }
            var crossApprovals = new List<CrossApproval>();
            foreach ( var cross in reqCrosses )
            {
                crossApprovals.Add(_fixture.Build<CrossApproval>().Without( r => r.ReturnedTs ).Without( r => r.RecalledByWarriorTs ).With( c => c.CrossId, cross.CrossId ).Create() );
            }
            var dbApprovedRings = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingApproval>( ringApprovals ) );
            var dbApprovedCrosses = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<CrossApproval>( crossApprovals ) );
            if ( ringsRequired ) mockGuildDbContext.Setup( m => m.RingApprovals ).Returns( dbApprovedRings.Object );
            //if ( crossesRequired ) mockGuildDbContext.Setup( m => m.CrossStatuses ).Returns( dbApprovedCrosses.Object );

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
            var requirementIds =_fixture.CreateMany<Guid>( numberOfRings.Count() );
            var approvalRecordUserId = Guid.NewGuid();

            var reqs = new List<RankRequirement>();
            var j = 0;
            foreach ( var numRings in numberOfRings )
            {
                reqs.Add(_fixture.Build<RankRequirement>().With( rr => rr.RequireRing, numberOfRings[ j ] > 0 ).With( rr => rr.Id, requirementIds.Skip( j ).First() )
                                                            .With( rr => rr.RequiredRingCount, numberOfRings[ j ] == 0 ? (Int32?)null : numberOfRings[ j ] )
                                                            .With( rr => rr.RequireCross, numberOfCrosses[ j ] > 0 )
                                                            .With( rr => rr.RequiredCrossCount, numberOfCrosses[ j ] == 0 ? (Int32?)null : numberOfCrosses[ j ] )
                                                            .With( rr => rr.RankId, rankId ).Create() );
                j++;
            }
            mockGuildDbContext.Setup( m => m.RankRequirements ).Returns( TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankRequirement>( reqs ) ).Object );

            var reqRings = new List<RankStatusCompletedRing>();
            var reqCrosses = new List<RankStatusCompletedCross>();
            var i = 0;
            foreach ( var reqId in requirementIds )
            {
                reqRings.AddRange(_fixture.Build<RankStatusCompletedRing>().With( r => r.RankId, rankId ).With( r => r.RankRequirementId, reqId ).With( r => r.UserId, approvalRecordUserId ).CreateMany( numberOfRings[ i ] ) );
                reqCrosses.AddRange(_fixture.Build<RankStatusCompletedCross>().With( r => r.RankId, rankId ).With( r => r.RankRequirementId, reqId ).With( r => r.UserId, approvalRecordUserId ).CreateMany( numberOfCrosses[ i ] ) );
                i++;
            }

            var dbRings = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankStatusCompletedRing>( reqRings ) );
            var dbCrosses = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankStatusCompletedCross>( reqCrosses ) );

            var ringApprovals = new List<RingApproval>();
            var crossApprovals = new List<CrossApproval>();
            var dbApprovedRings = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingApproval>( ringApprovals ) );
            var dbApprovedCrosses = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<CrossApproval>( crossApprovals ) );
            var ringWillFailFirst = false;
            var crossWillFailFirst = false;
            for ( var n = 0; n < numberOfRings.Length; n++ )
            {
                if ( numberOfRings[ n ] != 0 )
                {
                    ringWillFailFirst = true;
                    break;
                }
                else if ( numberOfCrosses[ n ] != 0 )
                {
                    crossWillFailFirst = true;
                    break;
                }
            }
            if ( ringsRequired && ringWillFailFirst ) mockGuildDbContext.Setup( m => m.RankStatusRings ).Returns( dbRings.Object );
            if ( crossesRequired && crossWillFailFirst ) mockGuildDbContext.Setup( m => m.RankStatusCrosses ).Returns( dbCrosses.Object );
            if ( ringsRequired && ringWillFailFirst ) mockGuildDbContext.Setup( m => m.RingApprovals ).Returns( dbApprovedRings.Object );
            //if ( crossesRequired && crossWillFailFirst ) mockGuildDbContext.Setup( m => m.CrossStatuses ).Returns( dbApprovedCrosses.Object );

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
            var rankApprovals =_fixture.Build<RankApproval>().Without( r => r.ApprovedAt ).Without( r => r.ReturnedTs ).Without( r => r.RecalledByWarriorTs ).CreateMany( numberOfApprovalRecords ).ToArray();
            for ( var i = 0; i < ranks.Count(); i++ )
            {
                if ( i < numberOfApprovalRecords ) rankApprovals[ i ].RankId = ranks[ i ].Id;
                if ( i + 1 < numberOfApprovalRecords ) rankApprovals[ i ].ApprovedAt = DateTime.UtcNow;
                ranks[ i ].Index = i;
            }
            var ranksDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<Rank>( ranks ) );
            mockGuildDbContext.Setup( m => m.Set<Rank>() ).Returns( ranksDbSet.Object );
            var rankApprovalsDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankApproval>( rankApprovals ) );
            mockGuildDbContext.Setup( m => m.Set<RankApproval>() ).Returns( rankApprovalsDbSet.Object );

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
            var rankApprovals =_fixture.Build<RankApproval>().With( r => r.UserId, userIdForStatuses ).Without( r => r.ApprovedAt )
                                                            .Without( r => r.ReturnedTs ).Without( r => r.RecalledByWarriorTs ).CreateMany( numberOfApprovalRecords ).ToArray();
            for ( var i = 0; i < ranks.Count(); i++ )
            {
                if ( i < rankIdIndex && i < numberOfApprovalRecords )
                {
                    rankApprovals[ i ].ApprovedAt = DateTime.UtcNow;
                    rankApprovals[ i ].PercentComplete = 100;
                }
                if ( i < numberOfApprovalRecords ) rankApprovals[ i ].RankId = ranks[ i ].Id;
                ranks[ i ].Index = i;
            }
            var ranksDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<Rank>( ranks ) );
            mockGuildDbContext.Setup( m => m.Set<Rank>() ).Returns( ranksDbSet.Object );
            var rankApprovalsDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankApproval>( rankApprovals ) );
            mockGuildDbContext.Setup( m => m.Set<RankApproval>() ).Returns( rankApprovalsDbSet.Object );

            // Act
            var result = await ranksProviderHelpers.AllPreviousRanksComplete( rankId, userIdForStatuses );

            // Assert
            Assert.IsTrue( result );
        }
    }
}
