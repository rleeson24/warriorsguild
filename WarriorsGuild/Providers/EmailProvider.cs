using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using WarriorsGuild.Email;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Helpers.Utilities.Models;

namespace WarriorsGuild.Providers
{
	public partial class EmailProvider : IEmailProvider
	{
		private String Signature
		{
			get
			{
				return @"<p>
						   <img src="""" height=""100px"" />
						 </p>";
			}
		}

		private readonly IRazorViewEngine _razorViewEngine;
		private readonly ITempDataProvider _tempDataProvider;
		private readonly IServiceProvider _serviceProvider;
		private readonly IEmailSender emailSender;
		private readonly IHttpContextAccessor httpContextAccessor;

		public EmailProvider( IRazorViewEngine razorViewEngine,
			ITempDataProvider tempDataProvider,
			IServiceProvider serviceProvider,
			IEmailSender emailSender, IHttpContextAccessor httpContextAccessor )
		{
			_razorViewEngine = razorViewEngine;
			_tempDataProvider = tempDataProvider;
			_serviceProvider = serviceProvider;
			this.emailSender = emailSender;
			this.httpContextAccessor = httpContextAccessor;
		}

		public async Task<string> RenderEmailViewToStringAsync( EmailView partial, string? model )
		{
			var routeData = new RouteData();
			routeData.Values.Add( "controller", "Email" );
			var actionContext = new ActionContext( httpContextAccessor.HttpContext!, routeData, new ActionDescriptor() );

			using ( var sw = new StringWriter() )
			{
				var viewResult = _razorViewEngine.FindView( actionContext, partial.ToString(), false );

				if ( viewResult.View == null )
				{
					throw new ArgumentNullException( $"{partial.ToString()} does not match any available view" );
				}

				var viewDictionary = new ViewDataDictionary( new EmptyModelMetadataProvider(), new ModelStateDictionary() )
				{
					Model = model
				};

				var viewContext = new ViewContext(
					actionContext,
					viewResult.View,
					viewDictionary,
					new TempDataDictionary( actionContext.HttpContext, _tempDataProvider ),
					sw,
					new HtmlHelperOptions()
				);

				await viewResult.View.RenderAsync( viewContext );
				return sw.ToString();
			}
		}

		public async Task SendAsync( string subject, string body, EmailView view )
		{
			var message = new EmailMessage();
			message.DisplayName = "rleeson_2000@yahoo.com";
			message.Recipients = new string[] { "rleeson_2000@yahoo.com" };
			message.Subject = subject;
			message.HtmlBody = await this.RenderEmailViewToStringAsync( view, body );
			message.TextBody = body;
			await sendViaSmtp( message );
		}

		public async Task SendAsync( string subject, string body, IEnumerable<String> emailAddresses, EmailView view )
		{
			var message = new EmailMessage();
			message.Recipients = emailAddresses;
			message.Subject = subject;
			message.HtmlBody = await this.RenderEmailViewToStringAsync( view, body );
			message.TextBody = body;
			await sendViaSmtp( message );
		}

		public async Task SendAsync( EmailMessage message )
		{
			await sendViaSmtp( message );
		}

		public async Task SendAsync( string subject, string body, string emailAddress, EmailView view )
		{
			var message = new EmailMessage();
			message.DisplayName = emailAddress;
			message.Recipients = new string[] { emailAddress };
			message.Subject = subject;
			message.HtmlBody = await this.RenderEmailViewToStringAsync( view, body );
			message.TextBody = body;
			await sendViaSmtp( message );
		}

		// Use NuGet to install SendGrid (Basic C# client lib) 
		private async Task sendViaSmtp( EmailMessage message )
		{
			var attachments = new List<Attachment>();
			if ( message.Attachments != null )
			{
				foreach ( var att in message.Attachments )
				{
					var fileBytes = (Byte[])att.Bytes.Clone();
					attachments.Add( new Attachment() { Content = fileBytes, Name = att.FileName, ContentType = att.ContentType } );
				}
			}

			// Now we just need to set the message body and we're done
			var msg = new Message( message.Recipients, message.Subject, message.TextBody, message.HtmlBody, attachments,
				new EmailRecipient( message.SenderName, message.SenderAddress ), new[] { new EmailRecipient( "Warriors Guild Tech Support", "tech@warriorsguild.com" ) } );
			await emailSender.SendEmailAsync( msg );

			//using ( var client = new SmtpClient() )
			//{
			//    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
			//    client.ServerCertificateValidationCallback = ( s, c, h, e ) => true;
			//    client.Connect( "smtp.gmail.com", 465, true );

			//    // Note: only needed if the SMTP server requires authentication
			//    client.Authenticate( "rleeson24@gmail.com", "yrwvgsqnzfhujegd" );
			//    //client.Connect( "smtp.zoho.com", 465, true );
			//    //client.Authenticate( "tech@warriorsguild.com", "AGBaHxgtn2cK" );

			//    await client.SendAsync( mMessage );
			//    client.Disconnect( true );
			//}
		}
	}
}