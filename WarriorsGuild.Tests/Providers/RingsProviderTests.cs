using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models.Rings;
using WarriorsGuild.Data.Models.Rings.Status;
using WarriorsGuild.DataAccess;
using WarriorsGuild.DataAccess.Models;
using WarriorsGuild.Email;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Providers;
using WarriorsGuild.Rings;
using WarriorsGuild.Rings.Mappers;
using WarriorsGuild.Rings.Models.Status;
using WarriorsGuild.Storage;
using static WarriorsGuild.Tests.TestHelpers;

namespace WarriorsGuild.Tests.Providers
{
    [TestFixture]
    public class RingsProviderTests
    {
        protected Fixture _fixture = new Fixture();
        private Guid USERID = Guid.NewGuid();
        private MockRepository mockRepository;

        private Mock<IGuildDbContext> mockGuildDbContext;
        private Mock<IRingRepository> mockRingRepository;
        private Mock<IRingMapper> mockRingMapper;
        private Mock<IDateTimeProvider> mockDateTimeProvider;
        private Mock<IBlobProvider> mockAttachmentProvider;
        private Mock<IUserProvider> mockUserProvider;
        private Mock<IEmailProvider> emailProvider;
        private Mock<IHttpContextAccessor> httpContextAccessor;
        private Mock<ILogger<RingsProvider>> logger;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository( MockBehavior.Strict );

            this.mockGuildDbContext = this.mockRepository.Create<IGuildDbContext>();
            this.mockRingRepository = this.mockRepository.Create<IRingRepository>();
            this.mockRingMapper = this.mockRepository.Create<IRingMapper>();
            this.mockUserProvider = this.mockRepository.Create<IUserProvider>();
            this.mockDateTimeProvider = this.mockRepository.Create<IDateTimeProvider>();
            this.mockAttachmentProvider = this.mockRepository.Create<IBlobProvider>();
            this.emailProvider = this.mockRepository.Create<IEmailProvider>();
            this.httpContextAccessor = this.mockRepository.Create<IHttpContextAccessor>();
            this.logger = this.mockRepository.Create<ILogger<RingsProvider>>();
        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        private RingsProvider CreateProvider()
        {
            return new RingsProvider(
                this.mockGuildDbContext.Object,
                this.mockRingRepository.Object,
                this.mockRingMapper.Object,
                this.mockDateTimeProvider.Object,
                this.mockUserProvider.Object,
                this.mockAttachmentProvider.Object, emailProvider.Object, httpContextAccessor.Object, logger.Object );
        }

        [Test]
        public async Task Get_Returns_Compiled_Ring_Tuple_From_RepoAsync()
        {
            // Arrange
            
            String userIdForStatuses = USERID.ToString();

            var unitUnderTest = this.CreateProvider();

            var expectedResult =_fixture.Build<Ring>().With( r => r.IsPinned, false ).With( r => r.Requirements, new System.Collections.Generic.List<RingRequirement>() ).CreateMany( 8 );
            var pinnedRingsResult =_fixture.Build<PinnedRing>().CreateMany( 2 );
            pinnedRingsResult.First().Ring.Id = expectedResult.First().Id;
            pinnedRingsResult.Skip( 1 ).First().Ring.Id = expectedResult.Skip( 4 ).First().Id;

            mockRingRepository.Setup( m => m.GetListAsync() ).Returns( Task.FromResult( expectedResult ) );

            // Act
            var result = await unitUnderTest.GetListAsync();
            // Assert

            Assert.AreSame( expectedResult, result );
        }

        [Test]
        public async Task GetPublic_Returns_value_from_repoAsync()
        {
            // Arrange
            
            var unitUnderTest = this.CreateProvider();

            var ring =_fixture.Build<Ring>().Create();
            mockRingRepository.Setup( m => m.GetPublicAsync() ).Returns( Task.FromResult( ring ) );
            // Act
            var result = await unitUnderTest.GetPublicAsync();

            // Assert
            Assert.AreSame( ring, result );
        }

        [Test]
        public async Task Get_StateUnderTest_ExpectedBehavior1()
        {
            // Arrange
            
            var unitUnderTest = this.CreateProvider();
            var id = Guid.NewGuid();
            String userIdForStatuses = USERID.ToString();

            var ring =_fixture.Build<Ring>().Create();
            mockRingRepository.Setup( m => m.GetAsync( id, false ) ).Returns( Task.FromResult( ring ) );

            // Act
            var result = await unitUnderTest.GetAsync(
                id );

            // Assert
            Assert.AreSame( ring, result );
        }

