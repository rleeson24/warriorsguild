using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WarriorsGuild.Helpers.Utilities.Files;
using WarriorsGuild.Ranks.Models.Status;

namespace WarriorsGuild.Ranks
{
    public interface IRecordCompletion
    {
        Task<RecordCompletionResponse> RecordCompletionAsync( RankStatusUpdateModel rankToUpdate, Guid userIdForStatus );
        Task<List<Guid>> UploadAttachmentsForRankReq( Guid rankId, Guid reqId, IEnumerable<MultipartFileData> fileData, Guid userIdForStatuses );
    }
    public class RecordCompletion : IRecordCompletion
    {
        private IRanksProvider _ranksProvider { get; }
        private IRankStatusProvider _rankStatusProvider { get; }
        private IRankRequirementProvider _rankRequirementProvider { get; }
        private IRanksProviderHelpers RanksProviderHelpers { get; }

        public RecordCompletion( IRankStatusProvider rankStatusProvider, IRanksProviderHelpers ranksProviderHelpers, IRankRequirementProvider rankRequirementProvider, IRanksProvider ranksProvider )
        {
            _rankStatusProvider = rankStatusProvider;
            RanksProviderHelpers = ranksProviderHelpers;
            _rankRequirementProvider = rankRequirementProvider;
            _ranksProvider = ranksProvider;
        }

        public async Task<RecordCompletionResponse> RecordCompletionAsync( RankStatusUpdateModel rankToUpdate, Guid userIdForStatus )
        {
            var response = new RecordCompletionResponse();
            var requirement = await _rankRequirementProvider.GetRequirementAsync( rankToUpdate.RankId, rankToUpdate.RankRequirementId );
            //if ( requirement.RequireCross )
            //{
            //    if ( !rankToUpdate.Crosses.Any() )
            //    {
            //        response.Error = "This Rank requirement requires Crosses";
            //        return response;
            //    }
            //    var approvedOrPendingCrosses = await CrossProvider.GetUnassignedPendingOrApproved( userIdForStatus );
            //    if ( approvedOrPendingCrosses.Count( c => rankToUpdate.Crosses.Contains( c.CrossId ) ) != rankToUpdate.Crosses.Count() )
            //    {
            //        response.Error = "One or more of the selected crosses have already been used or are no longer available for assignement. Refresh the page to get fresh lists.";
            //        return response;
            //    }
            //}
            //if ( requirement.RequireRing )
            //{
            //    if ( !rankToUpdate.Rings.Any() )
            //    {
            //        response.Error = "This Rank requirement requires Rings";
            //        return response;
            //    }
            //    var approvedOrPendingRings = await RingsProvider.GetUnassignedPendingOrApproved( userIdForStatus );
            //    if ( approvedOrPendingRings.Count( c => rankToUpdate.Rings.Contains( c.RingId ) ) != rankToUpdate.Rings.Count() )
            //    {
            //        response.Error = "One or more of the selected rings have already been used or are no longer available for assignement. Refresh the page to get fresh lists.";
            //        return response;
            //    }
            //}

            response = await _rankStatusProvider.RecordCompletionAsync( rankToUpdate, userIdForStatus );
            return response;
        }

        public async Task<List<Guid>> UploadAttachmentsForRankReq( Guid rankId, Guid reqId, IEnumerable<MultipartFileData> fileData, Guid userIdForStatuses )
        {
            var result = new List<Guid>();
            var requirement = await _rankRequirementProvider.GetRequirementAsync( rankId, reqId );
            var rank = await _ranksProvider.GetAsync( requirement.RankId );
            if ( await RanksProviderHelpers.AllPreviousRanksComplete( rankId, userIdForStatuses ) )
            {
                var i = 0;
                foreach ( var fd in fileData )
                {
                    var fileName = $"{rank.Name}_{requirement.Id.ToString()}_{userIdForStatuses}_{i}";
                    await _rankStatusProvider.UploadProofOfCompletionAsync( fileName, fd.Extension, fd.Content, fd.ContentDisposition.DispositionType.Value );
                    result.Add( await _rankStatusProvider.RecordProofOfCompletionDocumentAsync( requirement.Id, userIdForStatuses, fileName, fd.Extension ) );
                    i++;
                }
            }
            return result;
        }
    }
}