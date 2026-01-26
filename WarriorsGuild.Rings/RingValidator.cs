using System.Collections.Generic;
using System.Linq;
using WarriorsGuild.Data.Models.Rings;

namespace WarriorsGuild.Rings
{
    public interface IRingValidator
    {
        IEnumerable<string> ValidateRequirements( IEnumerable<RingRequirement> requirements );
    }

    public class RingValidator : IRingValidator
    {
        public IEnumerable<string> ValidateRequirements( IEnumerable<RingRequirement> requirements )
        {
            var result = new List<string>();
            var requirementHasZeroWeight = requirements.Any( r => r.Weight <= 0 );
            if ( requirementHasZeroWeight )
            {
                result.Add( "Each requirement weight must be greater than 0" );
            }
            var totalWeight = requirements.Sum( r => r.Weight );
            if ( totalWeight != 0 && totalWeight != 100 )
            {
                result.Add( $"The requirement weights must add up to 0 or 100.  Current sum is {totalWeight}" );
            }
            return result;
        }
    }
}
