using Microsoft.AspNetCore.Mvc;

namespace Schedify.Controllers.Organizer
{
    public class OrganizerController : Controller
    {
        [Route("organizer/dashboard")]
        [HttpGet]
        public ActionResult Dashboard()
        {
            return View();
        }

        [Route("organizer/events")]
        [HttpGet]
        public ActionResult Events()
        {
            return View();
        }

        [Route("organizer/resources")]
        [HttpGet]
        public ActionResult Resources()
        {
            return View();
        }
    }
}
