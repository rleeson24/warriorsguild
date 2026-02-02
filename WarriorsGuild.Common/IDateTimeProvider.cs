using System;

namespace WarriorsGuild.Helpers.Utilities
{
    /// <summary>
    /// Provides abstracted access to current date/time (enables testability and follows SRP).
    /// </summary>
    public interface IDateTimeProvider
    {
        DateTime GetCurrentDateTime();
    }
}
