using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.Data.Models.Ranks.Status;
using WarriorsGuild.DataAccess;
using WarriorsGuild.DataAccess.Models;
using WarriorsGuild.Email;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Models;
using WarriorsGuild.Providers;
using WarriorsGuild.Ranks;
using WarriorsGuild.Ranks.Mappers;
using WarriorsGuild.Ranks.Models;
using WarriorsGuild.Ranks.Models.Status;
using WarriorsGuild.Ranks.ViewModels;
using WarriorsGuild.Storage;
using WarriorsGuild.Storage.Models;
using static WarriorsGuild.Tests.TestHelpers;

namespace WarriorsGuild.Tests.Providers
{
    [TestFixture]
    public class RankStatusProviderTests
    {
        protected Fixture _fixture = new Fixture();
        private MockRepository mockRepository;

        private Guid USERID = Guid.NewGuid();

        private Mock<IGuildDbContext> mockGuildDbContext;
        private Mock<IRankRepository> mockRankRepository;
        private Mock<IRankMapper> mockRankMapper;
        private Mock<IDateTimeProvider> mockDateTimeProvider;
        private Mock<IRanksProviderHelpers> mockRpHelpers;
        private Mock<IBlobProvider> mockAttachmentProvider;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository( MockBehavior.Strict );

            this.mockGuildDbContext = this.mockRepository.Create<IGuildDbContext>();
            this.mockRankRepository = this.mockRepository.Create<IRankRepository>();
            this.mockRankMapper = this.mockRepository.Create<IRankMapper>();
            this.mockDateTimeProvider = this.mockRepository.Create<IDateTimeProvider>();
            this.mockAttachmentProvider = this.mockRepository.Create<IBlobProvider>();
            this.mockRpHelpers = this.mockRepository.Create<IRanksProviderHelpers>();
        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        private RankStatusProvider CreateProvider()
        {
            return new RankStatusProvider(
                this.mockGuildDbContext.Object,
                this.mockRankRepository.Object,
                this.mockRankMapper.Object,
                this.mockDateTimeProvider.Object,
                this.mockRpHelpers.Object,
                this.mockAttachmentProvider.Object);
        }

        [Test]
        public void RankStatuses()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            IQueryable<RankStatus> expectedRanks = CreateAsyncQueryable( new List<RankStatus>() { _fixture.Build<RankStatus>().Create() } );
            mockRankRepository.Setup( m => m.RankStatuses() ).Returns( expectedRanks );

            // Act
            var result = unitUnderTest.RankStatuses();

            // Assert
            Assert.AreEqual( expectedRanks, result );
        }

        [Test]
        public async Task GetStatusesAsync()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var rankId = Guid.NewGuid();
            var userId = USERID;
            var expectedStatuses = _fixture.Build<RankStatus>().With( s => s.RankId, rankId ).With( s => s.UserId, userId ).With( s => s.RecalledByWarriorTs, (DateTime?)null ).With( s => s.ReturnedTs, (DateTime?)null ).CreateMany( 6 );
            mockRankRepository.Setup( m => m.RankStatuses() ).Returns( CreateAsyncQueryable( expectedStatuses ) );

            // Act
            var result = await unitUnderTest.GetStatusesAsync(
                rankId,
                userId );

