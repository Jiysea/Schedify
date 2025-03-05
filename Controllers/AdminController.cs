



using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Schedify.Models;
using Schedify.ViewModels;
using Schedify.Data;
using Schedify.Services;

namespace Schedify.Controllers;


[Authorize(Roles = "Admin")]
public class AdminController : Controller
{

    private readonly ApplicationDbContext _context;
    private readonly UserService _userService;

    public AdminController(ApplicationDbContext context, UserService userService)
    {
        _context = context;
        _userService = userService;
    }

    [Route("admin/dashboard")]
    public ActionResult Dashboard()
    {
        return View();
    }

    [Route("admin/resources")]
    public ActionResult Resources()
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
    public async Task<IActionResult> CreateResource([FromForm] AddResourceViewModel model)
    {

        var user = await _userService.GetUserAsync();
        Console.WriteLine($"{user?.Id}");
        if (!ModelState.IsValid)
        {
            return PartialView("_ValidationMessages", ModelState);
        }

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