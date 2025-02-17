using Microsoft.AspNetCore.Mvc;

namespace Schedify.Controllers.Organizer
{
    [Area("Organizer")]
    public class DashboardController : Controller
    {
        [Route("organizer/dashboard")]
        public ActionResult Index()
        {
            return View("~/Views/Organizer/Dashboard/Index.cshtml");
        }

    }
}
