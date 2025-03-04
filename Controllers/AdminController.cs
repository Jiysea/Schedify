



using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Schedify.Models;
using Schedify.ViewModels;
using Schedify.Data;

namespace Schedify.Controllers;


[Authorize(Roles = "Admin")]
public class AdminController : Controller
{

    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

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

    [Route("admin/resource-extra")]
    [HttpGet]
    public IActionResult ResourceExtra(ResourceType selectedOption)
    {
        AddResourceViewModel viewModel = selectedOption switch
        {
            ResourceType.Venue => new AddResourceViewModel { Type = ResourceType.Venue },
            ResourceType.Equipment => new AddResourceViewModel { Type = ResourceType.Equipment },
            ResourceType.Furniture => new AddResourceViewModel { Type = ResourceType.Furniture },
            ResourceType.Catering => new AddResourceViewModel { Type = ResourceType.Catering },
            ResourceType.Personnel => new AddResourceViewModel { Type = ResourceType.Personnel },
            _ => new AddResourceViewModel { Type = ResourceType.Venue } // Default
        };

        return PartialView("~/Views/Admin/Partials/_ResourceExtraPartial.cshtml", viewModel);
    }

    [Route("admin/cost-types")]
    [HttpGet]
    public IActionResult GetCostTypes(string selectedOption)
    {
        if (Enum.TryParse<ResourceType>(selectedOption, out var resourceType))
        {
            return PartialView("~/Views/Admin/Partials/_CostTypeOptionsPartial.cshtml", resourceType);
        }

        return BadRequest();
    }

    [Route("admin/create-resource")]
    [HttpPost]
    public IActionResult CreateResource([FromForm] AddResourceViewModel model)
    {
        // if (!ModelState.IsValid)
        // {
        //     return Json(model);
        // }

        // var resource = new Resource
        // {
        //     Name = model.Name,
        //     Description = model.Description,
        //     Cost = model.CostAsDecimal,
        //     CostType = model.CostType,
        //     Capacity = model.Capacity,
        //     Quantity = model.Quantity,
        //     Size = model.Size,
        //     AddressLine1 = model.AddressLine1,
        //     AddressLine2 = model.AddressLine2,
        //     CityMunicipality = "Davao City",
        //     Province = "Davao del Sur",
        // };
        // Save to the database
        // _context.SaveChanges();


        return Json(model);
    }
}