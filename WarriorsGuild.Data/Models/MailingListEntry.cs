using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace WarriorsGuild.Data.Models
{
    public class MailingListEntry
    {
        public MailingListEntry()
        {

        }

        public MailingListEntry( MailAddress emailAddress, bool subscribed )
        {
            EmailAddress = emailAddress.Address;
            Subscribed = subscribed;
        }

        [Key]
        public string EmailAddress { get; set; }

        public bool Subscribed { get; set; }

        public bool FreeReportSent { get; set; }
    }
}