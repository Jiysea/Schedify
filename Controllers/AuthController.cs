using Microsoft.AspNetCore.Mvc;

namespace Schedify.Controllers
{
    public class AuthController : Controller
    {
        // GET: AuthController
        public ActionResult Login()
        {
            return View();
        }

    }
}
