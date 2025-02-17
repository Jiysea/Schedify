using Microsoft.AspNetCore.Mvc;

namespace Schedify.Controllers.Organizer
{
    public class OrganizerController : Controller
    {
        [Route("organizer/dashboard")]
        public ActionResult Index()
        {
            return View();
        }

    }
}
