using System;

namespace WarriorsGuild.Helpers.Utilities
{
    /// <summary>
    /// Composite interface for backward compatibility. Prefer depending on
    /// IDateTimeProvider or IEmailValidator for specific needs (ISP).
    /// </summary>
    public interface IHelpers : IDateTimeProvider, IEmailValidator
    {
    }
}