using System.Collections.Generic;

namespace WarriorsGuild.Email
{
    public record EmailMessage
    {
        public EmailMessage()
        {
            SenderAddress = "tech@warriorsguild.com";
            SenderName = "Warriors Guild Tech Support";
        }

        public string SenderAddress { get; set; }
        public string SenderName { get; set; }
        /// <summary>
        /// Recipient email addresses separated by commas
        /// </summary>
        public IEnumerable<string> Recipients { get; set; }
        public string Subject { get; set; }
        public string TextBody { get; set; }
        public string HtmlBody { get; set; }
        public AttachmentCollection Attachments { get; set; }
        public string DisplayName { get; set; }
    }
}