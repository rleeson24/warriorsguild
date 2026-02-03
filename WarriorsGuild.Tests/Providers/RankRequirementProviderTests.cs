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
    public class RankRequirementProviderTests
    {
        protected Fixture _fixture = new Fixture();
        private MockRepository mockRepository;

        private Guid USERID = Guid.NewGuid();

        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IRankRepository> mockRankRepository;
        private Mock<IRankMapper> mockRankMapper;
        private Mock<IRankStatusProvider> mockRankStatusProvider;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository( MockBehavior.Strict );

            this.mockUnitOfWork = this.mockRepository.Create<IUnitOfWork>();
            this.mockRankRepository = this.mockRepository.Create<IRankRepository>();
            this.mockRankMapper = this.mockRepository.Create<IRankMapper>();
            this.mockRankStatusProvider = this.mockRepository.Create<IRankStatusProvider>();
        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        private RankRequirementProvider CreateProvider()
        {
            return new RankRequirementProvider(
                this.mockUnitOfWork.Object,
                this.mockRankRepository.Object,
                this.mockRankMapper.Object,
                mockRankStatusProvider.Object);
        }

        [Test]
        public async Task GetRequirementsAsync()
        {
            Guid userIdForStatuses = default;
            var unitUnderTest = this.CreateProvider();
            var id = Guid.NewGuid();
            var expectedReqList = _fixture.Build<RankRequirement>().With( r => r.RankId, id ).CreateMany( 1 ).ToList();
            mockRankRepository.Setup( m => m.GetRequirements( id ) ).Returns( CreateAsyncQueryable( expectedReqList ) );
            var expectedStatuses = expectedReqList.Select( r => _fixture.Build<RankStatus>().With( s => s.RankRequirementId, r.Id ).With( s => s.UserId, userIdForStatuses ).Create() ).ToList();
            mockRankStatusProvider.Setup( m => m.GetStatusesAsync( id, userIdForStatuses ) ).ReturnsAsync( expectedStatuses );

            var expectedReturn = new List<RankRequirementViewModel>();

            expectedReqList.ForEach( r =>
            {
                var status = expectedStatuses.SingleOrDefault( s => s.RankRequirementId == r.Id );
                var hasStatus = status != null;
                IEnumerable<MinimalRingDetail> completedRings = new MinimalRingDetail[ 0 ];
                if ( r.RequireRing && hasStatus )
                {
                    completedRings =_fixture.CreateMany<MinimalRingDetail>();
                    mockRankStatusProvider.Setup( m => m.GetRingsForRankStatus( r.RankId, r.Id, userIdForStatuses ) ).ReturnsAsync( completedRings );
                }
                IEnumerable<MinimalCrossDetail> crossesToComplete = new MinimalCrossDetail[ 0 ];

                IEnumerable<MinimalGoalDetail> completedAtt = Array.Empty<MinimalGoalDetail>();
                if ( r.RequireAttachment && hasStatus )
                {
                    completedAtt = new[] { new MinimalAttachmentDetail { Id = Guid.NewGuid() } };
                    mockRankStatusProvider.Setup( m => m.GetAttachmentsForRankStatus( r.Id, userIdForStatuses ) ).ReturnsAsync( completedAtt );
                }

                if ( r.RequireCross )
                {
                    crossesToComplete =_fixture.CreateMany<MinimalCrossDetail>();
                    mockRankRepository.Setup( m => m.CrossesForRankReq( r.RankId, r.Id ) ).ReturnsAsync( crossesToComplete );
                }
                var vmToAdd = _fixture.Create<RankRequirementViewModel>();
                if ( status != null )
                    mockRankMapper.Setup( m => m.CreateRequirementViewModel( r, status.WarriorCompleted, status.GuardianCompleted, It.IsAny<IEnumerable<MinimalRingDetail>>(), It.IsAny<IEnumerable<MinimalCrossDetail>>(), It.IsAny<IEnumerable<MinimalGoalDetail>>() ) ).Returns( vmToAdd );
                else
                    mockRankMapper.Setup( m => m.CreateRequirementViewModel( r, null, null, It.IsAny<IEnumerable<MinimalRingDetail>>(), It.IsAny<IEnumerable<MinimalCrossDetail>>(), It.IsAny<IEnumerable<MinimalGoalDetail>>() ) ).Returns( vmToAdd );
                expectedReturn.Add( vmToAdd );
            } );


            // Act
            var result = await unitUnderTest.GetRequirementsWithStatus(
                id, userIdForStatuses );

            // Assert
            Assert.True( expectedReturn.SequenceEqual( result ) );
        }

        [Test]
        public async Task UpdateRequirementsAsync()
        {
            var unitUnderTest = this.CreateProvider();
            var id = Guid.NewGuid();
            var existingRequirementsList = _fixture.Build<RankRequirement>().With( r => r.RankId, id ).CreateMany( 4 ).ToList();
            var requirements = _fixture.Build<RankRequirementViewModel>().CreateMany( 3 );
            mockRankRepository.Setup( m => m.GetRequirements( id ) ).Returns( CreateAsyncQueryable( existingRequirementsList ) );
            mockRankRepository.Setup( m => m.UpdateRequirementsAsync( id, It.IsAny<IEnumerable<RankRequirementViewModel>>(), It.IsAny<IEnumerable<RankRequirement>>() ) ).Returns( Task.CompletedTask );
            mockUnitOfWork.Setup( m => m.SaveChangesAsync() ).ReturnsAsync( 1 );

            await unitUnderTest.UpdateRequirementsAsync( id, requirements );
        }
    }
}
