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
            // Arrange - pending approval (ApprovedAt=null) blocks adding new status
            var unitUnderTest = this.CreateProvider();
            var ringForStatus = _fixture.Build<RingStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            var pendingApproval = _fixture.Build<RingApproval>().With( r => r.RingId, ringForStatus.RingId ).With( r => r.UserId, userIdForStatuses ).With( r => r.ApprovedAt, (DateTime?)null ).With( r => r.RecalledByWarriorTs, (DateTime?)null ).With( r => r.ReturnedTs, (DateTime?)null ).Create();
            mockRingRepository.Setup( m => m.GetRequirementStatusAsync( ringForStatus.RingId, ringForStatus.RingRequirementId, userIdForStatuses ) ).Returns( Task.FromResult( (RingStatus)null ) );
            mockRingRepository.Setup( m => m.GetLatestPendingOrApprovedApprovalForRingAsync( ringForStatus.RingId, userIdForStatuses ) ).Returns( Task.FromResult( pendingApproval ) );

            // Act
            var result = await unitUnderTest.RecordCompletionAsync( ringForStatus, userIdForStatuses );

            // Assert
            Assert.IsFalse( result.Success );
            Assert.AreEqual( "There is currently a pending approval record.  Your Guardian must approve it before you can continue.", result.Error );
        }

        [Test]
        public async Task RecordCompletionAsync_NoApprovalRecordFound_AndDifferenceBetweenTotalPercentCompletedAndLastApprovedPercentIsLessThan30_StatusShouldBeUpdated_AndTheResponseSuccessFlagShouldBeTrue()
        {
            // Arrange - no approval record means we can add status
            var unitUnderTest = this.CreateProvider();
            var ringForStatus = _fixture.Build<RingStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            mockRingRepository.Setup( m => m.GetRequirementStatusAsync( ringForStatus.RingId, ringForStatus.RingRequirementId, userIdForStatuses ) ).Returns( Task.FromResult( (RingStatus)null ) );
            mockRingRepository.Setup( m => m.GetLatestPendingOrApprovedApprovalForRingAsync( ringForStatus.RingId, userIdForStatuses ) ).Returns( Task.FromResult( (RingApproval)null ) );
            var dummyDateTime = DateTime.UtcNow;
            mockDateTimeProvider.Setup( m => m.GetCurrentDateTime() ).Returns( dummyDateTime );
            var newRingStatus = _fixture.Build<RingStatus>().Create();
            mockRingMapper.Setup( m => m.CreateRingStatus( ringForStatus.RingId, ringForStatus.RingRequirementId, dummyDateTime, null, userIdForStatuses ) ).Returns( newRingStatus );
            RingStatus capturedStatus = null;
            mockRingRepository.Setup( m => m.PostRingStatusAsync( It.IsAny<RingStatus>() ) ).Callback<RingStatus>( s => capturedStatus = s ).Returns<RingStatus>( s => Task.FromResult( s ) );

            // Act
            var result = await unitUnderTest.RecordCompletionAsync( ringForStatus, userIdForStatuses );

            // Assert
            Assert.IsTrue( result.Success );
            Assert.AreSame( newRingStatus, capturedStatus );
        }

        [Test]
        public async Task RecordCompletionAsync_NoApprovalRecordFound_AndNoStatusFoundForAnyRequirement_StatusShouldBeUpdated_AndTheResponseSuccessFlagShouldBeTrue()
        {
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var ringForStatus = _fixture.Build<RingStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            mockRingRepository.Setup( m => m.GetRequirementStatusAsync( ringForStatus.RingId, ringForStatus.RingRequirementId, userIdForStatuses ) ).Returns( Task.FromResult( (RingStatus)null ) );
            mockRingRepository.Setup( m => m.GetLatestPendingOrApprovedApprovalForRingAsync( ringForStatus.RingId, userIdForStatuses ) ).Returns( Task.FromResult( (RingApproval)null ) );
            var dummyDateTime = DateTime.UtcNow;
            mockDateTimeProvider.Setup( m => m.GetCurrentDateTime() ).Returns( dummyDateTime );
            var newRingStatus = _fixture.Build<RingStatus>().Create();
            mockRingMapper.Setup( m => m.CreateRingStatus( ringForStatus.RingId, ringForStatus.RingRequirementId, dummyDateTime, null, userIdForStatuses ) ).Returns( newRingStatus );
            RingStatus capturedStatus = null;
            mockRingRepository.Setup( m => m.PostRingStatusAsync( It.IsAny<RingStatus>() ) ).Callback<RingStatus>( s => capturedStatus = s ).Returns<RingStatus>( s => Task.FromResult( s ) );

            // Act
            var result = await unitUnderTest.RecordCompletionAsync( ringForStatus, userIdForStatuses );

            // Assert
            Assert.IsTrue( result.Success );
            Assert.AreSame( newRingStatus, capturedStatus );
        }

        [Test]
        public async Task RecordCompletionAsync_ApprovedApprovalRecordFoundAndDifferenceBetweenTotalPercentCompletedAndLastApprovedPercentIsLessThan30_StatusShouldBeUpdated_AndNoApprovalRecordCreated_AndTheResponseSuccessFlagShouldBeTrue()
        {
            // Arrange - approved record means we can add more status
            var unitUnderTest = this.CreateProvider();
            var ringForStatus = _fixture.Build<RingStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            var approvedRecord = _fixture.Build<RingApproval>().With( r => r.RingId, ringForStatus.RingId ).With( r => r.UserId, userIdForStatuses ).With( r => r.ApprovedAt, DateTime.UtcNow ).Create();
            mockRingRepository.Setup( m => m.GetRequirementStatusAsync( ringForStatus.RingId, ringForStatus.RingRequirementId, userIdForStatuses ) ).Returns( Task.FromResult( (RingStatus)null ) );
            mockRingRepository.Setup( m => m.GetLatestPendingOrApprovedApprovalForRingAsync( ringForStatus.RingId, userIdForStatuses ) ).Returns( Task.FromResult( approvedRecord ) );
            var dummyDateTime = DateTime.UtcNow;
            mockDateTimeProvider.Setup( m => m.GetCurrentDateTime() ).Returns( dummyDateTime );
            var newRingStatus = _fixture.Build<RingStatus>().Create();
            mockRingMapper.Setup( m => m.CreateRingStatus( ringForStatus.RingId, ringForStatus.RingRequirementId, dummyDateTime, null, userIdForStatuses ) ).Returns( newRingStatus );
            RingStatus capturedStatus = null;
            mockRingRepository.Setup( m => m.PostRingStatusAsync( It.IsAny<RingStatus>() ) ).Callback<RingStatus>( s => capturedStatus = s ).Returns<RingStatus>( s => Task.FromResult( s ) );

            // Act
            var result = await unitUnderTest.RecordCompletionAsync( ringForStatus, userIdForStatuses );

            // Assert
            Assert.IsTrue( result.Success );
            Assert.AreSame( newRingStatus, capturedStatus );
            mockRingRepository.Verify( m => m.AddApprovalEntry( It.IsAny<RingApproval>() ), Times.Never );
        }

        [Test]
        public async Task RecordCompletionAsync_PercentCompletedIs100_StatusShouldBeUpdated_AndTheResponseSuccessFlagShouldBeTrue()
        {
            // Arrange - approved record (100% complete scenario)
            var unitUnderTest = this.CreateProvider();
            var ringForStatus = _fixture.Build<RingStatusUpdateModel>().Create();
            var userIdForStatuses = USERID;
            var approvedRecord = _fixture.Build<RingApproval>().With( r => r.RingId, ringForStatus.RingId ).With( r => r.UserId, userIdForStatuses ).With( r => r.ApprovedAt, DateTime.UtcNow ).Create();
            mockRingRepository.Setup( m => m.GetRequirementStatusAsync( ringForStatus.RingId, ringForStatus.RingRequirementId, userIdForStatuses ) ).Returns( Task.FromResult( (RingStatus)null ) );
            mockRingRepository.Setup( m => m.GetLatestPendingOrApprovedApprovalForRingAsync( ringForStatus.RingId, userIdForStatuses ) ).Returns( Task.FromResult( approvedRecord ) );
            var dummyDateTime = DateTime.UtcNow;
            mockDateTimeProvider.Setup( m => m.GetCurrentDateTime() ).Returns( dummyDateTime );
            var newRingStatus = _fixture.Build<RingStatus>().With( rs => rs.RingId, ringForStatus.RingId ).Create();
            mockRingMapper.Setup( m => m.CreateRingStatus( ringForStatus.RingId, ringForStatus.RingRequirementId, dummyDateTime, null, userIdForStatuses ) ).Returns( newRingStatus );
            RingStatus capturedStatus = null;
            mockRingRepository.Setup( m => m.PostRingStatusAsync( It.IsAny<RingStatus>() ) ).Callback<RingStatus>( s => capturedStatus = s ).Returns<RingStatus>( s => Task.FromResult( s ) );

            // Act
            var result = await unitUnderTest.RecordCompletionAsync( ringForStatus, userIdForStatuses );

            // Assert
            Assert.IsTrue( result.Success );
            Assert.AreSame( newRingStatus, capturedStatus );
        }

        [Test]
        public async Task GetPendingApprovalsAsync_Given_no_approval_records_Returns_an_empty_list()
        {
            // Arrange - repo returns empty (no records matching: !ApprovedAt, !Recalled, !Returned)
            var unitUnderTest = this.CreateProvider();
            var userIdForStatuses = USERID;
            mockRingRepository.Setup( m => m.GetPendingApprovalDetailsAsync( userIdForStatuses ) ).Returns( Task.FromResult( Enumerable.Empty<PendingApprovalDetail>() ) );

            // Act
            var result = await unitUnderTest.GetPendingApprovalsAsync( userIdForStatuses );

            // Assert
            Assert.IsFalse( result.Any() );
        }

        [Test]
        public async Task GetPendingApprovalsAsync_Given_no_unapproved_approval_records_Returns_an_empty_list()
        {
            // Arrange - repo returns empty (all records have ApprovedAt set)
            var unitUnderTest = this.CreateProvider();
            var userIdForStatuses = USERID;
            mockRingRepository.Setup( m => m.GetPendingApprovalDetailsAsync( userIdForStatuses ) ).Returns( Task.FromResult( Enumerable.Empty<PendingApprovalDetail>() ) );

            // Act
            var result = await unitUnderTest.GetPendingApprovalsAsync( userIdForStatuses );

            // Assert
            Assert.IsFalse( result.Any() );
        }

        [Test]
        public async Task GetPendingApprovalsAsync_Given_an_unapproved_approval_record_Returns_the_approval_record_with_recently_completed_requirements()
        {
            // Arrange - repo returns pre-computed PendingApprovalDetail
            var unitUnderTest = this.CreateProvider();
            var userIdForStatuses = USERID;
            var approvalRelatedRingId = Guid.NewGuid();
            var approvalId = Guid.NewGuid();
            var relatedRequirements = _fixture.Build<RingRequirement>().With( rr => rr.RingId, approvalRelatedRingId ).CreateMany( 8 ).ToArray();
            var unconfirmedReqs = relatedRequirements.Skip( 3 ).Take( 3 );
            var ring = _fixture.Build<Ring>().With( r => r.Id, approvalRelatedRingId ).Create();
            var expectedDetail = new PendingApprovalDetail
            {
                ApprovalRecordId = approvalId,
                RingId = approvalRelatedRingId,
                RingName = ring.Name,
                RingImageUploaded = ring.ImageUploaded,
                WarriorCompleted = DateTime.UtcNow,
                ImageExtension = ring.ImageExtension,
                UnconfirmedRequirements = unconfirmedReqs
            };
            mockRingRepository.Setup( m => m.GetPendingApprovalDetailsAsync( userIdForStatuses ) ).Returns( Task.FromResult( new[] { expectedDetail }.AsEnumerable() ) );

            // Act
            var result = await unitUnderTest.GetPendingApprovalsAsync( userIdForStatuses );

            // Assert
            Assert.AreEqual( 1, result.Count() );
            var resultRecord = result.First();
            Assert.AreEqual( approvalId, resultRecord.ApprovalRecordId );
            Assert.AreEqual( approvalRelatedRingId, resultRecord.RingId );
            Assert.AreEqual( ring.Name, resultRecord.RingName );
            Assert.AreEqual( ring.ImageUploaded, resultRecord.RingImageUploaded );
            Assert.IsTrue( resultRecord.UnconfirmedRequirements.SequenceEqual( unconfirmedReqs ) );
        }

        [Test]
        public async Task GetPendingApprovalsAsync_Given_a_list_of_approval_records_Returns_the_approval_records_with_related_recently_completed_requirements()
        {
            // Arrange - repo returns pre-computed PendingApprovalDetail list
            var unitUnderTest = this.CreateProvider();
            var userIdForStatuses = USERID;
            var approvalRelatedRingId = Guid.NewGuid();
            var approvalRelatedRingId2 = Guid.NewGuid();
            var approvalId1 = Guid.NewGuid();
            var approvalId2 = Guid.NewGuid();
            var relatedRequirements = _fixture.Build<RingRequirement>().With( rr => rr.RingId, approvalRelatedRingId ).CreateMany( 8 ).ToArray();
            var relatedRequirements2 = _fixture.Build<RingRequirement>().With( rr => rr.RingId, approvalRelatedRingId2 ).CreateMany( 8 ).ToArray();
            var ring1 = _fixture.Build<Ring>().With( r => r.Id, approvalRelatedRingId ).Create();
            var ring2 = _fixture.Build<Ring>().With( r => r.Id, approvalRelatedRingId2 ).Create();
            var expectedDetails = new[]
            {
                new PendingApprovalDetail { ApprovalRecordId = approvalId1, RingId = approvalRelatedRingId, RingName = ring1.Name, RingImageUploaded = ring1.ImageUploaded, WarriorCompleted = DateTime.UtcNow, ImageExtension = ring1.ImageExtension, UnconfirmedRequirements = relatedRequirements.Skip( 3 ).Take( 3 ) },
                new PendingApprovalDetail { ApprovalRecordId = approvalId2, RingId = approvalRelatedRingId2, RingName = ring2.Name, RingImageUploaded = ring2.ImageUploaded, WarriorCompleted = DateTime.UtcNow, ImageExtension = ring2.ImageExtension, UnconfirmedRequirements = relatedRequirements2.Skip( 3 ).Take( 3 ) }
            };
            mockRingRepository.Setup( m => m.GetPendingApprovalDetailsAsync( userIdForStatuses ) ).Returns( Task.FromResult( expectedDetails.AsEnumerable() ) );

            // Act
            var result = await unitUnderTest.GetPendingApprovalsAsync( userIdForStatuses );

            // Assert
            Assert.AreEqual( 2, result.Count() );
            var resultRecord = result.First();
            var resultRecord2 = result.Skip( 1 ).First();
            Assert.AreEqual( approvalId1, resultRecord.ApprovalRecordId );
            Assert.AreEqual( approvalRelatedRingId, resultRecord.RingId );
            Assert.AreEqual( ring1.Name, resultRecord.RingName );
            Assert.AreEqual( ring1.ImageUploaded, resultRecord.RingImageUploaded );
            Assert.AreEqual( approvalId2, resultRecord2.ApprovalRecordId );
            Assert.AreEqual( approvalRelatedRingId2, resultRecord2.RingId );
            Assert.AreEqual( ring2.Name, resultRecord2.RingName );
            Assert.AreEqual( ring2.ImageUploaded, resultRecord2.RingImageUploaded );
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
