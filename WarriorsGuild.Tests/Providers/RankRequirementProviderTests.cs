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

        private Mock<IGuildDbContext> mockGuildDbContext;
        private Mock<IRankRepository> mockRankRepository;
        private Mock<IRankMapper> mockRankMapper;
        private Mock<IRankStatusProvider> mockRankStatusProvider;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository( MockBehavior.Strict );

            this.mockGuildDbContext = this.mockRepository.Create<IGuildDbContext>();
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
                this.mockGuildDbContext.Object,
                this.mockRankRepository.Object,
                this.mockRankMapper.Object,
                mockRankStatusProvider.Object);
        }

        [Test]
        public async Task GetRequirementsAsync()
        {
            
            // Arrange
            Guid userIdForStatuses = default;
            var unitUnderTest = this.CreateProvider();
            var id = Guid.NewGuid();
            var expectedReq = _fixture.Build<RankRequirement>().CreateMany( 1 ).AsQueryable();
            mockRankRepository.Setup( m => m.GetRequirements( id ) ).Returns( expectedReq );
            var selectReqs = expectedReq.Skip( 2 );
            var expectedStatuses = selectReqs.Select( r =>_fixture.Build<RankStatus>().With( s => s.RankRequirementId, r.Id ).With( s => s.UserId, userIdForStatuses ).Create() );
            mockRankRepository.Setup( m => m.RankStatuses() ).Returns( expectedStatuses );

            var expectedReturn = new List<RankRequirementViewModel>();

            expectedReq.ToList().ForEach( r =>
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

                IEnumerable<MinimalAttachmentDetail> completedAtt = new MinimalAttachmentDetail[ 0 ];
                if ( r.RequireAttachment && hasStatus )
                {
                    completedAtt =_fixture.CreateMany<MinimalAttachmentDetail>();
                    mockRankStatusProvider.Setup( m => m.GetAttachmentsForRankStatus( r.Id, userIdForStatuses ) ).ReturnsAsync( completedAtt );
                }

                if ( r.RequireCross )
                {
                    crossesToComplete =_fixture.CreateMany<MinimalCrossDetail>();
                    mockRankRepository.Setup( m => m.CrossesForRankReq( r.RankId, r.Id ) ).ReturnsAsync( crossesToComplete );
                }
                var vmToAdd =_fixture.Create<RankRequirementViewModel>();
                var itIsRings = It.Is<IEnumerable<MinimalRingDetail>>( seq => seq.SequenceEqual( completedRings ) );
                var itIsCrosses = It.Is<IEnumerable<MinimalCrossDetail>>( seq => seq.SequenceEqual( crossesToComplete ) );
                var itIsAtts = It.Is<IEnumerable<MinimalAttachmentDetail>>( seq => seq.SequenceEqual( completedAtt ) );
                if ( status != null ) mockRankMapper.Setup( m => m.CreateRequirementViewModel( r,
                                                                status.WarriorCompleted,
                                                                status.GuardianCompleted, itIsRings, itIsCrosses, itIsAtts ) ).Returns( vmToAdd );
                else mockRankMapper.Setup( m => m.CreateRequirementViewModel( r,
                                                                        null,
                                                                        null, itIsRings, itIsCrosses, itIsAtts ) ).Returns( vmToAdd );
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
            
            // Arrange
            var unitUnderTest = this.CreateProvider();
            var id = Guid.NewGuid();
            var existingRequirements =_fixture.Build<RankRequirement>().CreateMany( 4 ).AsQueryable();
            var requirements =_fixture.Build<RankRequirementViewModel>().CreateMany( 3 ).AsQueryable();
            mockRankRepository.Setup( m => m.GetRequirements( id ) ).Returns( existingRequirements );
            mockRankRepository.Setup( m => m.UpdateRequirementsAsync( id, requirements, existingRequirements ) ).Returns( Task.CompletedTask );

            // Act
            await unitUnderTest.UpdateRequirementsAsync(
                id,
                requirements );
        }
    }
}
