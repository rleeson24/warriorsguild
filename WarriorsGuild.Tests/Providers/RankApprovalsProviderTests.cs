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
    public class RankApprovalsProviderTests
    {
        protected Fixture _fixture = new Fixture();
        private MockRepository mockRepository;

        private Guid USERID = Guid.NewGuid();

        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IAccountRepository> mockAccountRepository;
        private Mock<IRankRepository> mockRankRepository;
        private Mock<IRankMapper> mockRankMapper;
        private Mock<IDateTimeProvider> mockDateTimeProvider;
        private Mock<IRanksProviderHelpers> mockRpHelpers;
        private Mock<IUserProvider> mockUserProvider;
        private Mock<ILogger<RanksProvider>> mockLogger;
        private Mock<IEmailProvider> mockEmailProvider;
        private Mock<IHttpContextAccessor> mockHttpContextAccessor;
        private Mock<IRankRequirementProvider> mockRankRequirementProvider;
        private Mock<IRankStatusProvider> mockRankStatusProvider;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository( MockBehavior.Strict );

            this.mockUnitOfWork = this.mockRepository.Create<IUnitOfWork>();
            this.mockAccountRepository = this.mockRepository.Create<IAccountRepository>();
            this.mockRankRepository = this.mockRepository.Create<IRankRepository>();
            this.mockRankMapper = this.mockRepository.Create<IRankMapper>();
            this.mockUserProvider = this.mockRepository.Create<IUserProvider>();
            this.mockDateTimeProvider = this.mockRepository.Create<IDateTimeProvider>();
            this.mockRpHelpers = this.mockRepository.Create<IRanksProviderHelpers>();
            this.mockLogger = this.mockRepository.Create<ILogger<RanksProvider>>();
            this.mockEmailProvider = this.mockRepository.Create<IEmailProvider>();
            this.mockHttpContextAccessor = this.mockRepository.Create<IHttpContextAccessor>();
            this.mockRankRequirementProvider = this.mockRepository.Create<IRankRequirementProvider>();
            this.mockRankStatusProvider = this.mockRepository.Create<IRankStatusProvider>();
        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        private RankApprovalsProvider CreateProvider()
        {
            return new RankApprovalsProvider(
                this.mockUnitOfWork.Object,
                this.mockRankRepository.Object,
                this.mockAccountRepository.Object,
                this.mockRankMapper.Object,
                this.mockDateTimeProvider.Object,
                this.mockRpHelpers.Object,
                this.mockUserProvider.Object,
                this.mockRankStatusProvider.Object,
                this.mockLogger.Object,
                this.mockEmailProvider.Object,
                this.mockHttpContextAccessor.Object);
        }

        public async Task NotifyGuardiansOfRequestForPromotion()
        {
            var unitUnderTest = this.CreateProvider();
            var rank =_fixture.Create<Rank>();
            var totalCompleted = 65;
            await unitUnderTest.NotifyGuardiansOfRequestForPromotion( rank, totalCompleted, USERID );
            Assert.Fail();
            //mockEmailProvider.Verify(m => m.SendAsync()
        }

        [Test]
        public async Task GetPendingApprovalsAsync_Given_no_approval_records_Returns_an_empty_list()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var userIdForStatuses = USERID;
            mockRankRepository.Setup( m => m.GetPendingRankApprovalsWithRankAsync( userIdForStatuses ) ).ReturnsAsync( Array.Empty<RankApproval>() );

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
            mockRankRepository.Setup( m => m.GetPendingRankApprovalsWithRankAsync( userIdForStatuses ) ).ReturnsAsync( Array.Empty<RankApproval>() );

            // Act
            var result = await unitUnderTest.GetPendingApprovalsAsync(
                userIdForStatuses );

            // Assert
            Assert.IsFalse( result.Any() );
        }

        [Test]
        public async Task GetPendingApprovalsAsync_Given_an_unapproved_approval_record_Returns_the_approval_record_with_recently_completed_requirements_and_associated_Rings()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var userIdForStatuses = USERID;
            var approvalRelatedRankId = Guid.NewGuid();
            var rankApprovals =_fixture.Build<RankApproval>().Without( ra => ra.ApprovedAt ).Without( ra => ra.RecalledByWarriorTs ).Without( ra => ra.ReturnedTs ).CreateMany( 3 );
            rankApprovals.Skip( 2 ).First().UserId = userIdForStatuses;
            rankApprovals.Skip( 2 ).First().RankId = approvalRelatedRankId;
            var relatedRequirements =_fixture.Build<RankRequirement>().With( rr => rr.RankId, approvalRelatedRankId ).CreateMany( 8 );
            var relatedStatuses = relatedRequirements.Take( 3 ).Select( rr =>_fixture.Build<RankStatus>().With( rs => rs.RankId, approvalRelatedRankId )
                                                                                                          .With( rs => rs.RankRequirementId, rr.Id )
                                                                                                          .With( rs => rs.UserId, userIdForStatuses )
                                                                                                          .Without( ra => ra.RecalledByWarriorTs ).Without( ra => ra.ReturnedTs ).Create() ).ToArray();
            var unapprovedRelatedStatuses = relatedRequirements.Skip( 3 ).Take( 3 ).Select( rr =>_fixture.Build<RankStatus>().With( rs => rs.RankId, approvalRelatedRankId )
                                                                                                            .With( rs => rs.RankRequirementId, rr.Id )
                                                                                                          .With( rs => rs.GuardianCompleted, (DateTime?)null )
                                                                                                          .With( rs => rs.UserId, userIdForStatuses )
                                                                                                          .Without( ra => ra.RecalledByWarriorTs ).Without( ra => ra.ReturnedTs ).Create() ).ToArray();
            var approvalRecord = rankApprovals.Skip( 2 ).First();
            var rank = _fixture.Build<Rank>().With( r => r.Id, approvalRelatedRankId ).Create();
            approvalRecord.Rank = rank;
            approvalRecord.RankId = approvalRelatedRankId;
            mockRankRepository.Setup( m => m.GetPendingRankApprovalsWithRankAsync( userIdForStatuses ) ).ReturnsAsync( new[] { approvalRecord } );
            var pendingReqs = relatedRequirements.Skip( 3 ).Take( 3 ).ToArray();
            mockRankRepository.Setup( m => m.GetPendingRequirementsWithStatusForApprovalAsync( approvalRelatedRankId, userIdForStatuses ) ).ReturnsAsync( (pendingReqs, unapprovedRelatedStatuses) );
            var i = 0;
            var reqViewModels = new List<RankRequirementViewModel>();
            foreach ( var a in relatedRequirements.Skip( 3 ).Take( 3 ) )
            {
                IEnumerable<MinimalRingDetail> associatedRings = null;
                if ( i % 2 == 0 )
                {
                    a.RequireRing = true;
                    associatedRings =_fixture.Build<MinimalRingDetail>().CreateMany( 2 );
                    mockRankStatusProvider.Setup( m => m.GetRingsForRankStatus( a.RankId, a.Id, userIdForStatuses ) ).Returns( Task.FromResult( associatedRings ) );
                }
                else
                {
                    a.RequireRing = false;
                }
                a.RequireCross = false;
                a.RequireAttachment = false;
                var relatedStatus = unapprovedRelatedStatuses.Single( s => s.RankRequirementId == a.Id );
                var viewModel =_fixture.Build<RankRequirementViewModel>().Create();
                reqViewModels.Add( viewModel );
                mockRankMapper.Setup( m => m.CreateRequirementViewModel( a, relatedStatus.WarriorCompleted, null, associatedRings ?? Array.Empty<MinimalRingDetail>(), It.Is<IEnumerable<MinimalCrossDetail>>( _ => _.Any() == false ), It.Is<IEnumerable<MinimalGoalDetail>>( _ => _.Any() == false ) ) ).Returns( viewModel );
            }

            // Act
            var result = await unitUnderTest.GetPendingApprovalsAsync(
                userIdForStatuses );

            // Assert
            Assert.AreEqual( 1, result.Count() );
            var resultRecord = result.First();
            Assert.AreEqual( approvalRecord.Id, resultRecord.ApprovalRecordId );
            Assert.AreEqual( approvalRecord.PercentComplete, resultRecord.PercentComplete );
            Assert.AreEqual( approvalRecord.RankId, resultRecord.RankId );
            Assert.AreEqual( approvalRecord.Rank.Name, resultRecord.RankName );
            Assert.AreEqual( approvalRecord.Rank.ImageUploaded, resultRecord.RankImageUploaded );
            Assert.IsTrue( resultRecord.UnconfirmedRequirements.SequenceEqual( reqViewModels ) );
        }

        [Test]
        public async Task GetPendingApprovalsAsync_Given_an_unapproved_approval_record_Returns_the_approval_record_with_recently_completed_requirements_and_associated_Crosses()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var userIdForStatuses = USERID;
            var approvalRelatedRankId = Guid.NewGuid();
            var rankApprovals =_fixture.Build<RankApproval>().With( ra => ra.ApprovedAt, (DateTime?)null ).With( ra => ra.RecalledByWarriorTs, (DateTime?)null ).With( ra => ra.ReturnedTs, (DateTime?)null ).CreateMany( 3 );
            rankApprovals.Skip( 2 ).First().UserId = userIdForStatuses;
            rankApprovals.Skip( 2 ).First().RankId = approvalRelatedRankId;
            var relatedRequirements =_fixture.Build<RankRequirement>().With( rr => rr.RankId, approvalRelatedRankId ).CreateMany( 8 );
            var relatedStatuses = relatedRequirements.Take( 3 ).Select( rr =>_fixture.Build<RankStatus>().With( rs => rs.RankId, approvalRelatedRankId )
                                                                                                          .With( rs => rs.RankRequirementId, rr.Id )
                                                                                                          .With( rs => rs.UserId, userIdForStatuses )
                                                                                                          .With( ra => ra.RecalledByWarriorTs, (DateTime?)null ).With( ra => ra.ReturnedTs, (DateTime?)null ).Create() ).ToArray();
            var unapprovedRelatedStatuses = relatedRequirements.Skip( 3 ).Take( 3 ).Select( rr =>_fixture.Build<RankStatus>().With( rs => rs.RankId, approvalRelatedRankId )
                                                                                                            .With( rs => rs.RankRequirementId, rr.Id )
                                                                                                          .With( rs => rs.GuardianCompleted, (DateTime?)null )
                                                                                                          .With( rs => rs.UserId, userIdForStatuses )
                                                                                                          .With( ra => ra.RecalledByWarriorTs, (DateTime?)null ).With( ra => ra.ReturnedTs, (DateTime?)null ).Create() ).ToArray();
            var approvalRecord = rankApprovals.Skip( 2 ).First();
            var rank = _fixture.Build<Rank>().With( r => r.Id, approvalRelatedRankId ).Create();
            approvalRecord.Rank = rank;
            approvalRecord.RankId = approvalRelatedRankId;
            mockRankRepository.Setup( m => m.GetPendingRankApprovalsWithRankAsync( userIdForStatuses ) ).ReturnsAsync( new[] { approvalRecord } );
            var pendingReqs = relatedRequirements.Skip( 3 ).Take( 3 ).ToArray();
            mockRankRepository.Setup( m => m.GetPendingRequirementsWithStatusForApprovalAsync( approvalRelatedRankId, userIdForStatuses ) ).ReturnsAsync( (pendingReqs, unapprovedRelatedStatuses) );
            var i = 0;
            var reqViewModels = new List<RankRequirementViewModel>();
            foreach ( var a in relatedRequirements.Skip( 3 ).Take( 3 ) )
            {
                IEnumerable<MinimalCrossDetail> associatedCrosses = null;
                if ( i % 2 == 0 )
                {
                    a.RequireCross = true;
                    associatedCrosses =_fixture.Build<MinimalCrossDetail>().CreateMany( 2 );
                    mockRankStatusProvider.Setup( m => m.GetCrossesForRankStatus( a.RankId, a.Id, userIdForStatuses ) ).Returns( Task.FromResult( associatedCrosses ) );
                }
                else
                {
                    a.RequireCross = false;
                }
                a.RequireRing = false;
                a.RequireAttachment = false;
                var relatedStatus = unapprovedRelatedStatuses.Single( s => s.RankRequirementId == a.Id );
                var viewModel =_fixture.Build<RankRequirementViewModel>().Create();
                reqViewModels.Add( viewModel );
                mockRankMapper.Setup( m => m.CreateRequirementViewModel( a, relatedStatus.WarriorCompleted, null, It.Is<IEnumerable<MinimalRingDetail>>( _ => _.Any() == false ), associatedCrosses ?? Array.Empty<MinimalCrossDetail>(), It.Is<IEnumerable<MinimalGoalDetail>>( _ => _.Any() == false ) ) ).Returns( viewModel );
            }

            // Act
            var result = await unitUnderTest.GetPendingApprovalsAsync(
                userIdForStatuses );

            // Assert
            Assert.AreEqual( 1, result.Count() );
            var resultRecord = result.First();
            Assert.AreEqual( approvalRecord.Id, resultRecord.ApprovalRecordId );
            Assert.AreEqual( approvalRecord.PercentComplete, resultRecord.PercentComplete );
            Assert.AreEqual( approvalRecord.RankId, resultRecord.RankId );
            Assert.AreEqual( approvalRecord.Rank.Name, resultRecord.RankName );
            Assert.AreEqual( approvalRecord.Rank.ImageUploaded, resultRecord.RankImageUploaded );
            Assert.IsTrue( resultRecord.UnconfirmedRequirements.SequenceEqual( reqViewModels ) );
        }

        [Test]
        public async Task GetPendingApprovalsAsync_Given_an_unapproved_approval_record_Returns_the_approval_record_with_recently_completed_requirements_and_associated_Attachments()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var userIdForStatuses = USERID;
            var approvalRelatedRankId = Guid.NewGuid();
            var rankApprovals =_fixture.Build<RankApproval>().With( ra => ra.ApprovedAt, (DateTime?)null ).With( ra => ra.RecalledByWarriorTs, (DateTime?)null ).With( ra => ra.ReturnedTs, (DateTime?)null ).CreateMany( 3 );
            rankApprovals.Skip( 2 ).First().UserId = userIdForStatuses;
            rankApprovals.Skip( 2 ).First().RankId = approvalRelatedRankId;
            var relatedRequirements =_fixture.Build<RankRequirement>().With( rr => rr.RankId, approvalRelatedRankId ).CreateMany( 8 );
            var relatedStatuses = relatedRequirements.Take( 3 ).Select( rr =>_fixture.Build<RankStatus>().With( rs => rs.RankId, approvalRelatedRankId )
                                                                                                          .With( rs => rs.RankRequirementId, rr.Id )
                                                                                                          .With( rs => rs.UserId, userIdForStatuses )
                                                                                                          .With( ra => ra.RecalledByWarriorTs, (DateTime?)null ).With( ra => ra.ReturnedTs, (DateTime?)null ).Create() ).ToArray();
            var unapprovedRelatedStatuses = relatedRequirements.Skip( 3 ).Take( 3 ).Select( rr =>_fixture.Build<RankStatus>().With( rs => rs.RankId, approvalRelatedRankId )
                                                                                                            .With( rs => rs.RankRequirementId, rr.Id )
                                                                                                          .With( rs => rs.GuardianCompleted, (DateTime?)null )
                                                                                                          .With( rs => rs.UserId, userIdForStatuses )
                                                                                                          .With( ra => ra.RecalledByWarriorTs, (DateTime?)null ).With( ra => ra.ReturnedTs, (DateTime?)null ).Create() ).ToArray();
            var approvalRecordAtt = rankApprovals.Skip( 2 ).First();
            var rankAtt = _fixture.Build<Rank>().With( r => r.Id, approvalRelatedRankId ).Create();
            approvalRecordAtt.Rank = rankAtt;
            approvalRecordAtt.RankId = approvalRelatedRankId;
            mockRankRepository.Setup( m => m.GetPendingRankApprovalsWithRankAsync( userIdForStatuses ) ).ReturnsAsync( new[] { approvalRecordAtt } );
            var pendingReqsAtt = relatedRequirements.Skip( 3 ).Take( 3 ).ToArray();
            mockRankRepository.Setup( m => m.GetPendingRequirementsWithStatusForApprovalAsync( approvalRelatedRankId, userIdForStatuses ) ).ReturnsAsync( (pendingReqsAtt, unapprovedRelatedStatuses) );
            var i = 0;
            var reqViewModels = new List<RankRequirementViewModel>();
            foreach ( var a in relatedRequirements.Skip( 3 ).Take( 3 ) )
            {
                IEnumerable<MinimalGoalDetail> associatedAttachments = null;
                if ( i % 2 == 0 )
                {
                    a.RequireAttachment = true;
                    associatedAttachments =_fixture.Build<MinimalAttachmentDetail>().CreateMany( 2 );
                    mockRankStatusProvider.Setup( m => m.GetAttachmentsForRankStatus( a.Id, userIdForStatuses ) ).Returns( Task.FromResult( associatedAttachments ) );
                }
                else
                {
                    a.RequireAttachment = false;
                }
                a.RequireRing = false;
                a.RequireCross = false;
                var relatedStatus = unapprovedRelatedStatuses.Single( s => s.RankRequirementId == a.Id );
                var viewModel =_fixture.Build<RankRequirementViewModel>().Create();
                reqViewModels.Add( viewModel );
                mockRankMapper.Setup( m => m.CreateRequirementViewModel( a, relatedStatus.WarriorCompleted, null, It.Is<IEnumerable<MinimalRingDetail>>( _ => _.Any() == false ), It.Is<IEnumerable<MinimalCrossDetail>>( _ => _.Any() == false ), associatedAttachments ?? Array.Empty<MinimalGoalDetail>() ) ).Returns( viewModel );
            }

            // Act
            var result = await unitUnderTest.GetPendingApprovalsAsync(
                userIdForStatuses );

            // Assert
            Assert.AreEqual( 1, result.Count() );
            var resultRecord = result.First();
            Assert.AreEqual( approvalRecordAtt.Id, resultRecord.ApprovalRecordId );
            Assert.AreEqual( approvalRecordAtt.PercentComplete, resultRecord.PercentComplete );
            Assert.AreEqual( approvalRecordAtt.RankId, resultRecord.RankId );
            Assert.AreEqual( approvalRecordAtt.Rank.Name, resultRecord.RankName );
            Assert.AreEqual( approvalRecordAtt.Rank.ImageUploaded, resultRecord.RankImageUploaded );
            Assert.IsTrue( resultRecord.UnconfirmedRequirements.SequenceEqual( reqViewModels ) );
        }

        //[Test]
        //public async Task GetPendingApprovalsAsync_Given_a_list_of_approval_records_Returns_the_approval_records_with_related_recently_completed_requirements()
        //{
        //	
        //	// Arrange
        //	var unitUnderTest = this.CreateProvider();
        //	var userIdForStatuses = USERID;
        //	var approvalRelatedRankId = Guid.NewGuid();
        //	var approvalRelatedRankId2 = Guid.NewGuid();
        //	var rankApprovals =_fixture.Build<RankApproval>().With( ra => ra.ApprovedAt, (DateTime?)null ).With( ra => ra.RecalledByWarriorTs, (DateTime?)null ).With( ra => ra.ReturnedTs, (DateTime?)null ).CreateMany( 8 );
        //	rankApprovals.Skip( 2 ).First().UserId = userIdForStatuses;
        //	rankApprovals.Skip( 2 ).First().RankId = approvalRelatedRankId;
        //	rankApprovals.Skip( 4 ).First().UserId = userIdForStatuses;
        //	rankApprovals.Skip( 4 ).First().RankId = approvalRelatedRankId2;
        //	var relatedRequirements =_fixture.Build<RankRequirement>().With( rr => rr.RankId, approvalRelatedRankId ).CreateMany( 8 );
        //	var relatedRequirements2 =_fixture.Build<RankRequirement>().With( rr => rr.RankId, approvalRelatedRankId2 ).CreateMany( 8 );
        //	var relatedStatuses = relatedRequirements.Take( 3 ).Select( rr =>_fixture.Build<RankStatus>().With( rs => rs.RankId, approvalRelatedRankId )
        //																								  .With( rs => rs.RankRequirementId, rr.Id )
        //																								  .With( rs => rs.UserId, userIdForStatuses ).Create() ).ToArray();
        //	var unapprovedRelatedStatuses = relatedRequirements.Skip( 3 ).Take( 3 ).Select( rr =>_fixture.Build<RankStatus>().With( rs => rs.RankId, approvalRelatedRankId )
        //																									.With( rs => rs.RankRequirementId, rr.Id )
        //																								  .With( rs => rs.GuardianCompleted, (DateTime?)null )
        //																								  .With( rs => rs.UserId, userIdForStatuses )
        //																								  .With( ra => ra.RecalledByWarriorTs, (DateTime?)null ).With( ra => ra.ReturnedTs, (DateTime?)null ).Create() ).ToArray();
        //	var relatedStatuses2 = relatedRequirements2.Take( 3 ).Select( rr =>_fixture.Build<RankStatus>().With( rs => rs.RankId, approvalRelatedRankId2 )
        //																								  .With( rs => rs.RankRequirementId, rr.Id )
        //																								  .With( rs => rs.UserId, userIdForStatuses )
        //																								  .With( ra => ra.RecalledByWarriorTs, (DateTime?)null ).With( ra => ra.ReturnedTs, (DateTime?)null ).Create() ).ToArray();
        //	var unapprovedRelatedStatuses2 = relatedRequirements2.Skip( 3 ).Take( 3 ).Select( rr =>_fixture.Build<RankStatus>().With( rs => rs.RankId, approvalRelatedRankId2 )
        //																									.With( rs => rs.RankRequirementId, rr.Id )
        //																								  .With( rs => rs.GuardianCompleted, (DateTime?)null )
        //																								  .With( rs => rs.UserId, userIdForStatuses )
        //																								  .With( ra => ra.RecalledByWarriorTs, (DateTime?)null ).With( ra => ra.ReturnedTs, (DateTime?)null ).Create() ).ToArray();
        //	var requirements =_fixture.Build<RankRequirement>().CreateMany( 10 ).Concat( relatedRequirements ).Concat( relatedRequirements2 );
        //	mockGuildDbContext.Setup( m => m.RankApprovals ).Returns( TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankApproval>( rankApprovals ) ).Object );
        //	mockGuildDbContext.Setup( m => m.RankRequirements ).Returns( TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankRequirement>( requirements ) ).Object );
        //	mockGuildDbContext.Setup( m => m.RankStatuses ).Returns( TestHelpers.CreateDbSetMock( new TestAsyncEnumerable<RankStatus>( relatedStatuses.Concat( unapprovedRelatedStatuses ).Concat( relatedStatuses2 ).Concat( unapprovedRelatedStatuses2 ) ) ).Object );

        //	var reqViewModels = new List<RankRequirementViewModel>();
        //	foreach ( var a in relatedRequirements2.Skip( 3 ).Take( 3 ) )
        //	{
        //		IEnumerable<MinimalGoalDetail> associatedRings = null;
        //		if ( i % 2 == 0 )
        //		{
        //			a.RequireRing = true;
        //			associatedRings =_fixture.Build<MinimalGoalDetail>().CreateMany( 2 );
        //			mockRpHelpers.Setup( m => m.GetRingsForRankStatusAsync( a.RankId, a.Id, userIdForStatuses ) ).Returns( Task.FromResult( associatedRings ) );
        //		}
        //		else
        //		{
        //			a.RequireRing = false;
        //		}
        //		var relatedStatus = unapprovedRelatedStatuses.Single( s => s.RankRequirementId == a.Id );
        //		var viewModel =_fixture.Build<RankRequirementViewModel>().Create();
        //		reqViewModels.Add( viewModel );
        //		mockRankMapper.Setup( m => m.CreateRequirementViewModel( a, relatedStatus.WarriorCompleted, null, associatedRings, null ) ).Returns( viewModel );
        //	}

        //	// Act
        //	var result = await unitUnderTest.GetPendingApprovalsAsync(
        //		userIdForStatuses );

        //	// Assert
        //	Assert.AreEqual( 2, result.Count() );
        //	var resultRecord = result.First();
        //	var resultRecord2 = result.Skip( 1 ).First();
        //	Assert.AreEqual( rankApprovals.Skip( 2 ).First().Id, resultRecord.ApprovalRecordId );
        //	Assert.AreEqual( rankApprovals.Skip( 2 ).First().PercentComplete, resultRecord.PercentComplete );
        //	Assert.AreEqual( rankApprovals.Skip( 2 ).First().RankId, resultRecord.RankId );
        //	Assert.AreEqual( rankApprovals.Skip( 2 ).First().Rank.Name, resultRecord.RankName );
        //	Assert.AreEqual( rankApprovals.Skip( 2 ).First().Rank.ImageUploaded, resultRecord.RankImageUploaded );
        //	Assert.AreEqual( rankApprovals.Skip( 4 ).First().Id, resultRecord2.ApprovalRecordId );
        //	Assert.AreEqual( rankApprovals.Skip( 4 ).First().PercentComplete, resultRecord2.PercentComplete );
        //	Assert.AreEqual( rankApprovals.Skip( 4 ).First().RankId, resultRecord2.RankId );
        //	Assert.AreEqual( rankApprovals.Skip( 4 ).First().Rank.Name, resultRecord2.RankName );
        //	Assert.AreEqual( rankApprovals.Skip( 4 ).First().Rank.ImageUploaded, resultRecord2.RankImageUploaded );
        //	Assert.IsTrue( resultRecord.UnconfirmedRequirements.SequenceEqual( relatedRequirements.Skip( 3 ).Take( 3 ) ) );
        //	Assert.IsTrue( resultRecord2.UnconfirmedRequirements.SequenceEqual( relatedRequirements2.Skip( 3 ).Take( 3 ) ) );
        //}

        [Test]
        public async Task ApproveProgressAsync_GivenRankTheRankApprovalRecordUserIsNotRelatedToCurrentUser_NothingShouldBeDoneAndTheResultShouldBeFalse()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var approvalRecordId = Guid.NewGuid();
            var userIdForStatuses = USERID;
            var myUserId = Guid.NewGuid();
            var rankApprovals =_fixture.Build<RankApproval>().CreateMany( 3 );
            var relatedApprovalRecord = rankApprovals.Skip( 2 ).First();
            relatedApprovalRecord.Id = approvalRecordId;
            relatedApprovalRecord.UserId = userIdForStatuses;

            mockRankRepository.Setup( m => m.GetRankApprovalByIdAsync( approvalRecordId ) ).ReturnsAsync( relatedApprovalRecord );

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
        public async Task ApproveProgressAsync_GivenRankTheRankApprovalRecordDoesNotExist_NothingShouldBeDoneAndTheResultShouldBeFalse()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var approvalRecordId = Guid.NewGuid();
            var userIdForStatuses = USERID;
            mockRankRepository.Setup( m => m.GetRankApprovalByIdAsync( approvalRecordId ) ).ReturnsAsync( (RankApproval)null );

            // Act
            var result = await unitUnderTest.ApproveProgressAsync(
                approvalRecordId,
                userIdForStatuses );

            // Assert
            Assert.IsFalse( result.Success );
            Assert.AreEqual( "The approval record was not found", result.Error );
        }

        [Test]
        public async Task ApproveProgressAsync_GivenTheRankApprovalRecordCorrespondingToTheUserDoesNotExist_NothingShouldBeDoneAndTheResultShouldBeFalse()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var approvalRecordId = Guid.NewGuid();
            var userIdForStatuses = USERID;
            var rankApprovals =_fixture.Build<RankApproval>().CreateMany( 3 );
            var relatedApprovalRecord = rankApprovals.Skip( 2 ).First();
            relatedApprovalRecord.Id = Guid.NewGuid();
            mockRankRepository.Setup( m => m.GetRankApprovalByIdAsync( approvalRecordId ) ).ReturnsAsync( (RankApproval)null );

            // Act
            var result = await unitUnderTest.ApproveProgressAsync(
                approvalRecordId,
                userIdForStatuses );

            // Assert
            Assert.IsFalse( result.Success );
            Assert.AreEqual( "The approval record was not found", result.Error );
        }

        [Test]
        public async Task ApproveProgressAsync_GivenRankTheRankApprovalRecordIsAlreadyApproved_NothingShouldBeDoneAndTheResultShouldBeFalse()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var approvalRecordId = Guid.NewGuid();
            var userIdForStatuses = USERID;
            var myUserId = Guid.NewGuid();
            var rankApprovals =_fixture.Build<RankApproval>().CreateMany( 3 );
            var relatedApprovalRecord = rankApprovals.Skip( 2 ).First();
            relatedApprovalRecord.Id = approvalRecordId;
            relatedApprovalRecord.UserId = userIdForStatuses;
            relatedApprovalRecord.ApprovedAt = DateTime.UtcNow;

            mockRankRepository.Setup( m => m.GetRankApprovalByIdAsync( approvalRecordId ) ).ReturnsAsync( relatedApprovalRecord );
            mockUserProvider.Setup( m => m.UserIsRelatedToWarrior( myUserId, userIdForStatuses ) ).Returns( Task.FromResult( true ) );
            // Act
            var result = await unitUnderTest.ApproveProgressAsync(
                approvalRecordId,
                myUserId );

            // Assert
            Assert.IsFalse( result.Success );
            Assert.AreEqual( "This stage has already been approved", result.Error );
        }

        [Test]
        public async Task ApproveProgressAsync_GivenRankTheRankApprovalRecordIsFoundAndIsNotApproved_AndAllRelatedRingsAndCrossesAreApproved_TheApprovalInformationShouldBeSetAndAllUnconfirmedStatusesConfirmedAndTheResultShouldBeTrue()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var approvalRecordId = Guid.NewGuid();
            var userIdForStatuses = USERID;
            var myUserId = Guid.NewGuid();
            var rankApprovals =_fixture.Build<RankApproval>().CreateMany( 3 );
            var relatedApprovalRecord = rankApprovals.Skip( 2 ).First();
            relatedApprovalRecord.Id = approvalRecordId;
            relatedApprovalRecord.ApprovedAt = null;
            relatedApprovalRecord.UserId = userIdForStatuses;
            mockRankRepository.Setup( m => m.GetRankApprovalByIdAsync( approvalRecordId ) ).ReturnsAsync( relatedApprovalRecord );

            var requirementStatus =_fixture.Build<RankStatus>().With( rs => rs.RankId, relatedApprovalRecord.RankId ).With( rs => rs.UserId, userIdForStatuses ).CreateMany( 2 ).ToList();
            var unapprovedRequirements =_fixture.Build<RankStatus>()
                                                .With( rs => rs.RankId, relatedApprovalRecord.RankId )
                                                .With( rs => rs.UserId, userIdForStatuses )
                                                .With( rs => rs.GuardianCompleted, (DateTime?)null )
                                                .With( rs => rs.RecalledByWarriorTs, (DateTime?)null )
                                                .With( rs => rs.ReturnedTs, (DateTime?)null )
                                                .CreateMany( 3 );
            var dummyApprovalDate = DateTime.UtcNow.AddDays( -3 );
            var dummyStatuses =_fixture.Build<RankStatus>().With( rs => rs.GuardianCompleted, dummyApprovalDate ).CreateMany( 4 );
            requirementStatus.AddRange( unapprovedRequirements );
            requirementStatus.AddRange( dummyStatuses );
            var unapprovedStatuses = requirementStatus.Skip( 2 ).Take( 3 ).ToArray();
            mockRankRepository.Setup( m => m.GetUnapprovedRankStatusesAsync( relatedApprovalRecord.RankId, relatedApprovalRecord.UserId ) ).ReturnsAsync( unapprovedStatuses );

            var currentDateTime = DateTime.UtcNow;
            mockDateTimeProvider.Setup( m => m.GetCurrentDateTime() ).Returns( currentDateTime );

            mockUserProvider.Setup( m => m.UserIsRelatedToWarrior( myUserId, userIdForStatuses ) ).Returns( Task.FromResult( true ) );

            mockRpHelpers.Setup( m => m.AllAssociatedRingsAndCrossesAreCompletedAndApprovedAsync( relatedApprovalRecord.RankId,
                                                                                                    It.Is<IEnumerable<Guid>>( arg => unapprovedRequirements.Select( s => s.RankRequirementId ).SequenceEqual( arg ) ),
                                                                                                    relatedApprovalRecord.UserId ) )
                        .Returns( Task.FromResult( true ) );
            mockUnitOfWork.Setup( m => m.SaveChangesAsync() ).ReturnsAsync( 1 );

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
        public async Task ApproveProgressAsync_GivenRankTheRankApprovalRecordIsFoundAndIsNotApproved_AndNotAllRelatedRingsAndCrossesAreApproved_TheApprovalInformationShouldBeSetAndAllUnconfirmedStatusesConfirmedAndTheResultShouldBeTrue()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var approvalRecordId = Guid.NewGuid();
            var userIdForStatuses = USERID;
            var myUserId = Guid.NewGuid();
            var rankApprovals =_fixture.Build<RankApproval>().CreateMany( 3 );
            var relatedApprovalRecord = rankApprovals.Skip( 2 ).First();
            relatedApprovalRecord.Id = approvalRecordId;
            relatedApprovalRecord.ApprovedAt = null;
            relatedApprovalRecord.UserId = userIdForStatuses;
            mockRankRepository.Setup( m => m.GetRankApprovalByIdAsync( approvalRecordId ) ).ReturnsAsync( relatedApprovalRecord );

            var requirementStatus =_fixture.Build<RankStatus>().With( rs => rs.RankId, relatedApprovalRecord.RankId ).With( rs => rs.UserId, userIdForStatuses ).CreateMany( 2 ).ToList();
            var unapprovedRequirements =_fixture.Build<RankStatus>()
                                                .With( rs => rs.RankId, relatedApprovalRecord.RankId )
                                                .With( rs => rs.UserId, userIdForStatuses )
                                                .With( rs => rs.GuardianCompleted, (DateTime?)null )
                                                .With( rs => rs.RecalledByWarriorTs, (DateTime?)null )
                                                .With( rs => rs.ReturnedTs, (DateTime?)null )
                                                .CreateMany( 3 );
            var unapprovedStatusesList = requirementStatus.Skip( 2 ).Take( 3 ).ToArray();
            mockRankRepository.Setup( m => m.GetUnapprovedRankStatusesAsync( relatedApprovalRecord.RankId, relatedApprovalRecord.UserId ) ).ReturnsAsync( unapprovedStatusesList );

            mockUserProvider.Setup( m => m.UserIsRelatedToWarrior( myUserId, userIdForStatuses ) ).Returns( Task.FromResult( true ) );

            mockRpHelpers.Setup( m => m.AllAssociatedRingsAndCrossesAreCompletedAndApprovedAsync( relatedApprovalRecord.RankId,
                                                                                                    It.Is<IEnumerable<Guid>>( arg => unapprovedRequirements.Select( s => s.RankRequirementId ).SequenceEqual( arg ) ),
                                                                                                    relatedApprovalRecord.UserId ) )
                        .Returns( Task.FromResult( false ) );

            // Act
            var result = await unitUnderTest.ApproveProgressAsync(
                approvalRecordId,
                myUserId );

            // Assert
            Assert.IsFalse( result.Success );
            Assert.AreEqual( "All Crosses and Rings must be approved before you can approve the promotion", result.Error );
        }
    }
}
