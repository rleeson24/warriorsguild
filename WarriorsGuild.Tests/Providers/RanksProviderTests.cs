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
    public class RanksProviderTests
    {
        protected Fixture _fixture = new Fixture();
        private MockRepository mockRepository;

        private Guid USERID = Guid.NewGuid();

        private Mock<IGuildDbContext> mockGuildDbContext;
        private Mock<IRankRepository> mockRankRepository;
        private Mock<IRankMapper> mockRankMapper;
        private Mock<IBlobProvider> mockAttachmentProvider;
        
        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository( MockBehavior.Strict );

            this.mockGuildDbContext = this.mockRepository.Create<IGuildDbContext>();
            this.mockRankRepository = this.mockRepository.Create<IRankRepository>();
            this.mockRankMapper = this.mockRepository.Create<IRankMapper>();
            this.mockAttachmentProvider = this.mockRepository.Create<IBlobProvider>();
        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        private RanksProvider CreateProvider()
        {
            return new RanksProvider(
                this.mockGuildDbContext.Object,
                this.mockRankRepository.Object,
                this.mockRankMapper.Object,
                this.mockAttachmentProvider.Object);
        }

        [Test]
        public async Task GetListAsync()
        {
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var userIdForStatuses = USERID;
            var expectedList = _fixture.Build<Rank>().CreateMany( 3 ).ToList();
            mockRankRepository.Setup( m => m.List() ).Returns( CreateAsyncQueryable( expectedList ) );

            // Act
            var result = await unitUnderTest.GetListAsync( userIdForStatuses );

            // Assert
            Assert.IsTrue( expectedList.SequenceEqual( result ) );
        }

        [Test]
        public async Task GetPublicRankAsync()
        {
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var expectedList = _fixture.Build<Rank>().CreateMany( 1 ).ToList();
            mockRankRepository.Setup( m => m.List() ).Returns( CreateAsyncQueryable( expectedList ) );

            // Act
            var result = await unitUnderTest.GetPublicAsync();

            // Assert
            Assert.AreEqual( expectedList.Single(), result );
        }

        [Test]
        public async Task GetAsync()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var id = Guid.NewGuid();
            String userIdForStatuses = USERID.ToString();
            var expectedRank =_fixture.Build<Rank>().Create();
            mockRankRepository.Setup( m => m.Get( id, false ) ).Returns( expectedRank );

            // Act
            var result = await unitUnderTest.GetAsync(
                id );

            // Assert
            Assert.AreEqual( expectedRank, result );
        }

        [Test]
        public async Task GetMyRankAsync_NothingComplete_ShouldReturnNullCompletedAndFirstRankAsWorking()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var userIdForStatuses = USERID;
            var highestRankWithCompletions = (Rank)null;
            int workingRankIndex = 1;
            var rankByIndexResult = new Rank() { Index = 1 };
            int totalPercentCompleted = 0;
            var workingRankViewModel = new RankViewModel();
            mockRankRepository.Setup( m => m.GetHighestRankWithCompletionsAsync( USERID ) ).Returns( Task.FromResult<Rank>( highestRankWithCompletions ) );
            mockRankRepository.Setup( m => m.GetRankByIndexAsync( workingRankIndex, USERID ) ).Returns( Task.FromResult<Rank>( rankByIndexResult ) );
            mockRankMapper.Setup( m => m.MapToRankViewModel( rankByIndexResult, totalPercentCompleted ) ).Returns( workingRankViewModel );

            // Act
            var result = await unitUnderTest.GetCurrentRankAsync( userIdForStatuses );

            // Assert
            Assert.AreEqual( workingRankViewModel, result.WorkingRank );
        }

        [Test]
        public async Task GetMyRankAsync_PartiallyCompletedFirstRank_ShouldReturnFirstRankCompletedAndFirstRankAsWorking()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var userIdForStatuses = USERID;
            var highestRankWithCompletions =_fixture.Build<Rank>().Create();
            highestRankWithCompletions.Requirements =_fixture.Build<RankRequirement>()
                                                                .With( rr => rr.Weight, 10 )
                                                                .With( rr => rr.RankId, highestRankWithCompletions.Id )
                                                                .CreateMany( 10 ).ToList();
            var firstReq = highestRankWithCompletions.Requirements.First();
            highestRankWithCompletions.Statuses = new List<RankStatus>() {
                       _fixture.Build<RankStatus>()
                                                        .With( rs => rs.RankId, highestRankWithCompletions.Id )
                                                        .With( rs => rs.RankRequirementId, firstReq.Id )
                                                        .With( rs => rs.RecalledByWarriorTs, (DateTime?)null )
                                                        .With( rs => rs.ReturnedTs, (DateTime?)null )
                                                        .Create()
            };
            mockRankRepository.Setup( m => m.GetHighestRankWithCompletionsAsync( USERID ) ).Returns( Task.FromResult<Rank>( highestRankWithCompletions ) );
            var totalPercentCompleted = highestRankWithCompletions.Requirements.Where( r => highestRankWithCompletions.Statuses.Where( s => s.GuardianCompleted.HasValue ).Any( s => s.RankRequirementId == r.Id ) ).Sum( rr => rr.Weight );
            var workingRankViewModel =_fixture.Build<RankViewModel>().Create();
            var rankViewModel =_fixture.Build<RankViewModel>().Create();
            mockRankMapper.Setup( m => m.MapToRankViewModel( highestRankWithCompletions, totalPercentCompleted ) ).Returns( rankViewModel );

            // Act
            var result = await unitUnderTest.GetCurrentRankAsync( userIdForStatuses );

            // Assert
            Assert.AreEqual( totalPercentCompleted, result.CompletedCompletionPercentage );
            Assert.AreEqual( totalPercentCompleted, result.WorkingCompletionPercentage );
            Assert.AreSame( rankViewModel, result.WorkingRank );
            Assert.AreSame( rankViewModel, result.CompletedRank );
        }

        [Test]
        public async Task GetMyRankAsync_FullyCompletedRank_ShouldReturnHighestFullyCompletedRankAsCompletedAndNextRankAsWorking()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var userIdForStatuses = USERID;
            var workingRankPercentageComplete = 0;
            var highestRankWithCompletions =_fixture.Build<Rank>().Create();
            highestRankWithCompletions.Requirements =_fixture.Build<RankRequirement>()
                                                                .With( rr => rr.Weight, 10 )
                                                                .With( rr => rr.RankId, highestRankWithCompletions.Id )
                                                                .CreateMany( 10 ).ToList();
            var firstReq = highestRankWithCompletions.Requirements.First();
            highestRankWithCompletions.Statuses = highestRankWithCompletions.Requirements.Select( rr =>
                       _fixture.Build<RankStatus>()
                                                        .With( rs => rs.RankId, highestRankWithCompletions.Id )
                                                        .With( rs => rs.RankRequirementId, rr.Id )
                                                        .With( rs => rs.RecalledByWarriorTs, (DateTime?)null )
                                                        .With( rs => rs.ReturnedTs, (DateTime?)null )
                                                        .Create()
            ).ToList();
            mockRankRepository.Setup( m => m.GetHighestRankWithCompletionsAsync( USERID ) ).Returns( Task.FromResult<Rank>( highestRankWithCompletions ) );
            var totalPercentCompleted = highestRankWithCompletions.Requirements.Where( r => highestRankWithCompletions.Statuses.Where( s => s.GuardianCompleted.HasValue ).Any( s => s.RankRequirementId == r.Id ) ).Sum( rr => rr.Weight );

            Assert.AreEqual( 100, totalPercentCompleted );

            var completedRankViewModel =_fixture.Build<RankViewModel>().With( r => r.Index, highestRankWithCompletions.Index ).Create();
            var nextRank =_fixture.Build<Rank>().Create();
            nextRank.Statuses.Clear();
            var workingRankViewModel =_fixture.Build<RankViewModel>().With( r => r.Index, nextRank.Index ).Create();
            mockRankMapper.Setup( m => m.MapToRankViewModel( highestRankWithCompletions, totalPercentCompleted ) ).Returns( completedRankViewModel );
            mockRankRepository.Setup( m => m.GetRankByIndexAsync( highestRankWithCompletions.Index + 1, USERID ) ).Returns( Task.FromResult<Rank>( nextRank ) );
            mockRankMapper.Setup( m => m.MapToRankViewModel( nextRank, workingRankPercentageComplete ) ).Returns( workingRankViewModel );

            // Act
            var result = await unitUnderTest.GetCurrentRankAsync( userIdForStatuses );

            // Assert
            Assert.AreEqual( totalPercentCompleted, result.CompletedCompletionPercentage );
            Assert.AreEqual( workingRankPercentageComplete, result.WorkingCompletionPercentage );
            Assert.AreSame( workingRankViewModel, result.WorkingRank );
            Assert.AreSame( completedRankViewModel, result.CompletedRank );
        }

        [Test]
        public async Task UpdateAsync_RankDoesNotExist_UpdateShouldNotBeCalled()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var id = Guid.NewGuid();
            var rank =_fixture.Build<Rank>().Create();
            mockRankRepository.Setup( m => m.Get( id, true ) ).Returns( (Rank)null );

            // Act
            await unitUnderTest.UpdateAsync(
                id,
                rank );
        }

        [Test]
        public async Task UpdateAsync_RankExists_UpdateShouldBeCalled()
        {
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var id = Guid.NewGuid();
            var rank = _fixture.Build<Rank>().Create();
            var expectedRank = _fixture.Build<Rank>().Create();
            mockRankRepository.Setup( m => m.Get( id, true ) ).Returns( expectedRank );
            mockRankRepository.Setup( m => m.Update( id, expectedRank ) );
            mockGuildDbContext.Setup( m => m.SaveChangesAsync() ).Returns( Task.FromResult( 1 ) );

            // Act
            await unitUnderTest.UpdateAsync( id, rank );

            // Assert
            Assert.AreEqual( expectedRank.Name, rank.Name );
            Assert.AreEqual( expectedRank.Description, rank.Description );
            mockRankRepository.Verify( m => m.Update( id, expectedRank ), Times.Once );
        }

        [Test]
        [TestCase( 0 )]
        [TestCase( 3 )]
        [TestCase( 4 )]
        [TestCase( 7 )]
        [TestCase( 125 )]
        public async Task AddAsync( Int32 maxIndex )
        {
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var input = _fixture.Build<CreateRankModel>().Create();
            var newRank = _fixture.Build<Rank>().Create();
            mockRankRepository.Setup( m => m.GetMaxRankIndexAsync() ).Returns( Task.FromResult( maxIndex ) );
            mockRankMapper.Setup( m => m.CreateRank( input.Description, input.Name, maxIndex + 1 ) ).Returns( newRank );
            mockRankRepository.Setup( m => m.Add( newRank ) );
            mockGuildDbContext.Setup( m => m.SaveChangesAsync() ).Returns( Task.FromResult( 1 ) );

            // Act
            var result = await unitUnderTest.AddAsync( input );

            // Assert
            Assert.AreSame( newRank, result );
            mockRankRepository.Verify( m => m.Add( newRank ), Times.Once );
        }

        [Test]
        public async Task DeleteRankAsync_RankExists_DeletesFromRepo()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var id = Guid.NewGuid();
            mockRankRepository.Setup( m => m.DeleteRankAsync( id ) ).Returns( Task.CompletedTask );

            // Act
            await unitUnderTest.DeleteRankAsync(
                id );
        }

        [Test]
        public async Task DeleteRankAsync_RankDoesNotExist_DoesNotCallDeleteFromRepo()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var id = Guid.NewGuid();
            mockRankRepository.Setup( m => m.DeleteRankAsync( id ) ).Returns( Task.CompletedTask );

            // Act
            await unitUnderTest.DeleteRankAsync(
                id );
        }

        [Test]
        public async Task UpdateRankOrderAsync()
        {
            var unitUnderTest = this.CreateProvider();
            var request = _fixture.Build<GoalIndexEntry>().CreateMany( 3 );
            var expectedRanks = _fixture.Build<Rank>().CreateMany( 3 ).ToList();
            mockRankRepository.Setup( m => m.UpdateOrder( request ) );
            mockRankRepository.Setup( m => m.List() ).Returns( CreateAsyncQueryable( expectedRanks ) );
            mockGuildDbContext.Setup( m => m.SaveChangesAsync() ).Returns( Task.FromResult( 1 ) );

            var result = await unitUnderTest.UpdateOrderAsync( request );

            Assert.IsTrue( expectedRanks.SequenceEqual( result ) );
            mockRankRepository.Verify( m => m.UpdateOrder( request ), Times.Once );
        }

        [Test]
        public async Task GetRankImage_ReturnFileResultWithFilePathFromImageProvider()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var id = Guid.NewGuid();
            FileDownloadResult tmpFilePath =_fixture.Create<FileDownloadResult>();
            mockAttachmentProvider.Setup( m => m.DownloadFile( WarriorsGuildFileType.RankImage, id.ToString() ) ).Returns( Task.FromResult( tmpFilePath ) );

            // Act
            var result = await unitUnderTest.GetImage( id );

            // Assert
            Assert.IsInstanceOf<FileDetail>( result );
            Assert.AreEqual( tmpFilePath.FilePathToServe, result.FilePath );
            Assert.AreEqual( tmpFilePath.ContentType, result.ContentType );
        }

        [Test]
        public async Task UploadImage()
        {
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var id = Guid.NewGuid();
            string ext = ".pdf";
            string localFileName = "sadfsadfs";
            string mediaType = "adsfsd";
            var request = new HttpRequestMessage();
            var dummyResult = new FileUploadResult();
            Rank rank = _fixture.Create<Rank>();
            //mockAttachmentProvider.Setup( m => m.UploadFileAsync( WarriorsGuildFileType.RankImage, new byte[0], id.ToString(), mediaType ) ).Returns( Task.FromResult( dummyResult ) );
            mockRankRepository.Setup( m => m.Get( id, false ) ).Returns( rank );
            mockRankRepository.Setup( m => m.SetHasImage( rank, ext ) );
            mockGuildDbContext.Setup( m => m.SaveChangesAsync() ).Returns( Task.FromResult( 1 ) );

            // Act
            await unitUnderTest.UploadImageAsync(
                id,
                ext,
                localFileName,
                mediaType );

            mockRankRepository.Verify( m => m.SetHasImage( rank, ext ), Times.Once );
            mockGuildDbContext.Verify( m => m.SaveChangesAsync(), Times.Once );
        }
    }
}
