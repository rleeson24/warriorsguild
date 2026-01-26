using WarriorsGuild.Data.Models;
using WarriorsGuild.Models;

namespace WarriorsGuild.Areas.Products
{
    public interface IMailingListProvider
    {
        Task DeleteMailingListEntry( MailingListEntry entry );
        IQueryable<MailingListEntry> GetMailingList();
        Task<MailingListEntry> GetMailingListEntry( string id );
        Task<PostFreeReportResponse> PostFreeReportRequest( string emailAddress );
        Task<PostFreeReportResponse> PostFreeReportRequest( MailingListEntry entry );
        Task<MailingListEntry> PostMailingListEntry( MailingListEntry mailingListEntry );
        Task<MailingListEntry> PutMailingListEntry( string id, MailingListEntry mailingListEntry );
        Task Unsubscribe( string emailAddress );
        Task<MailingListEntry> GetMailingListEntryByEmail( string emailAddress );
    }
}