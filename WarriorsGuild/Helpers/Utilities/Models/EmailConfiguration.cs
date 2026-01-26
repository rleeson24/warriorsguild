namespace WarriorsGuild.Helpers.Utilities.Models
{
    public class EmailConfiguration
    {
        public string From { get; set; } = default!;
        public string SmtpServer { get; set; } = default!;
        public int Port { get; set; }
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string FromName { get; set; } = default!;
    }
}
