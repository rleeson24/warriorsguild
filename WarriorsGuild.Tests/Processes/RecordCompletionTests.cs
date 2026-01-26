using AutoFixture;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarriorsGuild.Crosses;
using WarriorsGuild.Crosses.Crosses.Status;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.Helpers.Utilities.Files;
using WarriorsGuild.Ranks;
using WarriorsGuild.Ranks.Models.Status;
using WarriorsGuild.Rings;
using WarriorsGuild.Rings.ViewModels;

namespace WarriorsGuild.Tests.Processes
{
    [TestFixture]
    public class RecordCompletionTests
    {
        protected Fixture _fixture = new Fixture();
        private readonly Guid USER_ID;
        private MockRepository mockRepository;

        private Mock<IRanksProvider> mockRanksProvider;
        private Mock<IRanksProviderHelpers> mockRpHelpers;
        private Mock<IRingsProvider> mockRingsProvider;
        private Mock<ICrossProvider> mockCrossProvider;
        private Mock<IRankStatusProvider> mockRankStatusProvider;
        private Mock<IRankRequirementProvider> mockRankRequirementProvider;

        public RecordCompletionTests()
        {
            USER_ID = Guid.NewGuid();
        }

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository( MockBehavior.Strict );

            this.mockRanksProvider = this.mockRepository.Create<IRanksProvider>();
            this.mockRpHelpers = this.mockRepository.Create<IRanksProviderHelpers>();
            this.mockRingsProvider = this.mockRepository.Create<IRingsProvider>();
            this.mockCrossProvider = this.mockRepository.Create<ICrossProvider>();
            this.mockRankStatusProvider = this.mockRepository.Create<IRankStatusProvider>();
            this.mockRankRequirementProvider = this.mockRepository.Create<IRankRequirementProvider>();
        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        private RecordCompletion CreateRecordCompletion()
        {
            return new RecordCompletion(
                this.mockRankStatusProvider.Object,
                this.mockRpHelpers.Object,
                mockRankRequirementProvider.Object,
                mockRanksProvider.Object );
        }

        [Test]
        public async Task RecordCompletionAsync_Given_RequireRing_and_list_of_rings_is_empty_Returns_BadRequest()
        {
            // Arrange
            var recordCompletion = this.CreateRecordCompletion();
            
            var rankToUpdate =_fixture.Build<RankStatusUpdateModel>().With( r => r.Rings, new Guid[ 0 ] ).Create();
            var dbRequirement =_fixture.Build<RankRequirement>().With( r => r.RequireCross, false ).With( r => r.RequireRing, true ).With( r => r.RequireAttachment, false ).Create();
            mockRankRequirementProvider.Setup( m => m.GetRequirementAsync( rankToUpdate.RankId, rankToUpdate.RankRequirementId ) ).Returns( Task.FromResult( dbRequirement ) );

            // Act
            var result = await recordCompletion.RecordCompletionAsync(
                rankToUpdate, USER_ID );

            // Assert
            Assert.IsFalse( result.Success );
            Assert.AreEqual( "This Rank requirement requires Rings", result.Error );
        }

        [Test]
        public async Task RecordCompletionAsync_Given_RequireRing_and_list_of_rings_do_not_match_pendingOrApproved_Rings_Returns_BadRequest()
        {
            var recordCompletion = this.CreateRecordCompletion();
            
            var rankToUpdate =_fixture.Build<RankStatusUpdateModel>().With( r => r.Rings, new[] { Guid.NewGuid(), Guid.NewGuid() } ).Create();

            var dbRequirement =_fixture.Build<RankRequirement>().With( r => r.RequireCross, false ).With( r => r.RequireRing, true ).With( r => r.RequireAttachment, false ).Create();
            mockRankRequirementProvider.Setup( m => m.GetRequirementAsync( rankToUpdate.RankId, rankToUpdate.RankRequirementId ) ).Returns( Task.FromResult( dbRequirement ) );

            var unassignedRings =_fixture.Build<UnassignedRingViewModel>().CreateMany( 3 );
            mockRingsProvider.Setup( m => m.GetUnassignedPendingOrApproved( USER_ID ) ).Returns( Task.FromResult( unassignedRings ) );

            // Act
            var result = await recordCompletion.RecordCompletionAsync(
                rankToUpdate, USER_ID );

            // Assert
            Assert.IsFalse( result.Success );
            Assert.AreEqual( "One or more of the selected rings have already been used or are no longer available for assignement. Refresh the page to get fresh lists.", result.Error );
        }

