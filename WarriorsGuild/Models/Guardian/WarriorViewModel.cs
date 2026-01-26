namespace WarriorsGuild.Models.Guardian
{
    public class WarriorViewModel
    {
        public string Id { get; internal set; } = default!;
        public string Name { get; internal set; } = default!;
        public string Username { get; internal set; } = default!;
        public string? AvatarSrc { get; internal set; }
    }
}