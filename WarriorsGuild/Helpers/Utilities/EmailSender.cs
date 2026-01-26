using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using WarriorsGuild.Helpers.Utilities.Models;

namespace WarriorsGuild.Helpers.Utilities
{
    public interface IEmailSender
    {
        void SendEmail( Message message );
        Task SendEmailAsync( Message message );
    }

    public class EmailSender : Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly IWebHostEnvironment _environment;

        public EmailSender( EmailConfiguration emailConfig, IWebHostEnvironment environment )
        {
            _emailConfig = emailConfig;
            _environment = environment;
        }

        public void SendEmail( Message message )
        {
            var emailMessage = CreateEmailMessage( message );

            Send( emailMessage );
        }
        public async Task SendEmailAsync( Message message )
        {
            var mailMessage = CreateEmailMessage( message );
            await SendAsync( mailMessage );
        }

        public async Task SendEmailAsync( string email, string subject, string htmlMessage )
        {
            var message = new Message( new[] { email }, subject, htmlMessage, htmlMessage, Array.Empty<Attachment>() );
            var mailMessage = CreateEmailMessage( message );
            await SendAsync( mailMessage );
        }

        private MimeMessage CreateEmailMessage( Message message )
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add( message.From ?? new MailboxAddress( _emailConfig.FromName, _emailConfig.From ) );
            emailMessage.To.AddRange( message.To );
            emailMessage.Subject = message.Subject;
            emailMessage.Bcc.AddRange( message.Bcc );
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message.HtmlContent ?? string.Format( "<h2 style='color:red;'>{0}</h2>", message.TextContent ),
                TextBody = message.TextContent
            };
            if ( message.Attachments != null && message.Attachments.Any() )
            {
                foreach ( var attachment in message.Attachments )
                {
                    bodyBuilder.Attachments.Add( attachment.Name, attachment.Content, ContentType.Parse( attachment.ContentType ) );
                }
            }
            emailMessage.Body = bodyBuilder.ToMessageBody();
            return emailMessage;
        }

        private void Send( MimeMessage mailMessage )
        {
            using ( var client = new SmtpClient() )
            {
                try
                {
                    client.Connect( _emailConfig.SmtpServer, _emailConfig.Port, SecureSocketOptions.StartTls );
                    client.AuthenticationMechanisms.Remove( "XOAUTH2" );
                    client.Authenticate( _emailConfig.UserName, _emailConfig.Password );

                    client.Send( mailMessage );
                }
                catch
                {
                    //log an error message or throw an exception or both.
                    throw;
                }
                finally
                {
                    client.Disconnect( true );
                    client.Dispose();
                }
            }
        }
        private async Task SendAsync( MimeMessage mailMessage )
        {
            using ( var client = new SmtpClient() )
            {
                try
                {
                    var sslOptions = SecureSocketOptions.StartTls;
                    // for demo-purposes, accept all ssl certificates (in case the server supports starttls)
                    if ( _environment.EnvironmentName != "Development" )
                    {
                        client.ServerCertificateValidationCallback = ( s, c, h, e ) => true;
                        sslOptions = SecureSocketOptions.SslOnConnect;
                    }

                    await client.ConnectAsync( _emailConfig.SmtpServer, _emailConfig.Port, sslOptions );
                    client.AuthenticationMechanisms.Remove( "XOAUTH2" );
                    await client.AuthenticateAsync( _emailConfig.UserName, _emailConfig.Password );

                    await client.SendAsync( mailMessage );
                }
                catch ( Exception )
                {
                    //log an error message or throw an exception, or both.
                    throw;
                }
                finally
                {
                    await client.DisconnectAsync( true );
                    client.Dispose();
                }
            }
        }
    }
}