        [Test]
        public async Task RecordCompletionAsync_Given_RequireRing_and_list_of_rings_matches_pendingOrApproved_Rings_Returns_Success()
        {
            var recordCompletion = this.CreateRecordCompletion();
            
            var rankToUpdate =_fixture.Build<RankStatusUpdateModel>().With( r => r.Rings, new[] { Guid.NewGuid(), Guid.NewGuid() } ).Create();

            var dbRequirement =_fixture.Build<RankRequirement>().With( r => r.RequireCross, false ).With( r => r.RequireRing, true ).With( r => r.RequireAttachment, false ).Create();
            mockRankRequirementProvider.Setup( m => m.GetRequirementAsync( rankToUpdate.RankId, rankToUpdate.RankRequirementId ) ).Returns( Task.FromResult( dbRequirement ) );

            var unassignedRings =_fixture.Build<UnassignedRingViewModel>().CreateMany( 3 );
            unassignedRings.ElementAt( 0 ).RingId = rankToUpdate.Rings.First();
            unassignedRings.ElementAt( 3 ).RingId = rankToUpdate.Rings.Skip( 1 ).First();
            mockRingsProvider.Setup( m => m.GetUnassignedPendingOrApproved( USER_ID ) ).Returns( Task.FromResult( unassignedRings ) );
            var recordCompletionResponse = new RecordCompletionResponse()
            {
                Error = String.Empty,
                Success = true
            };
            mockRankStatusProvider.Setup( m => m.RecordCompletionAsync( rankToUpdate, USER_ID ) ).Returns( Task.FromResult( recordCompletionResponse ) );

            // Act
            var result = await recordCompletion.RecordCompletionAsync(
                rankToUpdate, USER_ID );

            // Assert
            Assert.IsTrue( result.Success );
        }

        [Test]
        public async Task RecordCompletionAsync_Given_RequireCross_and_list_of_crosses_is_empty_Returns_BadRequest()
        {
            var recordCompletion = this.CreateRecordCompletion();
            
            var rankToUpdate =_fixture.Build<RankStatusUpdateModel>().With( r => r.Crosses, new Guid[ 0 ] ).Create();
            var dbRequirement =_fixture.Build<RankRequirement>().With( r => r.RequireCross, true ).With( r => r.RequireRing, false ).With( r => r.RequireAttachment, false ).Create();
            mockRankRequirementProvider.Setup( m => m.GetRequirementAsync( rankToUpdate.RankId, rankToUpdate.RankRequirementId ) ).Returns( Task.FromResult( dbRequirement ) );

            // Act
            var result = await recordCompletion.RecordCompletionAsync(
                rankToUpdate, USER_ID );

            // Assert
            Assert.IsFalse( result.Success );
            Assert.AreEqual( "This Rank requirement requires Crosses", result.Error );
        }

        [Test]
        public async Task RecordCompletionAsync_Given_RequireCross_and_list_of_crosses_do_not_match_pendingOrApproved_Crosses_Returns_BadRequest()
        {
            var recordCompletion = this.CreateRecordCompletion();
            
            var rankToUpdate =_fixture.Build<RankStatusUpdateModel>().With( r => r.Crosses, new[] { Guid.NewGuid(), Guid.NewGuid() } ).Create();

            var dbRequirement =_fixture.Build<RankRequirement>().With( r => r.RequireCross, true ).With( r => r.RequireRing, false ).With( r => r.RequireAttachment, false ).Create();
            mockRankRequirementProvider.Setup( m => m.GetRequirementAsync( rankToUpdate.RankId, rankToUpdate.RankRequirementId ) ).Returns( Task.FromResult( dbRequirement ) );
            var unassignedCrosses =_fixture.Build<UnassignedCrossViewModel>().CreateMany( 3 );
            mockCrossProvider.Setup( m => m.GetUnassignedPendingOrApproved( USER_ID ) ).Returns( Task.FromResult( unassignedCrosses ) );

            // Act
            var result = await recordCompletion.RecordCompletionAsync(
                rankToUpdate, USER_ID );

            // Assert
            Assert.IsFalse( result.Success );
            Assert.AreEqual( "One or more of the selected crosses have already been used or are no longer available for assignement. Refresh the page to get fresh lists.", result.Error );
        }

        [Test]
        public async Task RecordCompletionAsync_Given_RequireCross_and_list_of_crosss_matches_pendingOrApproved_Crosses_Returns_Success()
        {
            var recordCompletion = this.CreateRecordCompletion();
            
            var rankToUpdate =_fixture.Build<RankStatusUpdateModel>().With( r => r.Crosses, new[] { Guid.NewGuid(), Guid.NewGuid() } ).Create();

            var dbRequirement =_fixture.Build<RankRequirement>().With( r => r.RequireCross, true ).With( r => r.RequireRing, false ).With( r => r.RequireAttachment, false ).Create();
            mockRankRequirementProvider.Setup( m => m.GetRequirementAsync( rankToUpdate.RankId, rankToUpdate.RankRequirementId ) ).Returns( Task.FromResult( dbRequirement ) );

            var unassignedCrosss =_fixture.Build<UnassignedCrossViewModel>().CreateMany( 3 );
            unassignedCrosss.First().CrossId = rankToUpdate.Crosses.First();
            unassignedCrosss.Skip( 2 ).First().CrossId = rankToUpdate.Crosses.Skip( 1 ).First();
            mockCrossProvider.Setup( m => m.GetUnassignedPendingOrApproved( USER_ID ) ).Returns( Task.FromResult( unassignedCrosss ) );
            var recordCompletionResponse = new RecordCompletionResponse()
            {
                Error = String.Empty,
                Success = true
            };
            mockRankStatusProvider.Setup( m => m.RecordCompletionAsync( rankToUpdate, USER_ID ) ).Returns( Task.FromResult( recordCompletionResponse ) );

            // Act
            var result = await recordCompletion.RecordCompletionAsync(
                rankToUpdate, USER_ID );

            // Assert
            Assert.IsTrue( result.Success );
        }

