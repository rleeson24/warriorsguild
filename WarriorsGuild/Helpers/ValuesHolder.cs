namespace WarriorsGuild.Helpers
{
    public interface IValuesHolder
    {
        bool HasActiveSubscription { get; set; }
        bool IsPayingParty { get; set; }
    }

    public class ValuesHolder : IValuesHolder
    {
        public Boolean IsPayingParty { get; set; }
        public Boolean HasActiveSubscription { get; set; }
    }
}