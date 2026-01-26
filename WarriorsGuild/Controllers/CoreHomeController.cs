using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WarriorsGuild.Models;

namespace WarriorsGuild.Controllers
{
    public class CoreHomeController : Controller
    {
        private readonly ILogger<CoreHomeController> _logger;

        public CoreHomeController( ILogger<CoreHomeController> logger )
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache( Duration = 0, Location = ResponseCacheLocation.None, NoStore = true )]
        public IActionResult Error()
        {
            return View( new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier } );
        }
    }
}
