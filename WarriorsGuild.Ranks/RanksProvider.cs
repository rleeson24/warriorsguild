using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Ranks;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Models;
using WarriorsGuild.Ranks.Mappers;
using WarriorsGuild.Ranks.Models;
using WarriorsGuild.Ranks.ViewModels;
using WarriorsGuild.Storage;
using WarriorsGuild.Storage.Models;

namespace WarriorsGuild.Ranks
{
    public interface IRanksProvider
    {
        Task DeleteRankAsync( Guid id );
        Task<Rank> GetAsync( Guid id );
        Task<MyRankViewModel> GetCurrentRankAsync( Guid userIdForStatuses );
        Task<FileDetail> GetImage( Guid id );
        Task<IEnumerable<Rank>> GetListAsync( Guid userIdForStatuses );
        Task<Rank> GetPublicAsync();
        Task<Rank> AddAsync( CreateRankModel input );
        Task UpdateAsync( Guid id, Rank rank );

        Task<IEnumerable<Rank>> UpdateOrderAsync( IEnumerable<GoalIndexEntry> request );
        Task UploadImageAsync( Guid rankId, string fileExtension, string localFileName, string mediaType );

        Task<FileDetail> GetGuideAsync( Guid rankId );
        Task UploadGuideAsync( Guid crossId, string fileExtension, byte[] file, string mediaType );
    }

    public class RanksProvider : IRanksProvider
    {
        private IUnitOfWork _uow { get; }
        private IRankRepository _repo { get; }
        private IRankMapper _rankMapper { get; }
        private IBlobProvider FileProvider { get; }

        public RanksProvider( IUnitOfWork uow, IRankRepository repo, IRankMapper rankMapper, IBlobProvider fileProvider )
        {
            _uow = uow;
            _repo = repo;
            _rankMapper = rankMapper;
            FileProvider = fileProvider;
        }

        #region Ranks
        public async Task<IEnumerable<Rank>> GetListAsync( Guid userIdForStatuses )
        {
            return await _repo.List().ToArrayAsync();
        }

        public async Task<Rank> GetPublicAsync()
        {
            return await _repo.List().OrderBy( r => r.Index ).FirstOrDefaultAsync();
        }

        public async Task<Rank> GetAsync( Guid id )
        {
            return _repo.Get( id );
        }

        public async Task<MyRankViewModel> GetCurrentRankAsync( Guid userIdForStatuses )
        {
            var result = new MyRankViewModel();

            var highestRankWithCompletions = await _repo.GetHighestRankWithCompletionsAsync( userIdForStatuses );
            if ( highestRankWithCompletions != null )
            {
                var completedRequirements = highestRankWithCompletions.Requirements.Where( r => highestRankWithCompletions.Statuses.Where( s => s.GuardianCompleted.HasValue ).Any( s => s.RankRequirementId == r.Id ) );
                var percentageCompleted = completedRequirements.Sum( r => r.Weight );
                highestRankWithCompletions.Requirements = new RankRequirement[ 0 ];

                result.CompletedRank = _rankMapper.MapToRankViewModel( highestRankWithCompletions, percentageCompleted );
                result.CompletedCompletionPercentage = percentageCompleted;

            }

            //rank less than 100% - Completed Rank = Working Rank = highest rank
            //rank 100% - Completed Rank = rank, Working Rank = next rank with 0 completion
            if ( result.CompletedCompletionPercentage == 0 )
            {
                var workingRankIndex = 1;
                if ( result.CompletedRank == null )
                {
                }
                else if ( result.CompletedCompletionPercentage < 100 )
                {
                    workingRankIndex = result.CompletedRank.Index;
                }
                else
                {
                    workingRankIndex = result.CompletedRank.Index + 1;
                }
                result.WorkingRank = _rankMapper.MapToRankViewModel( await _repo.GetRankByIndexAsync( workingRankIndex, userIdForStatuses ), 0 );
            }
            else if ( result.CompletedCompletionPercentage < 100 )
            {
                result.WorkingCompletionPercentage = result.CompletedCompletionPercentage;
                result.WorkingRank = result.CompletedRank;
            }
            else if ( result.CompletedRank.Index > 0 )
            {
                result.WorkingRank = _rankMapper.MapToRankViewModel( await _repo.GetRankByIndexAsync( result.CompletedRank.Index + 1, userIdForStatuses ), 0 );
            }

            return result;
        }

        public async Task UpdateAsync( Guid id, Rank rank )
        {
            var existingParent = _repo.Get( id, true );
            if ( existingParent != null )
            {
                existingParent.Name = rank.Name;
                existingParent.Description = rank.Description;

                _repo.Update( id, existingParent );
                await _uow.SaveChangesAsync();
            }
        }

        public async Task<Rank> AddAsync( CreateRankModel input )
        {
            var maxIndexInDb = await _repo.GetMaxRankIndexAsync();
            var rank = _rankMapper.CreateRank( input.Description, input.Name, maxIndexInDb + 1 );
            _repo.Add( rank );
            await _uow.SaveChangesAsync();
            return rank;
        }

        public async Task DeleteRankAsync( Guid rankId )
        {
            await _repo.DeleteRankAsync( rankId );
        }


        public async Task<IEnumerable<Rank>> UpdateOrderAsync( IEnumerable<GoalIndexEntry> request )
        {
            _repo.UpdateOrder( request );
            await _uow.SaveChangesAsync();
            return await _repo.List().ToArrayAsync();
        }


        public async Task<FileDetail> GetImage( Guid rankId )
        {
            var fileResult = await FileProvider.DownloadFile( WarriorsGuildFileType.RankImage, rankId.ToString() );
            return new FileDetail( fileResult.FilePathToServe, null, fileResult.ContentType );
        }

        public async Task UploadImageAsync( Guid rankId, string fileExtension, string localFileName, string mediaType )
        {
            //var uploadedFileResult = await FileProvider.UploadFileAsync( WarriorsGuildFileType.RankImage, localFileName, rankId.ToString(), mediaType );

            var rank = _repo.Get( rankId );
            if ( rank != null )
            {
                _repo.SetHasImage( rank, fileExtension );
                await _uow.SaveChangesAsync();
            }
        }

        public async Task UploadGuideAsync( Guid rankId, string fileExtension, byte[] file, string mediaType )
        {
            var uploadedFileResult = await FileProvider.UploadFileAsync( WarriorsGuildFileType.Guide, file, rankId.ToString(), mediaType );

            var rank = _repo.Get( rankId );
            if ( rank != null )
            {
                _repo.SetHasGuide( rank, fileExtension );
                await _uow.SaveChangesAsync();
            }
        }

        public async Task<FileDetail> GetGuideAsync( Guid rankId )
        {
            var fileResult = await FileProvider.DownloadFile( WarriorsGuildFileType.Guide, rankId.ToString() );
            var rank = await _repo.GetRankWithGuideAsync( rankId );
            return new FileDetail( fileResult.FilePathToServe, rank?.Name + rank?.GuideFileExtension, fileResult.ContentType );
        }
        #endregion
    }
}