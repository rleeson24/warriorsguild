using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WarriorsGuild.Helpers.Utilities.Files;
using WarriorsGuild.Rings.Models.Status;

namespace WarriorsGuild.Rings
{
    public interface IRecordRingCompletion
    {
        Task<List<Guid>> UploadAttachmentsForRingReq( Guid ringId, Guid reqId, IEnumerable<MultipartFileData> fileData, Guid userIdForStatuses );
    }
    public class RecordRingCompletion : IRecordRingCompletion
    {
        private IRingsProvider RingsProvider { get; }

        public RecordRingCompletion( IRingsProvider ringsProvider )
        {
            RingsProvider = ringsProvider;
        }


        public async Task<RecordRingCompletionResponse> RecordCompletionAsync( RingStatusUpdateModel rankToUpdate, Guid userIdForStatus )
        {
            var response = await RingsProvider.RecordCompletionAsync( rankToUpdate, userIdForStatus );
            return response;
        }

        public async Task<List<Guid>> UploadAttachmentsForRingReq( Guid ringId, Guid reqId, IEnumerable<MultipartFileData> fileData, Guid userIdForStatuses )
        {
            var result = new List<Guid>();
            var requirement = await RingsProvider.GetRequirementAsync( ringId, reqId );
            var ring = await RingsProvider.GetAsync( requirement.RingId );
            var i = 0;
            foreach ( var fd in fileData )
            {
                var fileName = $"{ring.Name}_{requirement.Id.ToString()}_{userIdForStatuses}_{i}";
                await RingsProvider.UploadProofOfCompletionAsync( fileName, fd.Extension, fd.Content, fd.ContentDisposition.DispositionType.Value );
                result.Add( await RingsProvider.RecordProofOfCompletionDocumentAsync( requirement.Id, userIdForStatuses, fileName, fd.Extension ) );
                i++;
            }
            return result;
        }
    }
}