

using Microsoft.AspNetCore.Mvc;

namespace WarriorsGuild.Controllers
{
    public class RingsController : Controller
    {
        public ActionResult Index()
        {
            var urls = new Models.RingsUrls();
            urls.RingsUrl = "/api/rings";
            urls.RingStatusUrl = "/api/ringstatus";
            urls.PublicRingUrl = "/api/rings/public";
            urls.RecordCompletion = "/api/ringstatus/RecordCompletion";
            urls.ImageUploadBaseUrl = "api/rings/UploadImage";
            urls.UploadGuideUrl = "api/rings/UploadGuide";
            urls.DownloadGuideUrl = "api/rings/guide";
            urls.ImageBaseUrl = "/images/rings";
            urls.ProofOfCompletionUrl = "/api/ringstatus/ProofOfCompletion";
            return View( urls );
        }
    }
}
