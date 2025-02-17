using Microsoft.AspNetCore.Mvc;

namespace Schedify.Controllers;

public class AdminController : Controller
{
    [Route("admin/dashboard")]
    public ActionResult Dashboard()
    {
        return View();
    }
}