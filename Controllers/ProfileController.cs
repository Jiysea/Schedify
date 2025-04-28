using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Schedify.Controllers
{
    public class ProfileController : Controller
    {

        [Authorize(Roles = "Organizer")]
        [HttpGet("organizer/my-profile")]
        public ActionResult OrganizerMyProfile()
        {
            return View();
        }

    }
}
