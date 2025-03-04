



using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Schedify.Models;
using Schedify.ViewModels;
using Schedify.Data;
using Schedify.Binders;

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
    public IActionResult ResourceExtra(ResourceType selectedOption = ResourceType.Venue)
    {
        AddResourceViewModel viewModel = selectedOption switch
        {
            ResourceType.Venue => new VenueViewModel { Type = ResourceType.Venue },
            ResourceType.Equipment => new EquipmentViewModel { Type = ResourceType.Equipment },
            ResourceType.Furniture => new FurnitureViewModel { Type = ResourceType.Furniture },
            ResourceType.Catering => new CateringViewModel { Type = ResourceType.Catering },
            ResourceType.Personnel => new PersonnelViewModel { Type = ResourceType.Personnel },
            _ => new VenueViewModel { Type = ResourceType.Venue } // Default
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
        return Json(model);
        Console.WriteLine(model);
        // if (!ModelState.IsValid)
        // {
        //     return Json(model);
        // }

        switch (model) // Assuming you have a property indicating the type
        {
            case VenueViewModel venueModel:
                var venue = new ResourceVenue
                {
                    Capacity = venueModel.Capacity,
                    Size = venueModel.Size,
                    AddressLine1 = venueModel.AddressLine1,
                    AddressLine2 = venueModel.AddressLine2,
                    CityMunicipality = "Davao City",
                    Province = "Davao del Sur",
                };
                // _context.ResourceVenues.Add(venue);
                break;

            case EquipmentViewModel equipmentModel:
                var equipment = new ResourceEquipment
                {
                    Brand = equipmentModel.Brand,
                    Specifications = equipmentModel.SpecificationString,
                };
                // _context.ResourceEquipments.Add(equipment);
                break;

        }
        // Save to the database
        // _context.SaveChanges();


        return Json(model);
    }
}