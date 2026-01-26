namespace WarriorsGuild.Models
{
    public class WarriorDropDownItem
    {
        public string? Name { get; set; }
        public string Id { get; set; } = default!;
        public bool NeedsApproval { get; set; }
    }
}