using MimeKit;

namespace WarriorsGuild.Helpers.Utilities.Models
{
    public class Message
    {
        public MailboxAddress From { get; set; } = default!;
        public List<MailboxAddress> To { get; set; } = new List<MailboxAddress>();
        public List<MailboxAddress> Bcc { get; set; } = new List<MailboxAddress>();
        public string Subject { get; set; } = default!;
        public string? TextContent { get; set; }
        public string? HtmlContent { get; set; }
        public IEnumerable<Attachment> Attachments { get; set; }

        public Message( IEnumerable<string> to, string subject, string? textContent, string? htmlContent, IEnumerable<Attachment> attachments,
                        EmailRecipient? from = null, IEnumerable<EmailRecipient>? bcc = null )
        {
            if ( from != null )
            {
                From = new MailboxAddress( from.Name, from.EmailAddress );
            }
            To.AddRange( to.Select( x => new MailboxAddress( x, x ) ) );
            Bcc.AddRange( (bcc ?? new EmailRecipient[ 0 ]).Select( x => new MailboxAddress( x.Name, x.EmailAddress ) ) );
            Subject = subject;
            TextContent = textContent;
            HtmlContent = htmlContent;
            Attachments = attachments;
        }
    }

    public class EmailRecipient
    {
        public EmailRecipient( string? name, string address )
        {
            Name = name;
            EmailAddress = address;
        }

        public string? Name { get; set; }
        public string EmailAddress { get; set; }
    }

    public class Attachment
    {
        public byte[] Content { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string ContentType { get; set; } = default!;
    }
}
