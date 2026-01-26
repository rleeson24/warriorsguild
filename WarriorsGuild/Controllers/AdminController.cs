using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarriorsGuild.Models;

namespace WarriorsGuild.Controllers
{
    [Authorize( Policy = "MustBeAdmin" )]
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
        // GET: Admin
        public ActionResult Invite()
        {
            return View();
        }

        // GET: Admin
        public ActionResult ManagePriceOptions()
        {
            return View( new PaymentUrls
            {
                PriceOptionsUrl = "/api/PriceOptions"
            } );
        }
    }
}