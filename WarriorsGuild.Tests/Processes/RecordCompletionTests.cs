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
using Microsoft.Net.Http.Headers;
using WarriorsGuild.Helpers.Utilities.Files;
using WarriorsGuild.Ranks;
using WarriorsGuild.Ranks.Models.Status;
using WarriorsGuild.Storage.Models;
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
            var recordCompletion = this.CreateRecordCompletion();
            var rankToUpdate = _fixture.Build<RankStatusUpdateModel>().With( r => r.Rings, new Guid[ 0 ] ).Create();
            var dbRequirement = _fixture.Build<RankRequirement>().With( r => r.RequireCross, false ).With( r => r.RequireRing, true ).With( r => r.RequireAttachment, false ).Create();
            mockRankRequirementProvider.Setup( m => m.GetRequirementAsync( rankToUpdate.RankId, rankToUpdate.RankRequirementId ) ).Returns( Task.FromResult( dbRequirement ) );
            mockRankStatusProvider.Setup( m => m.RecordCompletionAsync( It.IsAny<RankStatusUpdateModel>(), It.IsAny<Guid>() ) ).ReturnsAsync( new RecordCompletionResponse { Success = false, Error = "This Rank requirement requires Rings" } );

            var result = await recordCompletion.RecordCompletionAsync( rankToUpdate, USER_ID );

            Assert.IsFalse( result.Success );
            Assert.AreEqual( "This Rank requirement requires Rings", result.Error );
        }

        [Test]
        public async Task RecordCompletionAsync_Given_RequireRing_and_list_of_rings_do_not_match_pendingOrApproved_Rings_Returns_BadRequest()
        {
            var recordCompletion = this.CreateRecordCompletion();
            var rankToUpdate = _fixture.Build<RankStatusUpdateModel>().With( r => r.Rings, new[] { Guid.NewGuid(), Guid.NewGuid() } ).Create();
            var dbRequirement = _fixture.Build<RankRequirement>().With( r => r.RequireCross, false ).With( r => r.RequireRing, true ).With( r => r.RequireAttachment, false ).Create();
            mockRankRequirementProvider.Setup( m => m.GetRequirementAsync( rankToUpdate.RankId, rankToUpdate.RankRequirementId ) ).Returns( Task.FromResult( dbRequirement ) );
            mockRankStatusProvider.Setup( m => m.RecordCompletionAsync( It.IsAny<RankStatusUpdateModel>(), It.IsAny<Guid>() ) ).ReturnsAsync( new RecordCompletionResponse { Success = false, Error = "One or more of the selected rings have already been used or are no longer available for assignement. Refresh the page to get fresh lists." } );

            var result = await recordCompletion.RecordCompletionAsync( rankToUpdate, USER_ID );

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

            var unassignedRings = _fixture.Build<UnassignedRingViewModel>().CreateMany( 3 ).ToList();
            unassignedRings[ 0 ].RingId = rankToUpdate.Rings.First();
            unassignedRings[ 1 ].RingId = rankToUpdate.Rings.Skip( 1 ).First();
            var recordCompletionResponse = new RecordCompletionResponse()
            {
                Error = String.Empty,
                Success = true
            };
            mockRankStatusProvider.Setup( m => m.RecordCompletionAsync( It.IsAny<RankStatusUpdateModel>(), It.IsAny<Guid>() ) ).ReturnsAsync( recordCompletionResponse );

            var result = await recordCompletion.RecordCompletionAsync( rankToUpdate, USER_ID );

            Assert.IsTrue( result.Success );
        }

        [Test]
        public async Task RecordCompletionAsync_Given_RequireCross_and_list_of_crosses_is_empty_Returns_BadRequest()
        {
            var recordCompletion = this.CreateRecordCompletion();
            var rankToUpdate = _fixture.Build<RankStatusUpdateModel>().With( r => r.Crosses, new Guid[ 0 ] ).Create();
            var dbRequirement = _fixture.Build<RankRequirement>().With( r => r.RequireCross, true ).With( r => r.RequireRing, false ).With( r => r.RequireAttachment, false ).Create();
            mockRankRequirementProvider.Setup( m => m.GetRequirementAsync( rankToUpdate.RankId, rankToUpdate.RankRequirementId ) ).Returns( Task.FromResult( dbRequirement ) );
            mockRankStatusProvider.Setup( m => m.RecordCompletionAsync( It.IsAny<RankStatusUpdateModel>(), It.IsAny<Guid>() ) ).ReturnsAsync( new RecordCompletionResponse { Success = false, Error = "This Rank requirement requires Crosses" } );

            var result = await recordCompletion.RecordCompletionAsync( rankToUpdate, USER_ID );

            Assert.IsFalse( result.Success );
            Assert.AreEqual( "This Rank requirement requires Crosses", result.Error );
        }

        [Test]
        public async Task RecordCompletionAsync_Given_RequireCross_and_list_of_crosses_do_not_match_pendingOrApproved_Crosses_Returns_BadRequest()
        {
            var recordCompletion = this.CreateRecordCompletion();
            var rankToUpdate = _fixture.Build<RankStatusUpdateModel>().With( r => r.Crosses, new[] { Guid.NewGuid(), Guid.NewGuid() } ).Create();
            var dbRequirement = _fixture.Build<RankRequirement>().With( r => r.RequireCross, true ).With( r => r.RequireRing, false ).With( r => r.RequireAttachment, false ).Create();
            mockRankRequirementProvider.Setup( m => m.GetRequirementAsync( rankToUpdate.RankId, rankToUpdate.RankRequirementId ) ).Returns( Task.FromResult( dbRequirement ) );
            mockRankStatusProvider.Setup( m => m.RecordCompletionAsync( It.IsAny<RankStatusUpdateModel>(), It.IsAny<Guid>() ) ).ReturnsAsync( new RecordCompletionResponse { Success = false, Error = "One or more of the selected crosses have already been used or are no longer available for assignement. Refresh the page to get fresh lists." } );

            var result = await recordCompletion.RecordCompletionAsync( rankToUpdate, USER_ID );

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

            var recordCompletionResponse = new RecordCompletionResponse()
            {
                Error = String.Empty,
                Success = true
            };
            mockRankStatusProvider.Setup( m => m.RecordCompletionAsync( It.IsAny<RankStatusUpdateModel>(), It.IsAny<Guid>() ) ).ReturnsAsync( recordCompletionResponse );

            var result = await recordCompletion.RecordCompletionAsync( rankToUpdate, USER_ID );

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
            var recordCompletion = this.CreateRecordCompletion();
            var rankId = Guid.NewGuid();
            var reqId = Guid.NewGuid();
            var fileData = Enumerable.Range( 0, 3 ).Select( _ => new MultipartFileData
            {
                Content = new byte[] { 1, 2, 3 },
                Extension = ".pdf",
                ContentDisposition = new ContentDispositionHeaderValue( "form-data" ) { FileName = "test.pdf" }
            } ).ToList();
            var userIdForStatuses = Guid.NewGuid();
            var req = _fixture.Create<RankRequirement>();
            var rank = _fixture.Create<Rank>();
            var attachmentGuids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            mockRankRequirementProvider.Setup( m => m.GetRequirementAsync( rankId, reqId ) ).Returns( Task.FromResult( req ) );
            mockRanksProvider.Setup( m => m.GetAsync( req.RankId ) ).Returns( Task.FromResult( rank ) );
            mockRpHelpers.Setup( m => m.AllPreviousRanksComplete( rankId, userIdForStatuses ) ).Returns( Task.FromResult( true ) );
            mockRankStatusProvider.Setup( m => m.UploadProofOfCompletionAsync( It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>() ) ).ReturnsAsync( new FileUploadResult() );
            var guidQueue = new Queue<Guid>( attachmentGuids );
            mockRankStatusProvider.Setup( m => m.RecordProofOfCompletionDocumentAsync( req.Id, userIdForStatuses, It.IsAny<string>(), It.IsAny<string>() ) ).Returns( () => Task.FromResult( guidQueue.Dequeue() ) );

            var result = await recordCompletion.UploadAttachmentsForRankReq( rankId, reqId, fileData, userIdForStatuses );

            Assert.IsTrue( attachmentGuids.SequenceEqual( result ) );
        }
    }
}
