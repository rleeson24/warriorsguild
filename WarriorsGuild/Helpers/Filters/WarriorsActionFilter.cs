using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using WarriorsGuild.Areas.Payments;
using WarriorsGuild.Crosses;
using WarriorsGuild.DataAccess;
using WarriorsGuild.Helpers.Utilities;
using WarriorsGuild.Models;
using WarriorsGuild.Providers;
using WarriorsGuild.Ranks;
using WarriorsGuild.Rings;

namespace WarriorsGuild.Helpers.Filters
{
    public class WarriorsActionFilter : IAsyncResultFilter
    {
        private readonly SessionManager sessionManager;
        private readonly IGuildDbContext _dbContext;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IValuesHolder valuesHolder;
        private readonly IRanksProvider ranksProvider;
        private readonly IRingsProvider ringsProvider;
        private readonly ICrossProvider crossProvider;
        private readonly ICovenantProvider covenantProvider;
        private readonly IGuardianIntroProvider _guardianIntroProvider;
        private readonly IUserProvider _userProvider;
        private readonly IRankApprovalsProvider rankApprovalsProvider;

        public WarriorsActionFilter( SessionManager sessionManager, IGuildDbContext dbContext, ISubscriptionRepository subscriptionRepository, IValuesHolder valuesHolder,
                                        IRanksProvider ranksProvider, IRingsProvider ringsProvider, ICrossProvider crossProvider, ICovenantProvider covenantProvider,
                                        IUserProvider userProvider, IGuardianIntroProvider guardianIntroProvider, IRankApprovalsProvider rankApprovalsProvider )
        {
            this.sessionManager = sessionManager;
            this._dbContext = dbContext;
            _subscriptionRepository = subscriptionRepository;
            this.valuesHolder = valuesHolder;
            this.ranksProvider = ranksProvider;
            this.ringsProvider = ringsProvider;
            this.crossProvider = crossProvider;
            this.covenantProvider = covenantProvider;
            _userProvider = userProvider;
            _guardianIntroProvider = guardianIntroProvider;
            this.rankApprovalsProvider = rankApprovalsProvider;
        }

        public async Task OnResultExecutionAsync( ResultExecutingContext filterContext, ResultExecutionDelegate next )
        {
            var controller = filterContext.Controller as Controller;

            //Do not load the ViewBag on ApiControllers
            if ( controller != null )
            {
                var actionName = ((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)filterContext.ActionDescriptor).ActionName.ToLower();
                if ( actionName != "logout" )
                {
                    var warriorsInSession = new List<WarriorDropDownItem>();
                    if ( filterContext.HttpContext.User?.Identity != null && filterContext.HttpContext.User.Identity.IsAuthenticated )
                    {
                        var myUserId = _userProvider.GetMyUserId( filterContext.HttpContext.User );
                        await LoadCovenantCookie( filterContext, myUserId.ToString(), actionName );
                        await LoadGuardianCookies( filterContext, myUserId.ToString(), actionName );
                        if ( filterContext.HttpContext.User.IsInRole( "Guardian" ) )
                        {
                            //var lastRetrievedSons = sessionManager.LastRetrievedWarriors;
                            //warriorsInSession = sessionManager.Warriors?.ToList();
                            //if ( warriorsInSession == null || !lastRetrievedSons.HasValue || lastRetrievedSons.Value <= DateTime.UtcNow.AddMinutes( -30 ) )
                            //{
                            warriorsInSession = new List<WarriorDropDownItem>();
                            var user = await _dbContext.Users.Where( u => u.Id == myUserId.ToString() ).Include( u => u.ChildUsers ).FirstAsync();
                            var warriors = user.ChildUsers;
                            if ( warriors != null )
                            {
                                foreach ( var w in warriors )
                                {
                                    var needsRankApproval = await rankApprovalsProvider.GetPendingApprovalsAsync( Guid.Parse( w.Id ) );
                                    var needsRingApproval = await ringsProvider.GetPendingApprovalsAsync( Guid.Parse( w.Id ) );
                                    var needsCrossApproval = await crossProvider.GetPendingApprovalsAsync( Guid.Parse( w.Id ) );
                                    var warriorVM = new WarriorDropDownItem
                                    {
                                        Id = w.Id,
                                        Name = $"{w.FirstName} {w.LastName}",
                                        NeedsApproval = needsCrossApproval.Any() || needsRankApproval.Any() || needsRingApproval.Any()
                                    };
                                    warriorsInSession.Add( warriorVM );
                                }
                            }
                            //sessionManager.Warriors = warriorsInSession.ToArray();
                            //sessionManager.LastRetrievedWarriors = DateTime.Now;
                            //}

                            //    if ( sessionManager.UserIdForStatuses == null )
                            //    {
                            //        sessionManager.UserIdForStatuses = warriorsInSession.FirstOrDefault()?.Id;
                            //    }
                        }
                        var userSub = await _subscriptionRepository.GetMySubscriptionAsync( myUserId.ToString() );
                        var hasActiveSubscription = false;
                        var isPayingParty = false;
                        //if ( userSub != null )
                        //{
                        //    hasActiveSubscription = !userSub.BillingAgreement.Cancelled.HasValue;
                        //    isPayingParty = userSub.UserSubscription.IsPayingParty;
                        //}
                        hasActiveSubscription = true;
                        isPayingParty = filterContext.HttpContext.User.IsInRole( "Guardian" );

                        controller.ViewBag.HasActiveSubscription = hasActiveSubscription;
                        controller.ViewBag.IsPayingParty = isPayingParty;
                    }
                    else
                    {
                        controller.ViewBag.HasActiveSubscription = false;
                        controller.ViewBag.IsPayingParty = false;
                    }
                    controller.ViewBag.Warriors = warriorsInSession;
                }
            }
            await next();
        }

