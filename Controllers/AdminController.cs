



using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Schedify.Models;
using Schedify.ViewModels;
using Schedify.Data;
using Schedify.Services;
using System.Text.Json;

namespace Schedify.Controllers;


[Authorize(Roles = "Admin")]
public class AdminController : Controller
{

    private readonly ApplicationDbContext _context;
    private readonly UserService _userService;
    private readonly ResourceService _resourceService;
    private readonly IWebHostEnvironment _environment;

    public AdminController(ApplicationDbContext context, UserService userService, ResourceService resourceService, IWebHostEnvironment environment)
    {
        _context = context;
        _userService = userService;
        _resourceService = resourceService;
        _environment = environment;
    }

    [Route("admin/dashboard")]
    public ActionResult Dashboard()
    {
        return View();
    }

    [Route("admin/resources")]
    public ActionResult Resources()
    {
        var resources = _resourceService.GetResources();

        var viewModel = new ResourceViewModel
        {
            Resources = resources,
            ResourceImages = _resourceService.GetResourceImages()
        };

        return View(viewModel);
    }

    [Route("admin/activity-logs")]
    public ActionResult ActivityLogs() => View();

    [Route("admin/profile")]
    public ActionResult Profile() => View();

    [Route("admin/resource-extra")]
    [HttpGet]
    public IActionResult ResourceExtra(ResourceType selectedOption)
    {
        ResourceViewModel viewModel = selectedOption switch
        {
            ResourceType.Venue => new ResourceViewModel { Type = ResourceType.Venue },
            ResourceType.Equipment => new ResourceViewModel { Type = ResourceType.Equipment },
            ResourceType.Furniture => new ResourceViewModel { Type = ResourceType.Furniture },
            ResourceType.Catering => new ResourceViewModel { Type = ResourceType.Catering },
            ResourceType.Personnel => new ResourceViewModel { Type = ResourceType.Personnel },
            _ => new ResourceViewModel { Type = ResourceType.Venue } // Default
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
    public async Task<IActionResult> CreateResource(ResourceViewModel model)
    {
        var result = await _resourceService.CreateResourceAsync(model);

        if (!result.IsSuccess)
        {
            // Split error messages and add them to ModelState
            foreach (var error in result.Error!)
            {
                ModelState.AddModelError(error.Key, error.Value);
            }

            return PartialView("_ValidationMessages", ModelState);
        }

        Response.Headers.Append("HX-Redirect", Url.Action("Resources", "Admin"));
        return Content(string.Empty);
    }

    [Route("admin/view-resource/{id}")]
    [HttpGet]
    public async Task<IActionResult> ViewResource(Guid id)
    {
        var resource = await _resourceService.GetResourceByIdAsync(id);
        
        if (resource == null)
        {
            return NotFound();
        }
        Console.WriteLine(resource.IsUsed);
        Console.WriteLine(resource.IsUsed);
        Console.WriteLine(resource.IsUsed);
        Console.WriteLine(resource.IsUsed);
        Console.WriteLine(resource.IsUsed);
        return PartialView("~/Views/Admin/Partials/_ViewResourcePartial.cshtml", resource);
    }

    [Route("admin/delete-resource/{id}")]
    [HttpDelete]
    public async Task<IActionResult> DeleteResource(Guid id)
    {
        var success = await _resourceService.DeleteResourceAsync(id);
        if (!success)
        {
            return NotFound(); // Return 404 if resource doesn't exist
        }

        Response.Headers.Append("HX-Redirect", Url.Action("Resources", "Admin")); // Redirect to resource list after deletion
        return Content(string.Empty);
    }


    // [Route("admin/view-edit-resource/{id}")]
    // [HttpGet]
    // public async Task<IActionResult> EditResource(Guid id)
    // {
    //     var resource = await _resourceService.GetResourceByIdAsync(id);

    //     if(resource == null)
    //     {
    //         return NotFound();
    //     }

    //     return PartialView("~/Views/Admin/Partials/_EditResourcePartial.cshtml", resource);
    // }
}