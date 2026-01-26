using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsGuild.Ranks.ViewModels;

namespace WarriorsGuild.Ranks
{
    public interface IRankValidator
    {
        IEnumerable<string> ValidateRequirements( IEnumerable<RankRequirementViewModel> requirements );
    }

    public class RankValidator : IRankValidator
    {
        public IEnumerable<string> ValidateRequirements( IEnumerable<RankRequirementViewModel> requirements )
        {
            var errors = new List<string>();
            var requirementHasZeroWeight = requirements.Any( r => r.Weight <= 0 );
            if ( requirementHasZeroWeight )
            {
                errors.Add( "Each requirement weight must be greater than 0" );
            }
            if ( requirements.Any( r => r.RequireRing && (!r.RequiredRingCount.HasValue || r.RequiredRingCount.Value == 0 || !r.RequiredRingType.HasValue) ) )
            {
                errors.Add( "A ring count must be given when requiring a ring" );
            }
            if ( requirements.Any( r => r.RequireCross && (!r.RequiredCrossCount.HasValue || r.RequiredCrossCount.Value == 0 || r.CrossesToComplete.Count( c => c.Id != Guid.Empty ) != r.RequiredCrossCount.Value) ) )
            {
                errors.Add( "Crosses must be specified when requiring a cross" );
            }
            var totalWeight = requirements.Sum( r => r.Weight );
            if ( totalWeight != 0 && totalWeight != 100 )
            {
                errors.Add( $"The requirement weights must add up to 0 or 100.  Current sum is {totalWeight}" );
            }
            return errors;
        }
    }
}
