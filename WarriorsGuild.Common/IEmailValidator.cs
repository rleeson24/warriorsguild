namespace WarriorsGuild.Helpers.Utilities
{
    /// <summary>
    /// Validates email format. Segregated from IDateTimeProvider per Interface Segregation Principle.
    /// </summary>
    public interface IEmailValidator
    {
        bool IsValidEmail( string email );
    }
}
