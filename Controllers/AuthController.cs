using Microsoft.AspNetCore.Mvc;

namespace Schedify.Controllers
{
    public class AuthController : Controller
    {
        [Route("login")]
        public ActionResult Index()
        {
            return View("Login");
        }

        [HttpPost("login/organizer")]
        public ActionResult GoToDashboard()
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Organizer" });
        }
    }
}