        private async Task LoadCovenantCookie( ResultExecutingContext filterContext, string myUserId, string actionName )
        {
            if ( actionName != "introandcovenant" )
            {
                var covenantCookie = filterContext.HttpContext.Request.Cookies[ "Covenant" ];
                if ( covenantCookie == null && filterContext.HttpContext.User.IsInRole( "Warrior" ) )
                {
                    var hasBeenSigned = await covenantProvider.ContractHasBeenSigned( new Guid( myUserId ) );
                    if ( !hasBeenSigned )
                    {
                        filterContext.HttpContext.Response.Redirect( @"/Dashboard/IntroAndCovenant" );
                    }
                    else
                    {
                        filterContext.HttpContext.Response.Cookies.Append( "Covenant", "true" );
                    }
                }
            }
        }

        private async Task LoadGuardianCookies( ResultExecutingContext filterContext, string myUserId, string actionName )
        {
            var isGuardian = filterContext.HttpContext.User.IsInRole( "Guardian" );
            if ( isGuardian )
            {
                if ( actionName != "guardianintro" )
                {
                    var covenantCookie = filterContext.HttpContext.Request.Cookies[ "GuardianIntroVideo" ];
                    if ( covenantCookie == null )
                    {
                        var hasBeenSigned = await _guardianIntroProvider.GuardianHasWatchedIntroVideo( new Guid( myUserId ) );
                        if ( !hasBeenSigned )
                        {
                            filterContext.HttpContext.Response.Redirect( @"/Dashboard/GuardianIntro" );
                            return;
                        }
                        else
                        {
                            filterContext.HttpContext.Response.Cookies.Append( "GuardianIntroVideo", "true" );

                        }
                    }

                    if ( actionName != "subscription" )
                    {
                        var isAdmin = filterContext.HttpContext.User.IsInRole( "Admin" );
                        if ( isGuardian && !isAdmin )
                        {
                            var subscriptionCookie = filterContext.HttpContext.Request.Cookies[ "HasSubscription" ];
                            if ( subscriptionCookie == null && !valuesHolder.HasActiveSubscription )
                            {
                                filterContext.HttpContext.Response.Redirect( @"/Manage/Subscription" );
                                return;
                            }
                            else
                            {
                                filterContext.HttpContext.Response.Cookies.Append( "HasSubscription", "true" );
                            }
                        }
                    }
                }
            }
        }
    }
}