using System;

namespace WarriorsGuild.Helpers.Utilities
{
    public interface IHelpers
    {
        DateTime GetCurrentDateTime();
        Boolean IsValidEmail( String email );

    }
}