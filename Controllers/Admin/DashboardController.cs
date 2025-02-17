using Microsoft.AspNetCore.Mvc;

namespace Schedify.Controllers.Admin
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        [Route("admin/dashboard")]
        public ActionResult Index()
        {
            return View("~/Views/Admin/Dashboard/Index.cshtml");
        }

    }
}
