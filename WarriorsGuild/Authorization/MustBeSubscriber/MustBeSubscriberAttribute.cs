//using Microsoft.AspNetCore.Authorization;
//using System;
//using System.Net;
//using System.Net.Http;
//using System.Threading;
//using System.Threading.Tasks;
//using WarriorsGuild.DataAccess;
//using WarriorsGuild.DataAccess.Repositories;

//namespace WarriorsGuild.Helpers.Attributes.WebApi
//{
//    public sealed class MustBeSubscriberAttribute : AuthorizeAttribute
//	{
//		public Task<HttpResponseMessage> ExecuteAuthorizationFilterAsync( HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation )
//		{
//			if ( !actionContext.RequestContext.Principal.IsInRole( "Admin" ) )
//			{
//				var userId = actionContext.RequestContext.Principal.Identity.GetUserId();
//				var userIdGuid = new Guid( userId );

//				using ( var db = new ApplicationDbContext() )
//				{
//					var subRepo = new SubscriptionRepository( db );
//					var userSub = subRepo.GetMySubscription( userId );
//					if ( userSub == null || userSub.BillingAgreement.Cancelled.HasValue )
//					{
//						actionContext.Response = new HttpResponseMessage
//						{
//							StatusCode = HttpStatusCode.Forbidden,
//							RequestMessage = actionContext.ControllerContext.Request
//						};
//						return FromResult( actionContext.Response );
//					}
//				}
//			}
//			return continuation();
//		}

//		private Task<HttpResponseMessage> FromResult( HttpResponseMessage result )
//		{
//			var source = new TaskCompletionSource<HttpResponseMessage>();
//			source.SetResult( result );
//			return source.Task;
//		}
//	}
//}