using Microsoft.AspNetCore.Mvc;

namespace WarriorsGuild.Controllers
{
    public class BlogController : Controller
    {
        // GET: Blog
        public ActionResult Index( string id )
        {
            ViewBag.BlogEntryId = id;
            return View();
        }
    }
}