            // Assert
            Assert.AreEqual( expectedStatuses, result );
        }

        [Test]
        public async Task RecordCompletionAsync_PreviousRanksNotApproved_NoDataShouldBeUpdated_AndTheResponseSuccessFlagShouldBeFalse()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var rankForStatus =_fixture.Build<RankStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            var savedStatus =_fixture.Build<RankStatus>().Create();
            mockRpHelpers.Setup( m => m.AllPreviousRanksComplete( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( false ) );

            // Act
            var result = await unitUnderTest.RecordCompletionAsync(
                rankForStatus,
                userIdForStatuses );

            // Assert
            Assert.IsFalse( result.Success );
            Assert.AreEqual( "You must complete the Ranks in order.", result.Error );
        }

        [Test]
        public async Task RecordCompletionAsync_ExistingStatusFound_NoDataShouldBeUpdated_AndTheResponseSuccessFlagShouldBeFalse()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var rankForStatus =_fixture.Build<RankStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            var savedStatus =_fixture.Build<RankStatus>().Create();
            mockRankRepository.Setup( m => m.GetRequirementStatusAsync( rankForStatus.RankId, rankForStatus.RankRequirementId, userIdForStatuses ) ).Returns( Task.FromResult( savedStatus ) );
            mockRpHelpers.Setup( m => m.AllPreviousRanksComplete( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( true ) );

            // Act
            var result = await unitUnderTest.RecordCompletionAsync(
                rankForStatus,
                userIdForStatuses );

            // Assert
            Assert.IsFalse( result.Success );
            Assert.AreEqual( "You have already completed this requirement.", result.Error );
        }

        [Test]
        public async Task RecordCompletionAsync_NonVoidUnapprovedApprovalRecordFound_NoDataShouldBeUpdated_AndTheResponseSuccessFlagShouldBeFalse()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var rankForStatus =_fixture.Build<RankStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            mockRankRepository.Setup( m => m.GetRequirementStatusAsync( rankForStatus.RankId, rankForStatus.RankRequirementId, userIdForStatuses ) ).Returns( Task.FromResult( (RankStatus)null ) );
            var pendingApproval = _fixture.Build<RankApproval>().With( r => r.RankId, rankForStatus.RankId ).With( r => r.UserId, userIdForStatuses ).With( r => r.ApprovedAt, (DateTime?)null ).With( r => r.RecalledByWarriorTs, (DateTime?)null ).With( r => r.ReturnedTs, (DateTime?)null ).Create();
            mockRankRepository.Setup( m => m.GetLatestPendingOrApprovedRankApprovalAsync( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( pendingApproval ) );
            mockRpHelpers.Setup( m => m.AllPreviousRanksComplete( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( true ) );

            // Act
            var result = await unitUnderTest.RecordCompletionAsync(
                rankForStatus,
                userIdForStatuses );

            // Assert
            Assert.IsFalse( result.Success );
            Assert.AreEqual( "There is currently a pending approval record.  Your Guardian must approve it before you can continue.", result.Error );
        }

        [Test]
        public async Task RecordCompletionAsync_NoApprovalRecordFound_AndDifferenceIsLessThan30_StatusShouldBeUpdated_AndTheResponseSuccessFlagShouldBeTrue()
        {
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var rankForStatus = _fixture.Build<RankStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            mockRankRepository.Setup( m => m.GetRequirementStatusAsync( rankForStatus.RankId, rankForStatus.RankRequirementId, userIdForStatuses ) ).Returns( Task.FromResult( (RankStatus)null ) );
            mockRankRepository.Setup( m => m.GetLatestPendingOrApprovedRankApprovalAsync( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( (RankApproval)null ) );
            mockRpHelpers.Setup( m => m.GetTotalCompletedPercent( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( 10 ) );
            mockRpHelpers.Setup( m => m.AllPreviousRanksComplete( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( true ) );
            var dummyDateTime = DateTime.UtcNow;
            mockDateTimeProvider.Setup( m => m.GetCurrentDateTime() ).Returns( dummyDateTime );
            var newRankStatus = _fixture.Build<RankStatus>().Create();
            mockRankMapper.Setup( m => m.CreateRankStatus( rankForStatus.RankId, rankForStatus.RankRequirementId, dummyDateTime, null, userIdForStatuses ) ).Returns( newRankStatus );
            RankStatus capturedStatus = null;
            mockRankRepository.Setup( m => m.PostRankStatusAsync( It.IsAny<RankStatus>() ) ).Callback<RankStatus>( s => capturedStatus = s ).Returns( Task.CompletedTask );

            // Act
            var result = await unitUnderTest.RecordCompletionAsync( rankForStatus, userIdForStatuses );

            // Assert
            Assert.IsTrue( result.Success );
            Assert.AreSame( newRankStatus, capturedStatus );
        }

        [Test]
        public async Task RecordCompletionAsync_NoApprovalRecordFound_AndNoStatusFoundForAnyRequirement_StatusShouldBeUpdated_AndTheResponseSuccessFlagShouldBeTrue()
        {
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var rankForStatus = _fixture.Build<RankStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            mockRankRepository.Setup( m => m.GetRequirementStatusAsync( rankForStatus.RankId, rankForStatus.RankRequirementId, userIdForStatuses ) ).Returns( Task.FromResult( (RankStatus)null ) );
            mockRankRepository.Setup( m => m.GetLatestPendingOrApprovedRankApprovalAsync( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( (RankApproval)null ) );
            mockRpHelpers.Setup( m => m.GetTotalCompletedPercent( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( 10 ) );
            mockRpHelpers.Setup( m => m.AllPreviousRanksComplete( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( true ) );
            var dummyDateTime = DateTime.UtcNow;
            mockDateTimeProvider.Setup( m => m.GetCurrentDateTime() ).Returns( dummyDateTime );
            var newRankStatus = _fixture.Build<RankStatus>().Create();
            mockRankMapper.Setup( m => m.CreateRankStatus( rankForStatus.RankId, rankForStatus.RankRequirementId, dummyDateTime, null, userIdForStatuses ) ).Returns( newRankStatus );
            RankStatus capturedStatus = null;
            mockRankRepository.Setup( m => m.PostRankStatusAsync( It.IsAny<RankStatus>() ) ).Callback<RankStatus>( s => capturedStatus = s ).Returns( Task.CompletedTask );

            // Act
            var result = await unitUnderTest.RecordCompletionAsync( rankForStatus, userIdForStatuses );

            // Assert
            Assert.IsTrue( result.Success );
            Assert.AreSame( newRankStatus, capturedStatus );
        }

        [Test]
        public async Task RecordCompletionAsync_ApprovedApprovalRecordFound_AndDifferenceIsLessThan30_StatusShouldBeUpdated_AndTheResponseSuccessFlagShouldBeTrue()
        {
            // Arrange - approved record with PercentComplete 50, total 60 so diff < 33
            var unitUnderTest = this.CreateProvider();
            var rankForStatus = _fixture.Build<RankStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            var approvedRecord = _fixture.Build<RankApproval>().With( r => r.RankId, rankForStatus.RankId ).With( r => r.UserId, userIdForStatuses ).With( r => r.PercentComplete, 50 ).With( r => r.ApprovedAt, DateTime.UtcNow ).Create();
            mockRankRepository.Setup( m => m.GetRequirementStatusAsync( rankForStatus.RankId, rankForStatus.RankRequirementId, userIdForStatuses ) ).Returns( Task.FromResult( (RankStatus)null ) );
            mockRankRepository.Setup( m => m.GetLatestPendingOrApprovedRankApprovalAsync( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( approvedRecord ) );
            mockRpHelpers.Setup( m => m.GetTotalCompletedPercent( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( 60 ) );
            mockRpHelpers.Setup( m => m.AllPreviousRanksComplete( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( true ) );
            var dummyDateTime = DateTime.UtcNow;
            mockDateTimeProvider.Setup( m => m.GetCurrentDateTime() ).Returns( dummyDateTime );
            var newRankStatus = _fixture.Build<RankStatus>().Create();
            mockRankMapper.Setup( m => m.CreateRankStatus( rankForStatus.RankId, rankForStatus.RankRequirementId, dummyDateTime, null, userIdForStatuses ) ).Returns( newRankStatus );
            RankStatus capturedStatus = null;
            mockRankRepository.Setup( m => m.PostRankStatusAsync( It.IsAny<RankStatus>() ) ).Callback<RankStatus>( s => capturedStatus = s ).Returns( Task.CompletedTask );

            // Act
            var result = await unitUnderTest.RecordCompletionAsync( rankForStatus, userIdForStatuses );

            // Assert
            Assert.IsTrue( result.Success );
            Assert.AreSame( newRankStatus, capturedStatus );
            mockRankRepository.Verify( m => m.AddApprovalEntry( It.IsAny<RankApproval>() ), Times.Never );
        }

        [Test]
        public async Task RecordCompletionAsync_ApprovedApprovalRecordFound_AndDifferenceIsGreaterThan30_StatusShouldNotBeUpdated_AndTheResponseSuccessFlagShouldBeTrue()
        {
            // Arrange - approved with PercentComplete 20, total 60, diff 40 >= 33 so blocks
            var unitUnderTest = this.CreateProvider();
            var rankForStatus = _fixture.Build<RankStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            var approvedRecord = _fixture.Build<RankApproval>().With( r => r.RankId, rankForStatus.RankId ).With( r => r.UserId, userIdForStatuses ).With( r => r.PercentComplete, 20 ).With( r => r.ApprovedAt, DateTime.UtcNow ).Create();
            mockRankRepository.Setup( m => m.GetRequirementStatusAsync( rankForStatus.RankId, rankForStatus.RankRequirementId, userIdForStatuses ) ).Returns( Task.FromResult( (RankStatus)null ) );
            mockRankRepository.Setup( m => m.GetLatestPendingOrApprovedRankApprovalAsync( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( approvedRecord ) );
            mockRpHelpers.Setup( m => m.GetTotalCompletedPercent( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( 60 ) );
            mockRpHelpers.Setup( m => m.AllPreviousRanksComplete( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( true ) );

            // Act
            var result = await unitUnderTest.RecordCompletionAsync( rankForStatus, userIdForStatuses );

            // Assert
            Assert.IsFalse( result.Success );
        }

        [Test]
        public async Task RecordCompletionAsync_NoApprovalRecordFound_AndDifferenceIsGreaterThan30_StatusShouldNotBeUpdated_AndTheResponseSuccessFlagShouldBeTrue()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var rankForStatus =_fixture.Build<RankStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            mockRankRepository.Setup( m => m.GetRequirementStatusAsync( rankForStatus.RankId, rankForStatus.RankRequirementId, userIdForStatuses ) ).Returns( Task.FromResult( (RankStatus)null ) );
            mockRankRepository.Setup( m => m.GetLatestPendingOrApprovedRankApprovalAsync( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( (RankApproval)null ) );
            mockRpHelpers.Setup( m => m.GetTotalCompletedPercent( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( 40 ) );
            mockRpHelpers.Setup( m => m.AllPreviousRanksComplete( rankForStatus.RankId, userIdForStatuses ) ).Returns( Task.FromResult( true ) );

            // Act
            var result = await unitUnderTest.RecordCompletionAsync(
                rankForStatus,
                userIdForStatuses );

            // Assert
            Assert.IsFalse( result.Success );
        }

        [Test]
        public async Task PostRankStatusAsync()
        {
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var rankStatus = _fixture.Build<RankStatus>().Create();
            mockRankRepository.Setup( m => m.AddStatusEntry( rankStatus ) );
            mockGuildDbContext.Setup( m => m.SaveChangesAsync() ).Returns( Task.FromResult( 1 ) );

            // Act
            var result = await unitUnderTest.PostRankStatusAsync( rankStatus );

            // Assert
            Assert.AreSame( rankStatus, result );
            mockRankRepository.Verify( m => m.AddStatusEntry( rankStatus ), Times.Once );
        }

        [Test]
        [TestCase( 0, 0 )]
        [TestCase( 0, 1 )]
        [TestCase( 1, 1 )]
        [TestCase( 2, 1 )]
        [TestCase( 0, 2 )]
        [TestCase( 1, 2 )]
        [TestCase( 2, 2 )]
        public async Task DeleteRankStatusAsync_WhenRankStatusFound_TheRecalledByWarriorTsShouldBeSet( Int32 numberOfCrosses, Int32 numberOfRings )
        {
            //var ranksProviderHelpers = this.CreateRanksProviderHelpers();
            //var updateModel = _fixture.Build<RankStatusUpdateModel>().Create();
            //var userIdForStatuses = Guid.NewGuid();

            //var removedRings = 0;
            //var removedCrosses = 0;
            //var relatedRings = _fixture.Build<RankStatusCompletedRing>()
            //                            .With( r => r.RankId, updateModel.RankId ).With( r => r.RankRequirementId, updateModel.RankRequirementId )
            //                            .With( r => r.UserId, userIdForStatuses )
            //                            .CreateMany( numberOfRings );

            //var existingRings = _fixture.CreateMany<RankStatusCompletedRing>( 10 ).Concat( relatedRings );
            //var dbRings = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankStatusCompletedRing>( existingRings ) );
            //mockGuildDbContext.Setup( m => m.RankStatusRings ).Returns( dbRings.Object );

            //foreach ( var ur in relatedRings )
            //{
            //    dbRings.Setup( m => m.Remove( ur ) )
            //            .Callback<RankStatusCompletedRing>( s => removedRings++ );
            //}

            //var relatedCrosses = _fixture.Build<RankStatusCompletedCross>()
            //                            .With( r => r.RankId, updateModel.RankId ).With( r => r.RankRequirementId, updateModel.RankRequirementId )
            //                            .With( r => r.UserId, userIdForStatuses )
            //                            .CreateMany( numberOfCrosses );
            //var existingCrosses = _fixture.CreateMany<RankStatusCompletedCross>( 10 ).Concat( relatedCrosses );
            //var dbCrosses = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankStatusCompletedCross>( existingCrosses ) );
            //mockGuildDbContext.Setup( m => m.RankStatusCrosses ).Returns( dbCrosses.Object );
            //foreach ( var ur in relatedCrosses )
            //{
            //    dbCrosses.Setup( m => m.Remove( ur ) )
            //            .Callback<RankStatusCompletedCross>( s => removedCrosses++ );
            //}

            //// Act
            //await ranksProviderHelpers.QueueRingsAndCrossesForRequirementForDelete(
            //    updateModel,
            //    userIdForStatuses );

            //// Assert
            //Assert.AreEqual( relatedCrosses.Count(), removedCrosses );
            //Assert.AreEqual( relatedRings.Count(), removedRings );

            var sequence = new MockSequence();
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var rankStatus =_fixture.Build<RankStatusUpdateModel>().Create();
            var rankStatuses =_fixture.Build<RankStatus>().With( rs => rs.RankId, rankStatus.RankId )
                                                                .With( rs => rs.RankRequirementId, rankStatus.RankRequirementId )
                                                                .With( rs => rs.UserId, USERID )
                                                                .CreateMany( 6 );
            rankStatuses.Skip( 2 ).First().RecalledByWarriorTs = null;
            rankStatuses.Skip( 2 ).First().ReturnedTs = null;
            rankStatuses.Skip( 2 ).First().GuardianCompleted = null;
            var rankStatusesDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankStatus>( rankStatuses ) );
            var pocAtts = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<ProofOfCompletionAttachment>(_fixture.Build<ProofOfCompletionAttachment>()
                                                                                                                    .With( a => a.RequirementId, rankStatus.RankRequirementId )
                                                                                                                    .With( a => a.UserId, USERID ).CreateMany( 2 ) ), true, false );

            var rankStatusCrossesDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankStatusCompletedCross>( Array.Empty<RankStatusCompletedCross>() ) );
            var rankStatusRingsDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankStatusCompletedRing>( Array.Empty<RankStatusCompletedRing>() ) );

            mockGuildDbContext.InSequence( sequence ).Setup( d => d.RankStatuses ).Returns( rankStatusesDbSet.Object );
            mockGuildDbContext.InSequence( sequence ).Setup( m => m.ProofOfCompletionAttachments ).Returns( pocAtts.Object );
            foreach ( var poc in pocAtts.Object )
            {
                mockGuildDbContext.InSequence( sequence ).Setup( m => m.SetDeleted( poc ) );
            }
            mockGuildDbContext.InSequence( sequence ).Setup( m => m.RankStatusCrosses ).Returns( rankStatusCrossesDbSet.Object );
            mockGuildDbContext.InSequence( sequence ).Setup( m => m.RankStatusRings ).Returns( rankStatusRingsDbSet.Object );
            mockGuildDbContext.InSequence( sequence ).Setup( m => m.SaveChangesAsync() ).Returns( Task.FromResult( 1 ) );

            // Act
            await unitUnderTest.DeleteRankStatusAsync(
                rankStatus, USERID );

            Assert.IsTrue( rankStatuses.Skip( 2 ).First().RecalledByWarriorTs.HasValue );
        }

        [Test]
        public async Task DeleteRankStatusAsync_WhenRankStatusFoundButAlreadyApproved_TheRecalledByWarriorTsShouldNotBeSet()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var rankStatus =_fixture.Build<RankStatusUpdateModel>().Create();
            var rankStatuses =_fixture.Build<RankStatus>().With( rs => rs.RankId, rankStatus.RankId )
                                                                .With( rs => rs.RankRequirementId, rankStatus.RankRequirementId )
                                                                .With( rs => rs.UserId, USERID )
                                                                .CreateMany( 6 );
            rankStatuses.Skip( 2 ).First().RecalledByWarriorTs = null;
            rankStatuses.Skip( 2 ).First().ReturnedTs = null;
            var rankStatusesDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankStatus>( rankStatuses ) );
            mockGuildDbContext.Setup( d => d.RankStatuses ).Returns( rankStatusesDbSet.Object );

            // Act
            var response = await unitUnderTest.DeleteRankStatusAsync(
                rankStatus, USERID );

            Assert.IsFalse( response.Success );
            Assert.AreEqual( "This requirement completion cannot be undone because it has already been approved", response.Error );
        }
    }
}