        [Test]
        public async Task Update_If_the_ring_exists_Call_the_repo_to_update_it()
        {
            // Arrange
            
            var unitUnderTest = this.CreateProvider();
            var id = Guid.NewGuid();
            var ring =_fixture.Build<Ring>().Create();
            var existingRing =_fixture.Build<Ring>().Create();

            mockRingRepository.Setup( m => m.GetAsync( id, true ) ).Returns( Task.FromResult( existingRing ) );
            mockRingRepository.Setup( m => m.UpdateAsync( id, It.Is<Ring>( r => r == existingRing && r.Name == ring.Name && r.Description == ring.Description && r.Type == ring.Type ) ) ).Returns( Task.CompletedTask );

            // Act
            await unitUnderTest.UpdateAsync(
                id,
                ring );

            // Assert
            Assert.AreEqual( existingRing.Name, ring.Name );
            Assert.AreEqual( existingRing.Description, ring.Description );
            Assert.AreEqual( existingRing.Type, ring.Type );
            //TODO: make sure no values change except these
        }

        [Test]
        public async Task Update_If_the_ring_does_not_exist_Does_not_call_the_repo_to_update_it()
        {
            // Arrange
            
            var unitUnderTest = this.CreateProvider();
            var id = Guid.NewGuid();
            var ring =_fixture.Build<Ring>().Create();
            Ring existingRing = null;

            mockRingRepository.Setup( m => m.GetAsync( id, true ) ).Returns( Task.FromResult( existingRing ) );

            // Act
            await unitUnderTest.UpdateAsync(
                id,
                ring );

            mockRingRepository.Verify( m => m.UpdateAsync( It.IsAny<Guid>(), It.IsAny<Ring>() ), Times.Never() );
        }

        [Test]
        public async Task RecordCompletionAsync_ExistingStatusFound_NoDataShouldBeUpdated_AndTheResponseSuccessFlagShouldBeFalse()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var ringForStatus =_fixture.Build<RingStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            var savedStatus =_fixture.Build<RingStatus>().Create(); ;
            mockRingRepository.Setup( m => m.GetRequirementStatusAsync( ringForStatus.RingId, ringForStatus.RingRequirementId, userIdForStatuses ) ).Returns( Task.FromResult( savedStatus ) );

            // Act
            var result = await unitUnderTest.RecordCompletionAsync(
                ringForStatus,
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
            var ringForStatus =_fixture.Build<RingStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            mockRingRepository.Setup( m => m.GetRequirementStatusAsync( ringForStatus.RingId, ringForStatus.RingRequirementId, userIdForStatuses ) ).Returns( Task.FromResult( (RingStatus)null ) );
            var ringApprovalsDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingApproval>(_fixture.Build<RingApproval>().CreateMany( 8 ) ) );
            var existingApprovalRecord = ringApprovalsDbSet.Object.Skip( 3 ).First();
            existingApprovalRecord.UserId = userIdForStatuses;
            existingApprovalRecord.RingId = ringForStatus.RingId;
            existingApprovalRecord.ReturnedTs = null;
            existingApprovalRecord.ReturnedReason = null;
            existingApprovalRecord.RecalledByWarriorTs = null;
            existingApprovalRecord.ApprovedAt = null;
            mockGuildDbContext.Setup( m => m.RingApprovals ).Returns( ringApprovalsDbSet.Object );

            // Act
            var result = await unitUnderTest.RecordCompletionAsync(
                ringForStatus,
                userIdForStatuses );

            // Assert
            Assert.IsFalse( result.Success );
            Assert.AreEqual( "There is currently a pending approval record.  Your Guardian must approve it before you can continue.", result.Error );
        }

