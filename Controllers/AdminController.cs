



using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Schedify.Models;
using Schedify.ViewModels;

namespace Schedify.Controllers;


[Authorize(Roles = "Admin")]
public class AdminController : Controller
{

    // private readonly UserManager<User> _userManager;

    // public AdminController(UserManager<User> userManager)
    // {
    //     _userManager = userManager;
    // }

    [Route("admin/dashboard")]
    public ActionResult Dashboard()
    {

        return View();
    }

    [Route("admin/actions")]
    public ActionResult Actions()
    {

        return View(new AddResourceViewModel());
    }

    [Route("admin/activity-logs")]
    public ActionResult ActivityLogs() => View();

    [Route("admin/profile")]
    public ActionResult Profile() => View();


}