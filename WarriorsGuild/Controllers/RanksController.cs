

using Microsoft.AspNetCore.Mvc;

namespace WarriorsGuild.Controllers
{
    public class RanksController : Controller
    {
        public ActionResult Index()
        {
            var urls = new Models.RanksUrls();
            urls.RanksUrl = "/api/ranks";
            urls.RankStatusUrl = "/api/rankstatus";
            urls.PublicRankUrl = "/api/ranks/public";
            urls.RecordCompletion = "/api/rankstatus/RecordCompletion";
            urls.ImageUploadBaseUrl = "api/ranks/UploadImage";
            urls.UploadGuideUrl = "api/ranks/UploadGuide";
            urls.DownloadGuideUrl = "api/ranks/Guide";
            urls.ImageBaseUrl = "/images/ranks";
            urls.RingStatusUrl = "/api/ringstatus";
            urls.CrossStatusUrl = "/api/crossStatus";
            urls.ProofOfCompletionUrl = "/api/rankstatus/ProofOfCompletion";
            urls.CrossUrl = "/api/crosses";
            return View( urls );
        }
    }
}
