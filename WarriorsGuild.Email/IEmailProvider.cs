using System.Collections.Generic;
using System.Threading.Tasks;

namespace WarriorsGuild.Email
{
    public interface IEmailProvider
    {
        Task<string> RenderEmailViewToStringAsync( EmailView partial, string model );
        Task SendAsync( EmailMessage message );
        Task SendAsync( string subject, string body, EmailView view );
        Task SendAsync( string subject, string body, string emailAddress, EmailView view );
        Task SendAsync( string subject, string body, IEnumerable<string> emailAddresses, EmailView view );
    }
}