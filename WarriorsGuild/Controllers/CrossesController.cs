using Microsoft.AspNetCore.Mvc;

namespace WarriorsGuild.Controllers
{
    public class CrossesController : Controller
    {
        public ActionResult Index()
        {
            var urls = new Models.CrossUrls();
            urls.CrossesUrl = "/api/crosses";
            urls.CrossStatusUrl = "/api/crossstatus/";
            urls.PublicCrossUrl = "/api/crosses/public";
            //urls.RecordCompletion = "/api/rankstatus/RecordCompletion";
            urls.ImageUploadBaseUrl = "api/crosses/UploadImage";
            urls.UploadGuideUrl = "api/crosses/UploadGuide";
            urls.DownloadGuideUrl = "api/crosses/guide";
            urls.ImageBaseUrl = "/images/crosses";
            return View( urls );
        }
    }
}
