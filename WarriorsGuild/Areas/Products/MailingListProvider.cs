using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using WarriorsGuild.Data.Models;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Email;
using WarriorsGuild.Models;
using WarriorsGuild.Providers;

namespace WarriorsGuild.Areas.Products
{
    public class MailingListProvider : IMailingListProvider
    {
        private IGuildDbContext _guildDbContext;
        private IEmailProvider EmailProvider { get; }

        public MailingListProvider( IGuildDbContext guildDbContext, IEmailProvider emailProvider )
        {
            _guildDbContext = guildDbContext;
            EmailProvider = emailProvider;
        }

        public IQueryable<MailingListEntry> GetMailingList()
        {
            return _guildDbContext.MailingList;
        }

        public async Task<MailingListEntry> GetMailingListEntry( string id )
        {
            return await _guildDbContext.MailingList.FindAsync( id );
        }

        public async Task<MailingListEntry> GetMailingListEntryByEmail( string emailAddress )
        {
            return await _guildDbContext.MailingList.FirstOrDefaultAsync( m => m.EmailAddress == emailAddress );
        }

        public async Task<MailingListEntry> PutMailingListEntry( string id, MailingListEntry mailingListEntry )
        {
            _guildDbContext.Entry( mailingListEntry ).State = EntityState.Modified;

            try
            {
                await _guildDbContext.SaveChangesAsync();
            }
            catch ( DbUpdateConcurrencyException )
            {
                if ( !MailingListEntryExists( id ) )
                {
                    throw new ConflictException();
                }
                else
                {
                    throw;
                }
            }

            return mailingListEntry;
        }

        public async Task<MailingListEntry> PostMailingListEntry( MailingListEntry mailingListEntry )
        {
            _guildDbContext.MailingList.Add( mailingListEntry );

            try
            {
                await _guildDbContext.SaveChangesAsync();
            }
            catch ( DbUpdateException )
            {
                if ( MailingListEntryExists( mailingListEntry.EmailAddress ) )
                {
                    throw new ConflictException();
                }
                else
                {
                    throw;
                }
            }

            return mailingListEntry;
        }

        public async Task DeleteMailingListEntry( MailingListEntry entry )
        {
            _guildDbContext.Entry( entry ).CurrentValues.SetValues( new { Subscribed = false } );
            await _guildDbContext.SaveChangesAsync();
        }


        public async Task<PostFreeReportResponse> PostFreeReportRequest( string emailAddress )
        {
            var response = new PostFreeReportResponse();

            var mailingListEntry = new MailingListEntry( new MailAddress( emailAddress ), true );
            _guildDbContext.MailingList.Add( mailingListEntry );
            _guildDbContext.SaveChanges();

            await SendReport( emailAddress, mailingListEntry );
            return response;
        }


        public async Task<PostFreeReportResponse> PostFreeReportRequest( MailingListEntry entry )
        {
            var response = new PostFreeReportResponse();
            await SendReport( entry.EmailAddress, entry );
            return response;
        }

        private async Task SendReport( string emailAddress, MailingListEntry mailingListEntry )
        {
            var message = new EmailMessage();
            message.DisplayName = emailAddress;
            message.Recipients = new[] { emailAddress };
            message.Subject = "Warrior's Guild - Free Report";
            message.TextBody = "You must have an HTML compatible email client to view the report";
            message.HtmlBody = await EmailProvider.RenderEmailViewToStringAsync( EmailView.FreeReport, null );
            await EmailProvider.SendAsync( message );
            _guildDbContext.Entry( mailingListEntry ).Entity.FreeReportSent = true;
            _guildDbContext.SaveChanges();
        }


        public async Task Unsubscribe( string emailAddress )
        {
            var entry = await _guildDbContext.MailingList.FirstOrDefaultAsync( e => e.EmailAddress.ToLower() == emailAddress.ToLower() );
            if ( entry != null )
            {
                _guildDbContext.Entry( entry ).Entity.Subscribed = false;
                await _guildDbContext.SaveChangesAsync();
            }
        }

        private bool MailingListEntryExists( string id )
        {
            return _guildDbContext.MailingList.Count( e => e.EmailAddress == id ) > 0;
        }
    }
}