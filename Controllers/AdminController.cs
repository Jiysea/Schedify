



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
        var viewModel = new ResourceViewModel
        {
            Resources = _resourceService.GetResources(),
            ResourceImages = _resourceService.GetResourceImages()
        };

        return View(viewModel);
    }

    public IActionResult ViewResource(string Id)
    {
        return View();
    }

    [Route("admin/activity-logs")]
    public ActionResult ActivityLogs() => View();

    public ActionResult Test() => View();

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

        // Custom Validations

        if (model.ImageFile == null)
        {
            ModelState.AddModelError(nameof(model.ImageFile), "An image is required.");
        }

        if (model.Type == ResourceType.Venue)
        {
            if (string.IsNullOrWhiteSpace(model.AddressLine1))
            {
                ModelState.AddModelError(nameof(model.AddressLine1), "This field is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Size))
            {
                ModelState.AddModelError(nameof(model.Size), "This field is required.");
            }

            if (model.Capacity <= 0)
            {
                ModelState.AddModelError(nameof(model.Capacity), "Value must be at least 1.");
            }
        }

        if (model.Type == ResourceType.Equipment)
        {

            if (string.IsNullOrWhiteSpace(model.Brand))
            {
                ModelState.AddModelError(nameof(model.Brand), "This field is required.");
            }

            if (model.Specifications == null || model.Specifications.Count == 0)
            {
                ModelState.AddModelError(nameof(model.Specifications), "Specifications cannot be empty.");
            }
        }

        if (model.Type == ResourceType.Furniture)
        {
            if (string.IsNullOrWhiteSpace(model.Material))
            {
                ModelState.AddModelError(nameof(model.Material), "This field is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Dimensions))
            {
                ModelState.AddModelError(nameof(model.Dimensions), "This field is required.");
            }
        }

        if (model.Type == ResourceType.Catering)
        {
            if (string.IsNullOrWhiteSpace(model.MenuItems))
            {
                ModelState.AddModelError(nameof(model.MenuItems), "This field is required.");
            }

            if (string.IsNullOrWhiteSpace(model.PriceItems))
            {
                ModelState.AddModelError(nameof(model.PriceItems), "This field is required.");
            }
        }

        if (model.Type == ResourceType.Personnel)
        {
            Console.WriteLine(model.ShiftAsString);
            if (string.IsNullOrWhiteSpace(model.Position))
            {
                ModelState.AddModelError(nameof(model.Position), "This field is required.");
            }

            if (string.IsNullOrWhiteSpace(model.ShiftAsString))
            {
                ModelState.AddModelError(nameof(model.ShiftAsString), "This field is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Experience))
            {
                ModelState.AddModelError(nameof(model.Experience), "This field is required.");
            }
        }

        // Sending all validation errors to frontend
        if (!ModelState.IsValid)
        {
            return PartialView("_ValidationMessages", ModelState);
        }

        var user = await _userService.GetUserAsync();

        if (user == null)
        {
            throw new InvalidOperationException("User must be authenticated.");
        }

        var resource = new Resource
        {
            UserId = user.Id,
            ProviderName = null!,
            ProviderPhoneNumber = null!,
            ProviderEmail = null!,
            Name = null!,
            Description = null!,
            Type = model.Type,
            Cost = 0.00m,
            CostType = null!,
            Quantity = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            switch (model.Type)
            {
                case ResourceType.Venue:
                    resource = new Resource
                    {
                        UserId = user.Id,
                        ProviderName = model.ProviderName,
                        ProviderPhoneNumber = model.ProviderPhoneNumber,
                        ProviderEmail = model.ProviderEmail,
                        Name = model.Name,
                        Description = model.Description,
                        Type = model.Type,
                        Cost = model.CostAsDecimal,
                        CostType = model.CostType,
                        Quantity = 1,
                        Capacity = model.Capacity,
                        Size = model.Size,
                        AddressLine1 = model.AddressLine1,
                        AddressLine2 = model.AddressLine2,
                        CityMunicipality = "Davao City",
                        Province = "Davao del Sur",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Resources.Add(resource);
                    break;
                case ResourceType.Equipment:
                    string specificationsJson = JsonSerializer.Serialize(model.Specifications);
                    resource = new Resource
                    {
                        UserId = user.Id,
                        ProviderName = model.ProviderName,
                        ProviderPhoneNumber = model.ProviderPhoneNumber,
                        ProviderEmail = model.ProviderEmail,
                        Name = model.Name,
                        Description = model.Description,
                        Type = model.Type,
                        Cost = model.CostAsDecimal,
                        CostType = model.CostType,
                        Quantity = 1,
                        Brand = model.Brand,
                        Specifications = specificationsJson,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Resources.Add(resource);
                    break;
                case ResourceType.Furniture:
                    resource = new Resource
                    {
                        UserId = user.Id,
                        ProviderName = model.ProviderName,
                        ProviderPhoneNumber = model.ProviderPhoneNumber,
                        ProviderEmail = model.ProviderEmail,
                        Name = model.Name,
                        Description = model.Description,
                        Type = model.Type,
                        Cost = model.CostAsDecimal,
                        CostType = model.CostType,
                        Quantity = 1,
                        Material = model.Material,
                        Dimensions = model.Dimensions,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Resources.Add(resource);
                    break;
                case ResourceType.Catering:
                    resource = new Resource
                    {
                        UserId = user.Id,
                        ProviderName = model.ProviderName,
                        ProviderPhoneNumber = model.ProviderPhoneNumber,
                        ProviderEmail = model.ProviderEmail,
                        Name = model.Name,
                        Description = model.Description,
                        Type = model.Type,
                        Cost = model.CostAsDecimal,
                        CostType = model.CostType,
                        Quantity = 1,
                        MenuItems = model.MenuItems,
                        PriceItems = model.PriceItems,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Resources.Add(resource);
                    break;
                case ResourceType.Personnel:
                    resource = new Resource
                    {
                        UserId = user.Id,
                        ProviderName = model.ProviderName,
                        ProviderPhoneNumber = model.ProviderPhoneNumber,
                        ProviderEmail = model.ProviderEmail,
                        Name = model.Name,
                        Description = model.Description,
                        Type = model.Type,
                        Cost = model.CostAsDecimal,
                        CostType = model.CostType,
                        Quantity = 1,
                        Position = model.Position,
                        Shift = model.ShiftAsString,
                        Experience = model.Experience,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Resources.Add(resource);
                    break;
            }

            await _context.SaveChangesAsync();

            string newFileName = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            newFileName += Path.GetExtension(model.ImageFile!.FileName);
            string imageFullPath = _environment.WebRootPath + "/resources/" + newFileName;

            // Ensure the directory exists
            string directoryPath = Path.GetDirectoryName(imageFullPath)!;
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (var stream = System.IO.File.Create(imageFullPath))
            {
                await model.ImageFile.CopyToAsync(stream);
            }

            Image image = new Image
            {
                ResourceId = resource.Id,
                ImageFileName = newFileName
            };

            _context.Images.Add(image);

            // Save to the database
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            Response.Headers.Append("HX-Redirect", Url.Action("Resources", "Admin"));

            return Content(string.Empty);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            await transaction.RollbackAsync();
            ModelState.AddModelError("", "An error occurred while saving data.");
            Response.Headers.Append("HX-Redirect", Url.Action("Resources", "Admin"));
            return View("~/Views/Admin/Resources.cshtml", model);
        }

    }

}