        //      [Test]
        //public async Task RecordCompletionAsync_StateUnderTest_ExpectedBehavior1()
        //{
        //	// Arrange
        //	var recordCompletion = this.CreateRecordCompletion();
        //	RingStatusUpdateModel rankToUpdate = null;
        //	Guid userIdForStatus = default( global::System.Guid );

        //	// Act
        //	var result = await recordCompletion.RecordCompletionAsync(
        //		rankToUpdate,
        //		userIdForStatus );

        //	// Assert
        //	Assert.Fail();
        //}

        [Test]
        public async Task UploadAttachments_WhenPreviousRanksAreNotApproved_ShouldReturnAnEmptyResultList()
        {
            // Arrange
            
            var recordCompletion = this.CreateRecordCompletion();
            var rankId = Guid.NewGuid();
            var reqId = Guid.NewGuid();
            IEnumerable<MultipartFileData> fileData = null;
            var userIdForStatuses = Guid.NewGuid();
            var req =_fixture.Create<RankRequirement>();
            var rank =_fixture.Create<Rank>();
            mockRankRequirementProvider.Setup( m => m.GetRequirementAsync( rankId, reqId ) ).Returns( Task.FromResult( req ) );
            mockRanksProvider.Setup( m => m.GetAsync( req.RankId ) ).Returns( Task.FromResult( rank ) );
            mockRpHelpers.Setup( m => m.AllPreviousRanksComplete( rankId, userIdForStatuses ) ).Returns( Task.FromResult( false ) );

            // Act
            var result = await recordCompletion.UploadAttachmentsForRankReq(
                rankId,
                reqId,
                fileData,
                userIdForStatuses );

            // Assert
            Assert.IsTrue( result.Count() == 0 );
        }

        [Test]
        public async Task UploadAttachments_WhenUploadingFiles_ShouldReturnAResultListWithTheAttachmentGuids()
        {
            // Arrange
            
            var recordCompletion = this.CreateRecordCompletion();
            var rankId = Guid.NewGuid();
            var reqId = Guid.NewGuid();
            var fileData =_fixture.CreateMany<MultipartFileData>( 3 );
            var userIdForStatuses = Guid.NewGuid();
            var req =_fixture.Create<RankRequirement>();
            var rank =_fixture.Create<Rank>();
            mockRankRequirementProvider.Setup( m => m.GetRequirementAsync( rankId, reqId ) ).Returns( Task.FromResult( req ) );
            mockRanksProvider.Setup( m => m.GetAsync( req.RankId ) ).Returns( Task.FromResult( rank ) );
            mockRpHelpers.Setup( m => m.AllPreviousRanksComplete( rankId, userIdForStatuses ) ).Returns( Task.FromResult( true ) );

            var attachmentGuids = new List<Guid>();
            var i = 0;
            //foreach ( var fd in fileData )
            //{
            //    var fileName = $"{rank.Name}_{req.Id.ToString()}_{userIdForStatuses}_{i}";
            //    var fileUploadResult =_fixture.Create<WarriorsGuild.Providers.FileStorage.FileUploadResult>();
            //    mockRanksProvider.Setup( m => m.UploadProofOfCompletionAsync( fileName, fd.FileExtension, fd.FileContent, fd.MediaType ) ).Returns( Task.FromResult( fileUploadResult ) );
            //    var attachmentGuid = Guid.NewGuid();
            //    mockRanksProvider.Setup( m => m.RecordProofOfCompletionDocumentAsync( req.Id, userIdForStatuses, fileName, fd.FileExtension ) ).Returns( Task.FromResult( attachmentGuid ) );
            //    attachmentGuids.Add( attachmentGuid );
            //    i++;
            //}

            // Act
            var result = await recordCompletion.UploadAttachmentsForRankReq(
                rankId,
                reqId,
                fileData,
                userIdForStatuses );

            // Assert
            Assert.IsTrue( attachmentGuids.SequenceEqual( result ) );
        }
    }
}