        [Test]
        public async Task RecordCompletionAsync_NoApprovalRecordFound_AndDifferenceBetweenTotalPercentCompletedAndLastApprovedPercentIsLessThan30_StatusShouldBeUpdated_AndTheResponseSuccessFlagShouldBeTrue()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var ringForStatus =_fixture.Build<RingStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            mockRingRepository.Setup( m => m.GetRequirementStatusAsync( ringForStatus.RingId, ringForStatus.RingRequirementId, userIdForStatuses ) ).Returns( Task.FromResult( (RingStatus)null ) );
            var ringApprovalsDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingApproval>(_fixture.Build<RingApproval>().CreateMany( 8 ) ) );
            RingStatus addedRingStatus = null;
            mockGuildDbContext.Setup( m => m.RingApprovals ).Returns( ringApprovalsDbSet.Object );
            var ringStatusesDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingStatus>(_fixture.Build<RingStatus>().CreateMany( 8 ) ), false, true );
            mockGuildDbContext.Setup( m => m.RingStatuses ).Returns( ringStatusesDbSet.Object );
            var dummyDateTime = DateTime.UtcNow;
            mockDateTimeProvider.Setup( m => m.GetCurrentDateTime() ).Returns( dummyDateTime );
            var newRingStatus =_fixture.Build<RingStatus>().Create();
            mockRingMapper.Setup( m => m.CreateRingStatus( ringForStatus.RingId, ringForStatus.RingRequirementId, dummyDateTime, null, userIdForStatuses ) ).Returns( newRingStatus );

            mockGuildDbContext.Setup( m => m.SaveChangesAsync() ).Returns( Task.FromResult( 1 ) );

            ringStatusesDbSet.Setup( m => m.Add( newRingStatus ) )
                                                            .Callback<RingStatus>( ( s ) => addedRingStatus = s );

            // Act
            var result = await unitUnderTest.RecordCompletionAsync(
                ringForStatus,
                userIdForStatuses );

            // Assert
            Assert.IsTrue( result.Success );
            Assert.AreSame( newRingStatus, addedRingStatus );
        }

        [Test]
        public async Task RecordCompletionAsync_NoApprovalRecordFound_AndNoStatusFoundForAnyRequirement_StatusShouldBeUpdated_AndTheResponseSuccessFlagShouldBeTrue()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var ringForStatus =_fixture.Build<RingStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            mockRingRepository.Setup( m => m.GetRequirementStatusAsync( ringForStatus.RingId, ringForStatus.RingRequirementId, userIdForStatuses ) ).Returns( Task.FromResult( (RingStatus)null ) );
            var ringApprovalsDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingApproval>(_fixture.Build<RingApproval>().CreateMany( 8 ) ) );
            RingStatus addedRingStatus = null;
            mockGuildDbContext.Setup( m => m.RingApprovals ).Returns( ringApprovalsDbSet.Object );
            var ringStatusesDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingStatus>( new RingStatus[ 0 ] ), false, true );
            mockGuildDbContext.Setup( m => m.RingStatuses ).Returns( ringStatusesDbSet.Object );
            var dummyDateTime = DateTime.UtcNow;
            mockDateTimeProvider.Setup( m => m.GetCurrentDateTime() ).Returns( dummyDateTime );
            var newRingStatus =_fixture.Build<RingStatus>().Create();
            mockRingMapper.Setup( m => m.CreateRingStatus( ringForStatus.RingId, ringForStatus.RingRequirementId, dummyDateTime, null, userIdForStatuses ) ).Returns( newRingStatus );

            mockGuildDbContext.Setup( m => m.SaveChangesAsync() ).Returns( Task.FromResult( 1 ) );

            ringStatusesDbSet.Setup( m => m.Add( newRingStatus ) )
                                                            .Callback<RingStatus>( ( s ) => addedRingStatus = s );

            // Act
            var result = await unitUnderTest.RecordCompletionAsync(
                ringForStatus,
                userIdForStatuses );

            // Assert
            Assert.IsTrue( result.Success );
            Assert.AreSame( newRingStatus, addedRingStatus );
        }

        [Test]
        public async Task RecordCompletionAsync_ApprovedApprovalRecordFoundAndDifferenceBetweenTotalPercentCompletedAndLastApprovedPercentIsLessThan30_StatusShouldBeUpdated_AndNoApprovalRecordCreated_AndTheResponseSuccessFlagShouldBeTrue()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var ringForStatus =_fixture.Build<RingStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            mockRingRepository.Setup( m => m.GetRequirementStatusAsync( ringForStatus.RingId, ringForStatus.RingRequirementId, userIdForStatuses ) ).Returns( Task.FromResult( (RingStatus)null ) );
            var ringApprovalsDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingApproval>(_fixture.Build<RingApproval>().With( r => r.RecalledByWarriorTs, (DateTime?)null ).CreateMany( 10 ) ) );
            ringApprovalsDbSet.Object.Skip( 4 ).First().RingId = ringForStatus.RingId;
            RingStatus addedRingStatus = null;
            mockGuildDbContext.Setup( m => m.RingApprovals ).Returns( ringApprovalsDbSet.Object );
            var ringRequirements =_fixture.Build<RingRequirement>().With( rs => rs.Weight, 10 ).With( rs => rs.RingId, ringForStatus.RingId ).CreateMany( 10 );
            var ringStatuses = ringRequirements.Take( 6 ).Select( rr => new RingStatus() { RingId = rr.RingId, RingRequirementId = rr.Id, UserId = userIdForStatuses, WarriorCompleted = DateTime.UtcNow } );
            var ringStatusesDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingStatus>( ringStatuses ), false, true );
            mockGuildDbContext.Setup( m => m.RingStatuses ).Returns( ringStatusesDbSet.Object );
            var dummyDateTime = DateTime.UtcNow;
            mockDateTimeProvider.Setup( m => m.GetCurrentDateTime() ).Returns( dummyDateTime );
            var newRingStatus =_fixture.Build<RingStatus>().Create();
            mockRingMapper.Setup( m => m.CreateRingStatus( ringForStatus.RingId, ringForStatus.RingRequirementId, dummyDateTime, null, userIdForStatuses ) ).Returns( newRingStatus );
            mockGuildDbContext.Setup( m => m.SaveChangesAsync() ).Returns( Task.FromResult( 1 ) );

            ringStatusesDbSet.Setup( m => m.Add( newRingStatus ) )
                                                            .Callback<RingStatus>( ( s ) => addedRingStatus = s );


            // Act
            var result = await unitUnderTest.RecordCompletionAsync(
                ringForStatus,
                userIdForStatuses );

            // Assert
            Assert.IsTrue( result.Success );
            Assert.AreSame( newRingStatus, addedRingStatus );
            ringApprovalsDbSet.Verify( r => r.Add( It.IsAny<RingApproval>() ), Times.Never );
        }

        [Test]
        public async Task RecordCompletionAsync_PercentCompletedIs100_StatusShouldBeUpdated_AndTheResponseSuccessFlagShouldBeTrue()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var ringForStatus =_fixture.Build<RingStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            mockRingRepository.Setup( m => m.GetRequirementStatusAsync( ringForStatus.RingId, ringForStatus.RingRequirementId, userIdForStatuses ) ).Returns( Task.FromResult( (RingStatus)null ) );
            var ringApprovalsDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingApproval>(_fixture.Build<RingApproval>().With( r => r.RecalledByWarriorTs, (DateTime?)null ).CreateMany( 10 ) ) );
            ringApprovalsDbSet.Object.Skip( 4 ).First().RingId = ringForStatus.RingId;
            RingStatus addedRingStatus = null;
            mockGuildDbContext.Setup( m => m.RingApprovals ).Returns( ringApprovalsDbSet.Object );
            var ringRequirements =_fixture.Build<RingRequirement>().With( rs => rs.Weight, 10 ).With( rs => rs.RingId, ringForStatus.RingId ).CreateMany( 10 );
            var ringStatuses = ringRequirements.Take( 9 ).Select( rr => new RingStatus() { RingId = rr.RingId, RingRequirementId = rr.Id, UserId = userIdForStatuses, WarriorCompleted = DateTime.UtcNow } );
            var ringStatusesDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingStatus>( ringStatuses ), false, true );
            mockGuildDbContext.Setup( m => m.RingStatuses ).Returns( ringStatusesDbSet.Object );
            var dummyDateTime = DateTime.UtcNow;
            mockDateTimeProvider.Setup( m => m.GetCurrentDateTime() ).Returns( dummyDateTime );
            var newRingStatus =_fixture.Build<RingStatus>().With( rs => rs.RingId, ringForStatus.RingId ).With( rs => rs.RingRequirementId, ringRequirements.Last().Id ).Create();
            mockRingMapper.Setup( m => m.CreateRingStatus( ringForStatus.RingId, ringForStatus.RingRequirementId, dummyDateTime, null, userIdForStatuses ) ).Returns( newRingStatus );
            mockGuildDbContext.Setup( m => m.SaveChangesAsync() ).Returns( Task.FromResult( 1 ) );

            ringStatusesDbSet.Setup( m => m.Add( newRingStatus ) )
                                                            .Callback<RingStatus>( ( s ) => addedRingStatus = s );

            // Act
            var result = await unitUnderTest.RecordCompletionAsync(
                ringForStatus,
                userIdForStatuses );

            // Assert
            Assert.IsTrue( result.Success );
            Assert.AreSame( newRingStatus, addedRingStatus );
        }

        [Test]
        public async Task GetPendingApprovalsAsync_Given_no_approval_records_Returns_an_empty_list()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var userIdForStatuses = USERID;
            var ringApprovals =_fixture.Build<RingApproval>().With( ra => ra.ApprovedAt, (DateTime?)null ).CreateMany( 3 );
            mockGuildDbContext.Setup( m => m.RingApprovals ).Returns( TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingApproval>( ringApprovals ) ).Object );

            // Act
            var result = await unitUnderTest.GetPendingApprovalsAsync(
                userIdForStatuses );

            // Assert
            Assert.IsFalse( result.Any() );
        }

        [Test]
        public async Task GetPendingApprovalsAsync_Given_no_unapproved_approval_records_Returns_an_empty_list()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var userIdForStatuses = USERID;
            var ringApprovals =_fixture.Build<RingApproval>().With( ra => ra.UserId, userIdForStatuses ).CreateMany( 3 );
            mockGuildDbContext.Setup( m => m.RingApprovals ).Returns( TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingApproval>( ringApprovals ) ).Object );

            // Act
            var result = await unitUnderTest.GetPendingApprovalsAsync(
                userIdForStatuses );

            // Assert
            Assert.IsFalse( result.Any() );
        }

        [Test]
        public async Task GetPendingApprovalsAsync_Given_an_unapproved_approval_record_Returns_the_approval_record_with_recently_completed_requirements()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var userIdForStatuses = USERID;
            var approvalRelatedRingId = Guid.NewGuid();
            var ringApprovals =_fixture.Build<RingApproval>().With( ra => ra.ApprovedAt, (DateTime?)null ).With( ra => ra.RecalledByWarriorTs, (DateTime?)null ).With( ra => ra.ReturnedTs, (DateTime?)null ).CreateMany( 3 );
            ringApprovals.Skip( 2 ).First().UserId = userIdForStatuses;
            ringApprovals.Skip( 2 ).First().RingId = approvalRelatedRingId;
            var relatedRequirements =_fixture.Build<RingRequirement>().With( rr => rr.RingId, approvalRelatedRingId ).CreateMany( 8 );
            var relatedStatuses = relatedRequirements.Take( 3 ).Select( rr =>_fixture.Build<RingStatus>().With( rs => rs.RingId, approvalRelatedRingId )
                                                                                                          .With( rs => rs.RingRequirementId, rr.Id )
                                                                                                          .With( rs => rs.UserId, userIdForStatuses )
                                                                                                          .With( ra => ra.RecalledByWarriorTs, (DateTime?)null ).With( ra => ra.ReturnedTs, (DateTime?)null ).Create() ).ToArray();
            var unapprovedRelatedStatuses = relatedRequirements.Skip( 3 ).Take( 3 ).Select( rr =>_fixture.Build<RingStatus>().With( rs => rs.RingId, approvalRelatedRingId )
                                                                                                            .With( rs => rs.RingRequirementId, rr.Id )
                                                                                                          .With( rs => rs.GuardianCompleted, (DateTime?)null )
                                                                                                          .With( rs => rs.UserId, userIdForStatuses )
                                                                                                          .With( ra => ra.RecalledByWarriorTs, (DateTime?)null ).With( ra => ra.ReturnedTs, (DateTime?)null ).Create() ).ToArray();
            var requirements =_fixture.Build<RingRequirement>().CreateMany( 10 ).Concat( relatedRequirements );
            mockGuildDbContext.Setup( m => m.RingApprovals ).Returns( TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingApproval>( ringApprovals ) ).Object );
            mockGuildDbContext.Setup( m => m.RingRequirements ).Returns( TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingRequirement>( requirements ) ).Object );
            mockGuildDbContext.Setup( m => m.RingStatuses ).Returns( TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingStatus>( relatedStatuses.Concat( unapprovedRelatedStatuses ) ) ).Object );


            // Act
            var result = await unitUnderTest.GetPendingApprovalsAsync(
                userIdForStatuses );

            // Assert
            Assert.AreEqual( 1, result.Count() );
            var resultRecord = result.First();
            Assert.AreEqual( ringApprovals.Skip( 2 ).First().Id, resultRecord.ApprovalRecordId );
            Assert.AreEqual( ringApprovals.Skip( 2 ).First().RingId, resultRecord.RingId );
            Assert.AreEqual( ringApprovals.Skip( 2 ).First().Ring.Name, resultRecord.RingName );
            Assert.AreEqual( ringApprovals.Skip( 2 ).First().Ring.ImageUploaded, resultRecord.RingImageUploaded );
            Assert.IsTrue( resultRecord.UnconfirmedRequirements.SequenceEqual( relatedRequirements.Skip( 3 ).Take( 3 ) ) );
        }

        [Test]
        public async Task GetPendingApprovalsAsync_Given_a_list_of_approval_records_Returns_the_approval_records_with_related_recently_completed_requirements()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var userIdForStatuses = USERID;
            var approvalRelatedRingId = Guid.NewGuid();
            var approvalRelatedRingId2 = Guid.NewGuid();
            var ringApprovals =_fixture.Build<RingApproval>().Without( ra => ra.ApprovedAt ).Without( ra => ra.RecalledByWarriorTs ).Without( ra => ra.ReturnedTs ).CreateMany( 8 );
            ringApprovals.Skip( 2 ).First().UserId = userIdForStatuses;
            ringApprovals.Skip( 2 ).First().RingId = approvalRelatedRingId;
            ringApprovals.Skip( 4 ).First().UserId = userIdForStatuses;
            ringApprovals.Skip( 4 ).First().RingId = approvalRelatedRingId2;
            var relatedRequirements =_fixture.Build<RingRequirement>().With( rr => rr.RingId, approvalRelatedRingId ).CreateMany( 8 );
            var relatedRequirements2 =_fixture.Build<RingRequirement>().With( rr => rr.RingId, approvalRelatedRingId2 ).CreateMany( 8 );
            var relatedStatuses = relatedRequirements.Take( 3 ).Select( rr =>_fixture.Build<RingStatus>().With( rs => rs.RingId, approvalRelatedRingId )
                                                                                                          .With( rs => rs.RingRequirementId, rr.Id )
                                                                                                          .With( rs => rs.UserId, userIdForStatuses )
                                                                                                          .Without( ra => ra.RecalledByWarriorTs ).Without( ra => ra.ReturnedTs ).Create() ).ToArray();
            var unapprovedRelatedStatuses = relatedRequirements.Skip( 3 ).Take( 3 ).Select( rr =>_fixture.Build<RingStatus>().With( rs => rs.RingId, approvalRelatedRingId )
                                                                                                            .With( rs => rs.RingRequirementId, rr.Id )
                                                                                                          .With( rs => rs.GuardianCompleted, (DateTime?)null )
                                                                                                          .With( rs => rs.UserId, userIdForStatuses )
                                                                                                          .Without( ra => ra.RecalledByWarriorTs ).Without( ra => ra.ReturnedTs ).Create() ).ToArray();
            var relatedStatuses2 = relatedRequirements2.Take( 3 ).Select( rr =>_fixture.Build<RingStatus>().With( rs => rs.RingId, approvalRelatedRingId2 )
                                                                                                          .With( rs => rs.RingRequirementId, rr.Id )
                                                                                                          .With( rs => rs.UserId, userIdForStatuses )
                                                                                                          .Without( ra => ra.RecalledByWarriorTs ).Without( ra => ra.ReturnedTs ).Create() ).ToArray();
            var unapprovedRelatedStatuses2 = relatedRequirements2.Skip( 3 ).Take( 3 ).Select( rr =>_fixture.Build<RingStatus>().With( rs => rs.RingId, approvalRelatedRingId2 )
                                                                                                            .With( rs => rs.RingRequirementId, rr.Id )
                                                                                                          .With( rs => rs.GuardianCompleted, (DateTime?)null )
                                                                                                          .With( rs => rs.UserId, userIdForStatuses )
                                                                                                          .Without( ra => ra.RecalledByWarriorTs ).Without( ra => ra.ReturnedTs ).Create() ).ToArray();
            var requirements =_fixture.Build<RingRequirement>().CreateMany( 10 ).Concat( relatedRequirements ).Concat( relatedRequirements2 );
            mockGuildDbContext.Setup( m => m.RingApprovals ).Returns( TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingApproval>( ringApprovals ) ).Object );
            mockGuildDbContext.Setup( m => m.RingRequirements ).Returns( TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingRequirement>( requirements ) ).Object );
            mockGuildDbContext.Setup( m => m.RingStatuses ).Returns( TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingStatus>( relatedStatuses.Concat( unapprovedRelatedStatuses ).Concat( relatedStatuses2 ).Concat( unapprovedRelatedStatuses2 ) ) ).Object );


            // Act
            var result = await unitUnderTest.GetPendingApprovalsAsync(
                userIdForStatuses );

            // Assert
            Assert.AreEqual( 2, result.Count() );
            var resultRecord = result.First();
            var resultRecord2 = result.Skip( 1 ).First();
            Assert.AreEqual( ringApprovals.Skip( 2 ).First().Id, resultRecord.ApprovalRecordId );
            Assert.AreEqual( ringApprovals.Skip( 2 ).First().RingId, resultRecord.RingId );
            Assert.AreEqual( ringApprovals.Skip( 2 ).First().Ring.Name, resultRecord.RingName );
            Assert.AreEqual( ringApprovals.Skip( 2 ).First().Ring.ImageUploaded, resultRecord.RingImageUploaded );
            Assert.AreEqual( ringApprovals.Skip( 4 ).First().Id, resultRecord2.ApprovalRecordId );
            Assert.AreEqual( ringApprovals.Skip( 4 ).First().RingId, resultRecord2.RingId );
            Assert.AreEqual( ringApprovals.Skip( 4 ).First().Ring.Name, resultRecord2.RingName );
            Assert.AreEqual( ringApprovals.Skip( 4 ).First().Ring.ImageUploaded, resultRecord2.RingImageUploaded );
            Assert.IsTrue( resultRecord.UnconfirmedRequirements.SequenceEqual( relatedRequirements.Skip( 3 ).Take( 3 ) ) );
            Assert.IsTrue( resultRecord2.UnconfirmedRequirements.SequenceEqual( relatedRequirements2.Skip( 3 ).Take( 3 ) ) );
        }

        [Test]
        public async Task ApproveProgressAsync_GivenRankTheRingApprovalRecordUserIsNotRelatedToCurrentUser_NothingShouldBeDoneAndTheResultShouldBeFalse()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var approvalRecordId = Guid.NewGuid();
            var userIdForStatuses = USERID;
            var myUserId = Guid.NewGuid();
            var ringApprovals =_fixture.Build<RingApproval>().CreateMany( 3 );
            var relatedApprovalRecord = ringApprovals.Skip( 2 ).First();
            relatedApprovalRecord.Id = approvalRecordId;
            relatedApprovalRecord.UserId = userIdForStatuses;

            var dbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingApproval>( ringApprovals ) );

            mockGuildDbContext.Setup( m => m.RingApprovals ).Returns( dbSet.Object );

            mockUserProvider.Setup( m => m.UserIsRelatedToWarrior( myUserId, userIdForStatuses ) ).Returns( Task.FromResult( false ) );

            // Act
            var result = await unitUnderTest.ApproveProgressAsync(
                approvalRecordId,
                myUserId );

            // Assert
            Assert.IsFalse( result.Success );
            Assert.AreEqual( "Invalid user was supplied for status update", result.Error );
        }

        [Test]
        public async Task ApproveProgressAsync_GivenRingTheRingApprovalRecordDoesNotExist_NothingShouldBeDoneAndTheResultShouldBeFalse()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var approvalRecordId = Guid.NewGuid();
            var userIdForStatuses = USERID;
            var myUserId = Guid.NewGuid();
            var ringApprovals =_fixture.Build<RingApproval>().CreateMany( 3 );
            mockGuildDbContext.Setup( m => m.RingApprovals ).Returns( TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingApproval>( ringApprovals ) ).Object );

            // Act
            var result = await unitUnderTest.ApproveProgressAsync(
                approvalRecordId,
                myUserId );

            // Assert
            Assert.IsFalse( result.Success );
            Assert.AreEqual( "The approval record was not found", result.Error );
        }

        [Test]
        public async Task ApproveProgressAsync_GivenRingTheRingApprovalRecordIsAlreadyApproved_NothingShouldBeDoneAndTheResultShouldBeFalse()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var approvalRecordId = Guid.NewGuid();
            var userIdForStatuses = USERID;
            var myUserId = Guid.NewGuid();
            var ringApprovals =_fixture.Build<RingApproval>().CreateMany( 3 );
            var relatedApprovalRecord = ringApprovals.Skip( 2 ).First();
            relatedApprovalRecord.Id = approvalRecordId;
            relatedApprovalRecord.UserId = userIdForStatuses;

            mockGuildDbContext.Setup( m => m.RingApprovals ).Returns( TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingApproval>( ringApprovals ) ).Object );

            mockUserProvider.Setup( m => m.UserIsRelatedToWarrior( myUserId, userIdForStatuses ) ).Returns( Task.FromResult( true ) );


            // Act
            var result = await unitUnderTest.ApproveProgressAsync(
                approvalRecordId,
                myUserId );

            // Assert
            Assert.IsFalse( result.Success );
            Assert.AreEqual( "This ring has already been approved", result.Error );
        }

        [Test]
        public async Task ApproveProgressAsync_GivenRingTheRingApprovalRecordIsFoundAndIsNotApproved_TheApprovalInformationShouldBeSetAndAllUnconfirmedStatusesConfirmedAndTheResultShouldBeTrue()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var approvalRecordId = Guid.NewGuid();
            var userIdForStatuses = USERID;
            var myUserId = Guid.NewGuid();

            var ringApprovals =_fixture.Build<RingApproval>().CreateMany( 3 );
            var relatedApprovalRecord = ringApprovals.Skip( 2 ).First();
            relatedApprovalRecord.Id = approvalRecordId;
            relatedApprovalRecord.ApprovedAt = null;
            relatedApprovalRecord.UserId = userIdForStatuses;

            mockUserProvider.Setup( m => m.UserIsRelatedToWarrior( myUserId, userIdForStatuses ) ).Returns( Task.FromResult( true ) );

            var requirementStatus =_fixture.Build<RingStatus>().With( rs => rs.RingId, relatedApprovalRecord.RingId ).With( rs => rs.UserId, userIdForStatuses ).CreateMany( 5 ).ToList();
            requirementStatus.Skip( 2 ).First().GuardianCompleted = null;
            requirementStatus.Skip( 2 ).First().RecalledByWarriorTs = null;
            requirementStatus.Skip( 2 ).First().ReturnedTs = null;
            requirementStatus.Skip( 3 ).First().GuardianCompleted = null;
            requirementStatus.Skip( 3 ).First().RecalledByWarriorTs = null;
            requirementStatus.Skip( 3 ).First().ReturnedTs = null;
            requirementStatus.Skip( 4 ).First().GuardianCompleted = null;
            requirementStatus.Skip( 4 ).First().RecalledByWarriorTs = null;
            requirementStatus.Skip( 4 ).First().ReturnedTs = null;
            var dummyApprovalDate = DateTime.UtcNow.AddDays( -3 );
            var dummyStatuses =_fixture.Build<RingStatus>().With( rs => rs.GuardianCompleted, dummyApprovalDate ).CreateMany( 4 );
            requirementStatus.AddRange( dummyStatuses );
            mockGuildDbContext.Setup( m => m.RingStatuses ).Returns( TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingStatus>( requirementStatus ) ).Object );
            var currentDateTime = DateTime.UtcNow;
            mockDateTimeProvider.Setup( m => m.GetCurrentDateTime() ).Returns( currentDateTime );
            mockGuildDbContext.Setup( m => m.RingApprovals ).Returns( TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingApproval>( ringApprovals ) ).Object );

            mockGuildDbContext.Setup( m => m.SaveChangesAsync() ).Returns( Task.FromResult( 1 ) );

            // Act
            var result = await unitUnderTest.ApproveProgressAsync(
                approvalRecordId,
                myUserId );

            // Assert
            Assert.IsTrue( result.Success );
            Assert.IsNull( result.Error );
            Assert.AreNotEqual( null, requirementStatus.First().GuardianCompleted );
            Assert.AreNotEqual( null, requirementStatus.Skip( 1 ).First().GuardianCompleted );
            Assert.AreNotEqual( currentDateTime, requirementStatus.First().GuardianCompleted );
            Assert.AreNotEqual( currentDateTime, requirementStatus.Skip( 1 ).First().GuardianCompleted );
            Assert.AreEqual( currentDateTime, requirementStatus.Skip( 2 ).First().GuardianCompleted );
            Assert.AreEqual( currentDateTime, requirementStatus.Skip( 3 ).First().GuardianCompleted );
            Assert.AreEqual( currentDateTime, requirementStatus.Skip( 4 ).First().GuardianCompleted );
            foreach ( var d in dummyStatuses )
            {
                Assert.AreEqual( dummyApprovalDate, d.GuardianCompleted );
            }
            Assert.AreEqual( currentDateTime, relatedApprovalRecord.ApprovedAt );
        }

        [Test]
        public async Task PostRingStatusAsync()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var ringStatus =_fixture.Build<RingStatus>().Create();
            var expectedResult =_fixture.Build<RingStatus>().Create();
            mockRingRepository.Setup( m => m.PostRingStatusAsync( ringStatus ) ).Returns( Task.FromResult( expectedResult ) );

            // Act
            var result = await unitUnderTest.PostRingStatusAsync(
                ringStatus );

            // Assert
            Assert.AreEqual( expectedResult, result );
        }

        [Test]
        public async Task DeleteRingStatusAsync()
        {
            
            var sequence = new MockSequence();
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var ringStatus =_fixture.Build<RingStatusUpdateModel>().Create();
            var ringStatuses =_fixture.Build<RingStatus>().With( rs => rs.RingId, ringStatus.RingId )
                                                                .With( rs => rs.RingRequirementId, ringStatus.RingRequirementId )
                                                                .With( rs => rs.UserId, USERID )
                                                                .CreateMany( 6 );
            ringStatuses.Skip( 2 ).First().RecalledByWarriorTs = null;
            ringStatuses.Skip( 2 ).First().ReturnedTs = null;
            ringStatuses.Skip( 2 ).First().GuardianCompleted = null;
            var ringStatusesDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingStatus>( ringStatuses ) );

            var attachments = new TestAsyncEnumerable<ProofOfCompletionAttachment>(_fixture.Build<ProofOfCompletionAttachment>()
                                                                                                                                .With( a => a.RequirementId, ringStatus.RingRequirementId )
                                                                                                                                .With( a => a.UserId, USERID ).CreateMany( 2 ) );
            var pocAtts = TestHelpers.CreateDbSetMock( attachments, true, false );

            mockGuildDbContext.Setup( d => d.RingStatuses ).Returns( ringStatusesDbSet.Object );
            mockGuildDbContext.InSequence( sequence ).Setup( m => m.ProofOfCompletionAttachments ).Returns( pocAtts.Object );
            foreach ( var poc in pocAtts.Object )
            {
                mockGuildDbContext.InSequence( sequence ).Setup( m => m.SetDeleted( poc ) );
            }
            mockGuildDbContext.Setup( m => m.SaveChangesAsync() ).Returns( Task.FromResult( 1 ) );

            // Act
            await unitUnderTest.DeleteRingStatusAsync(
                ringStatus, USERID );

            Assert.IsTrue( ringStatuses.Skip( 2 ).First().RecalledByWarriorTs.HasValue );
        }

        [Test]
        public async Task DeleteRingStatusAsync_WhenRingStatusFoundButAlreadyApproved_TheRecalledByWarriorTsShouldNotBeSet()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var ringStatus =_fixture.Build<RingStatusUpdateModel>().Create();
            var ringStatuses =_fixture.Build<RingStatus>().With( rs => rs.RingId, ringStatus.RingId )
                                                                .With( rs => rs.RingRequirementId, ringStatus.RingRequirementId )
                                                                .With( rs => rs.UserId, USERID )
                                                                .CreateMany( 6 );
            ringStatuses.Skip( 2 ).First().RecalledByWarriorTs = null;
            ringStatuses.Skip( 2 ).First().ReturnedTs = null;
            var ringStatusesDbSet = TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RingStatus>( ringStatuses ) );
            mockGuildDbContext.Setup( d => d.RingStatuses ).Returns( ringStatusesDbSet.Object );

            // Act
            var response = await unitUnderTest.DeleteRingStatusAsync(
                ringStatus, USERID );

            Assert.IsFalse( response.Success );
            Assert.AreEqual( "This requirement completion cannot be undone because it has already been approved", response.Error );
        }
    }
}
