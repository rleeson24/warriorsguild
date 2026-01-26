using WarriorsGuild.Crosses.Models;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Data.Models.Crosses;

namespace WarriorsGuild.Crosses.Mappers
{
    public interface ICrossMapper
    {
        //RankViewModel MapToRankViewModel( Rank rank, IEnumerable<RankStatus> statuses );
        CrossViewModel MapToViewModel( Cross arg );
        CrossViewModel MapToViewModel( Cross arg, CrossApproval status );
    }
    public class CrossMapper : ICrossMapper
    {
        public CrossViewModel MapToViewModel( Cross arg, CrossApproval status )
        {
            if ( arg == null ) return null;
            var result = new CrossViewModel();
            result.Id = arg.Id;
            result.Name = arg.Name;
            result.Description = arg.Description;
            result.Index = arg.Index;
            result.ImageUploaded = arg.ImageUploaded;
            result.CompletedAt = status?.CompletedAt;
            result.ApprovedAt = status?.ApprovedAt;
            return result;
        }

        public CrossViewModel MapToViewModel( Cross arg )
        {
            if ( arg == null ) return null;
            var result = new CrossViewModel();
            result.Id = arg.Id;
            result.Name = arg.Name;
            result.Description = arg.Description;
            result.Index = arg.Index;
            result.ImageUploaded = arg.ImageUploaded;
            result.ImageExtension = arg.ImageExtension;
            return result;
        }
    }
}