using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Schedify.Data;
using Schedify.Hubs;
using Schedify.Models;
using Schedify.Services;
using Schedify.ViewModels;

namespace Schedify.Controllers;

[Authorize(Roles = "Organizer")]
public class OrganizerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<AlertHub> _hubContext;
    private readonly EventService _eventService;
    private readonly ResourceService _resourceService;
    private readonly UserService _userService;
    public OrganizerController(ApplicationDbContext context, EventService eventService, ResourceService resourceService, UserService userService, IHubContext<AlertHub> hubContext)
    {
        _context = context;
        _eventService = eventService;
        _resourceService = resourceService;
        _userService = userService;
        _hubContext = hubContext;
    }

    [HttpPost("organizer/save-connection-id")]
    public IActionResult SaveConnectionId(string connectionId)
    {
        string? userId = _userService.GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        AlertHub.SaveConnectionId(userId, connectionId); // Save user’s connection ID somewhere
        return Ok();
    }

    [HttpGet("organizer/events")]
    public IActionResult Events(DateTime? startDate, DateTime? endDate)
    {
        var viewModel = GetEventsViewModel(startDate, endDate);

        if (Request.Headers["HX-Request"].Any())
        {
            return PartialView("~/Views/Organizer/Partials/_EventsListPartial.cshtml", viewModel);
        }

        return View(viewModel);
    }

    [HttpGet("organizer/view-event/{id}")]
    public async Task<IActionResult> ViewEvent(Guid id)
    {
        var evt = await _eventService.GetEventByIdAsync(id);

        if (evt == null) return NotFound();

        bool hasVenue = _eventService.IsEventHasVenue(evt.Id);

        var model = new ViewEventViewModel()
        {
            Id = evt.Id,
            Name = evt.Name,
            Description = evt.Description,
            StartAt = evt.StartAt,
            EndAt = evt.EndAt,
            Status = evt.Status,
            EntryFee = evt.EntryFee.ToString("N2"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EventHasVenue = hasVenue,
        };

        return PartialView("~/Views/Organizer/Partials/_ViewEventPartial.cshtml", model);
    }

    [HttpPost("organizer/create-event")]
    public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
    {
        var result = await _eventService.CreateEventAsync(model);

        if (!result.IsSuccess)
        {
            // Split error messages and add them to ModelState
            foreach (var error in result.Error!)
            {
                ModelState.AddModelError(error.Key, error.Value);
                if (error.Key == "Authentication")
                {
                    Response.Headers.Append("HX-Redirect", Url.Action("Login", "Auth"));
                }
            }

            return PartialView("_ValidationMessages", ModelState);
        }

        ModelState.Clear();
        var viewModel = GetEventsViewModel(null, null);

        // Define the SweetAlert event in the HX-Trigger header
        var sweetAlertJson = "{\"closeModal\": true, \"clearValidations\": true, \"showSweetAlert\": { \"title\": \"Successfully created an event!\", \"icon\": \"success\", \"timer\": 3000 }}";

        Response.Headers.Append("HX-Trigger", sweetAlertJson);
        return PartialView("~/Views/Organizer/Partials/_UpdateEventsListPartial.cshtml", viewModel);
    }

    [HttpGet("organizer/show-update-event/{id}")]
    public async Task<IActionResult> ShowUpdateEvent(Guid id)
    {
        var _event = await _eventService.GetEditEventByIdAsync(id);

        if (_event == null)
        {
            return NotFound();
        }

        if (_event.Status != EventStatus.Draft)
        {
            // Redirect to events list when event status is not Draft
            Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer"));
            return Content(string.Empty);
        }

        return PartialView("~/Views/Organizer/Partials/_UpdateEventPartial.cshtml", _event);
    }

    [HttpPost("organizer/update-event")]
    public async Task<IActionResult> UpdateEvent(UpdateEventViewModel model)
    {
        var result = await _eventService.UpdateEventAsync(model);

        if (!result.IsSuccess)
        {
            // Split error messages and add them to ModelState
            foreach (var error in result.Error!)
            {
                ModelState.AddModelError(error.Key, error.Value);
                if (error.Key == "Authentication")
                {
                    Response.Headers.Append("HX-Redirect", Url.Action("Login", "Auth"));
                    return Content(string.Empty);
                }

                if (error.Key == "NoChanges")
                {
                    Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer"));
                    return Content(string.Empty);
                }

                if (error.Key == "InvalidStatus")
                {
                    Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer"));
                    return Content(string.Empty);
                }
            }

            return PartialView("_ValidationEditMessages", ModelState);
        }

        ModelState.Clear();
        var viewModel = GetEventsViewModel(null, null);

        // Define the SweetAlert event in the HX-Trigger header
        var sweetAlertJson = "{\"closeModal\": true, \"clearValidations\": true, \"showSweetAlert\": { \"title\": \"Successfully updated an event!\", \"icon\": \"success\", \"timer\": 3000 }}";

        Response.Headers.Append("HX-Trigger", sweetAlertJson);
        return PartialView("~/Views/Organizer/Partials/_UpdateEventsListPartial.cshtml", viewModel);
    }

    [HttpDelete("organizer/delete-event/{id}")]
    public async Task<IActionResult> DeleteResource(Guid id)
    {
        var success = await _eventService.DeleteEventAsync(id);
        if (!success)
        {
            return NotFound(); // Return 404 if resource doesn't exist
        }

        Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer")); // Redirect to resource list after deletion
        return Content(string.Empty);
    }

    [HttpPatch("organizer/open-event/{id}")]
    public async Task<IActionResult> OpenEvent(Guid id)
    {
        var success = await _eventService.OpenEventByIdAsync(id);

        if (!success)
        {
            return NotFound();
        }

        Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer")); // Redirect to resource list after deletion
        return Content(string.Empty);
    }

    [HttpPatch("organizer/draft-event/{id}")]
    public async Task<IActionResult> DraftEvent(Guid id)
    {
        var success = await _eventService.DraftEventByIdAsync(id);

        if (!success)
        {
            return NotFound();
        }

        Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer")); // Redirect to resource list after deletion
        return Content(string.Empty);
    }

    [HttpPatch("organizer/cancel-event/{id}")]
    public async Task<IActionResult> CancelEvent(Guid id)
    {
        var success = await _eventService.CancelEventByIdAsync(id);

        if (!success)
        {
            return NotFound();
        }

        Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer")); // Redirect to resource list after deletion
        return Content(string.Empty);
    }

    [HttpGet("organizer/show-postpone-event/{id}")]
    public async Task<IActionResult> ShowPostponeEvent(Guid id)
    {
        // Changing the status to "Postponed" is techincally an "Edit/Update"
        var _event = await _eventService.GetEditEventByIdAsync(id);

        if (_event == null)
        {
            return NotFound();
        }

        if (_event.Status == EventStatus.Open || _event.Status == EventStatus.Ongoing)
        {
            return PartialView("~/Views/Organizer/Partials/_PostponeEventPartial.cshtml", _event);
        }

        // Redirect to events list when event status is not Open or Ongoing
        Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer"));
        return Content(string.Empty);
    }

    [HttpPost("organizer/postpone-event")]
    public async Task<IActionResult> PostponeEvent(UpdateEventViewModel model)
    {
        var result = await _eventService.PostponeEventByIdAsync(model);

        if (!result.IsSuccess)
        {
            // Split error messages and add them to ModelState
            foreach (var error in result.Error!)
            {
                if (error.Key == "Authentication")
                {
                    Response.Headers.Append("HX-Redirect", Url.Action("Login", "Auth"));
                    return Content(string.Empty);
                }

                if (error.Key == "InvalidStatus")
                {
                    Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer"));
                    return Content(string.Empty);
                }

                if (error.Key == "NoChanges")
                {
                    ModelState.AddModelError("CustomError", error.Value);
                    break;
                }

                ModelState.AddModelError(error.Key, error.Value);
            }

            return PartialView("_ValidationEditMessages", ModelState);
        }

        Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer"));
        return Content(string.Empty);
    }

    // --------------------------------------------------------------------------------------
    // Resources
    // --------------------------------------------------------------------------------------

    [HttpPost("organizer/resources/set-event")]
    public IActionResult SetEventSession(Guid eventId)
    {
        HttpContext.Session.SetString("SelectedEventId", eventId.ToString());
        return Ok();
    }

    [HttpGet("organizer/resources")]
    public IActionResult Resources()
    {
        var eventIdString = HttpContext.Session.GetString("SelectedEventId");

        List<Event>? events = _eventService.GetEventsByUser();

        Event? evt = null;

        // Get the Event Id to pass to the hidden input
        if (eventIdString != null)
        {
            evt = _eventService.GetEventById(Guid.Parse(eventIdString));
        }
        else if (events!.Any() != false)
        {
            evt = _eventService.GetEventById(events!.First().Id);
        }

        // if there's no event, redirect to Events page
        if (evt == null)
        {
            Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer"));
            return RedirectToAction("Events", "Organizer");
        }

        var resources = _resourceService.GetResourcesByEventId(evt.Id);

        var model = new ResourceViewModel
        {
            EventId = evt.Id,
            Events = events!,
            SelectedName = evt.Name,
            Resources = resources!,
            ResourceImages = _resourceService.GetResourceImageFromList(resources!),
        };

        return View(model);
    }

    [HttpGet("organizer/select-event-dropdown/{EventId}")]
    public async Task<IActionResult> GetEventsDropdown(Guid EventId)
    {
        var events = await _eventService.GetEventsForDropdown();
        var evt = _eventService.GetEventById(EventId);

        if (evt == null)
        {
            Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer"));
            return RedirectToAction("Events", "Organizer");
        }

        var resources = _resourceService.GetResourcesByEventId(EventId);

        var model = new ResourceViewModel
        {
            EventId = EventId,
            Events = events,
            SelectedName = evt != null ? evt.Name : events.First().Name,
            Resources = resources!,
            ResourceImages = _resourceService.GetResourceImageFromList(resources!),
        };

        return PartialView("~/Views/Organizer/Partials/_ResourceListPartial.cshtml", model);
    }

    // ----------------------------------------------
    // Create Resource Modal requests
    // ----------------------------------------------

    // called when opening the CreateResourceModal
    [HttpGet("organizer/open-create-resource-modal/{EventId}")]
    public IActionResult OpenResourceModal(Guid EventId)
    {
        var model = new CreateResourceViewModel
        {
            EventId = EventId
        };

        return PartialView("~/Views/Organizer/Partials/CreateResourceModal.cshtml", model);
    }

    // used in CreateResourceModal
    [HttpGet("organizer/select-resource-type")]
    public IActionResult ResourceExtra(ResourceType selectedOption)
    {
        CreateResourceViewModel viewModel = selectedOption switch
        {
            ResourceType.Venue => new CreateResourceViewModel { ResourceType = ResourceType.Venue },
            ResourceType.Equipment => new CreateResourceViewModel { ResourceType = ResourceType.Equipment },
            ResourceType.Furniture => new CreateResourceViewModel { ResourceType = ResourceType.Furniture },
            ResourceType.Catering => new CreateResourceViewModel { ResourceType = ResourceType.Catering },
            ResourceType.Personnel => new CreateResourceViewModel { ResourceType = ResourceType.Personnel },
            _ => new CreateResourceViewModel { ResourceType = ResourceType.Venue } // Default
        };
        return PartialView("_SelectResourceTypePartial", viewModel);
    }

    // if Furniture is the resource type
    [HttpGet("organizer/select-furniture-material")]
    public IActionResult SelectFurnitureMaterial(FurnitureMaterial selectedOption)
    {
        CreateResourceViewModel viewModel = selectedOption switch
        {
            FurnitureMaterial.Wood => new CreateResourceViewModel { Material = FurnitureMaterial.Wood },
            FurnitureMaterial.Metal => new CreateResourceViewModel { Material = FurnitureMaterial.Metal },
            FurnitureMaterial.Plastic => new CreateResourceViewModel { Material = FurnitureMaterial.Plastic },
            FurnitureMaterial.Glass => new CreateResourceViewModel { Material = FurnitureMaterial.Glass },
            FurnitureMaterial.Fabric => new CreateResourceViewModel { Material = FurnitureMaterial.Fabric },
            FurnitureMaterial.Other => new CreateResourceViewModel { Material = FurnitureMaterial.Other },
            _ => new CreateResourceViewModel { Material = FurnitureMaterial.Wood } // Default
        };

        if (selectedOption == FurnitureMaterial.Other)
            viewModel.IsOtherMaterial = true;
        else
            viewModel.IsOtherMaterial = false;

        viewModel.ResourceType = ResourceType.Furniture;
        return PartialView("_SelectResourceTypePartial", viewModel);
    }

    [HttpGet("organizer/cost-types")]
    public IActionResult GetCostTypes(string selectedOption)
    {
        if (Enum.TryParse<ResourceType>(selectedOption, out var resourceType))
        {
            return PartialView("_CostTypeOptionsPartial", resourceType);
        }

        return BadRequest();
    }

    [HttpPost("organizer/create-resource")]
    public async Task<IActionResult> CreateResource(Guid EventId, CreateResourceViewModel model)
    {
        var result = await _resourceService.CreateResourceAsync(model, EventId);

        if (!result.IsSuccess)
        {
            // Split error messages and add them to ModelState
            foreach (var error in result.Error!)
            {
                ModelState.AddModelError(error.Key, error.Value);
            }

            return PartialView("_ValidationMessages", ModelState);
        }

        // Once everything is clear with no errors, it's time to hx-swap
        ModelState.Clear();
        List<Event>? events = _eventService.GetEventsByUser();

        Event? evt = _eventService.GetEventById(EventId);

        // if there's no event, redirect to Events page
        if (evt == null)
        {
            Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer"));
            return RedirectToAction("Events", "Organizer");
        }

        var resources = _resourceService.GetResourcesByEventId(EventId);

        var viewModel = new ResourceViewModel
        {
            EventId = EventId,
            Events = events!,
            SelectedName = evt.Name,
            Resources = resources!,
            ResourceImages = _resourceService.GetResourceImageFromList(resources!),
        };

        // Define the SweetAlert event in the HX-Trigger header
        var sweetAlertJson = "{\"closeModal\": true, \"clearValidations\": true, \"showSweetAlert\": { \"title\": \"Successfully added a resource!\", \"icon\": \"success\", \"timer\": 3000 }}";
        Response.Headers.Append("HX-Trigger", sweetAlertJson);
        return PartialView("~/Views/Organizer/Partials/_UpdateResourcesListPartial.cshtml", viewModel);
    }

    // ----------------------------------------------
    // Create Resource Modal requests
    // ----------------------------------------------

    // ----------------------------------------------
    // View Resource Modal requests
    // ----------------------------------------------

    [HttpGet("organizer/view-resource/{ResourceId}")]
    public async Task<IActionResult> ViewResource(Guid ResourceId)
    {
        var resource = await _resourceService.GetResourceByIdAsync(ResourceId);
        if (resource == null) return NotFound();

        var resourceWithType = await _resourceService.GetResourceByTypeAsync(ResourceId, resource.ResourceType);
        if (resourceWithType == null) return NotFound();

        var viewModel = GetViewResourceViewModel(resourceWithType);

        return PartialView("~/Views/Organizer/Partials/_ViewResourcePartial.cshtml", viewModel);
    }

    // ----------------------------------------------
    // View Resource Modal requests
    // ----------------------------------------------

    // [Route("organizer/resource-change-page/{change}")]
    // [HttpGet]
    // public IActionResult ChangePage(ResourceType type, int change, int currentPage)
    // {
    //     int pageSize = 8; // Default page size
    //     int newPage = currentPage + change; // Calculate new page number

    //     if (newPage < 1) newPage = 1; // Prevent negative pages
    //     var totalCount = _context.Resources.Count(r => r.ResourceType == type);
    //     var resources = _resourceService.GetResourcesByType(type, newPage, pageSize);

    //     var viewModel = new ResourceViewModel
    //     {
    //         Resources = resources!,
    //         ResourceImages = _resourceService.GetResourceImageFromList(resources!),
    //         CurrentPage = newPage,
    //         PageSize = pageSize,
    //         TotalCount = totalCount
    //     };

    //     return View("~/Views/Organizer/Resources.cshtml");
    // }

    // --------------------------------------------------------------------------------------
    // Feedbacks
    // --------------------------------------------------------------------------------------


    [Route("organizer/feedbacks")]
    [HttpGet]
    public IActionResult Feedbacks() => View();

    [Route("organizer/stats")]
    [HttpGet]
    public IActionResult Stats() => View();

    // --------------------------------------------------------------------------------------
    // Private Methods
    // --------------------------------------------------------------------------------------

    private EventsViewModel GetEventsViewModel(DateTime? startDate, DateTime? endDate)
    {
        // Set default values if not provided
        startDate ??= new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc); // Jan 1st, 00:00 UTC
        endDate ??= DateTime.UtcNow.Date.AddHours(23).AddMinutes(59); // Today at 23:59 UTC

        var DraftEvents = _eventService.GetEventsByOrganizerDraft(startDate, endDate?.AddHours(23).AddMinutes(59));
        var PublishedEvents = _eventService.GetEventsByOrganizerPublished(startDate, endDate?.AddHours(23).AddMinutes(59));
        var ConcludedEvents = _eventService.GetEventsByOrganizerConcluded(startDate, endDate?.AddHours(23).AddMinutes(59));

        var pAttendeeCount = _eventService.GetAttendeeCounts(PublishedEvents);
        var cAttendeeCount = _eventService.GetAttendeeCounts(ConcludedEvents);
        var MergedAttendeeCounts = pAttendeeCount.Concat(cAttendeeCount)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value); // kvp == key-value pair

        var hasResources = _context.Events
            .ToDictionary(
                e => e.Id,
                e => e.Resources.Any()
            );

        var viewModel = new EventsViewModel
        {
            DraftEvents = DraftEvents,
            PublishedEvents = PublishedEvents,
            ConcludedEvents = ConcludedEvents,
            EventAttendeeCounts = MergedAttendeeCounts,
            HasResources = hasResources
        };

        return viewModel;
    }

    private ViewResourceViewModel? GetViewResourceViewModel(Resource? resource)
    {
        if (resource == null) return null;
        
        var viewModel = new ViewResourceViewModel
        {
            Id = resource.Id,
            ImageFileName = resource.Image!.ImageFileName,
            ProviderName = resource.ProviderName,
            ProviderEmail = resource.ProviderEmail,
            ProviderPhoneNumber = resource.ProviderPhoneNumber,
            Name = resource.Name,
            Description = resource.Description,
            ResourceType = resource.ResourceType,
            Cost = resource.Cost,
            CostType = resource.CostType,
            CreatedAt = resource.CreatedAt,
            UpdatedAt = resource.UpdatedAt,
        };

        if (resource.ResourceType == ResourceType.Venue)
        {
            viewModel.Capacity = resource.ResourceVenue.Capacity;
            viewModel.Size = resource.ResourceVenue.Size;
            viewModel.AddressLine1 = resource.ResourceVenue.AddressLine1;
            viewModel.AddressLine2 = resource.ResourceVenue.AddressLine2;
            viewModel.CityMunicipality = resource.ResourceVenue.CityMunicipality;
            viewModel.Province = resource.ResourceVenue.Province;
        }
        else if (resource.ResourceType == ResourceType.Equipment)
        {
            viewModel.Quantity = resource.ResourceEquipment.Quantity;
            viewModel.Brand = resource.ResourceEquipment.Brand;
            viewModel.Warranty = resource.ResourceEquipment.Warranty;
            viewModel.Specifications = JsonSerializer.Deserialize<Dictionary<string, string>>(resource.ResourceEquipment.Specifications!)!;
        }
        else if (resource.ResourceType == ResourceType.Furniture)
        {
            viewModel.Quantity = resource.ResourceFurniture.Quantity;
            viewModel.Material = resource.ResourceFurniture.Material;
            viewModel.OtherMaterial = resource.ResourceFurniture.OtherMaterial;
            viewModel.Dimensions = resource.ResourceFurniture.Dimensions;
            viewModel.Warranty = resource.ResourceFurniture.Warranty;
        }
        else if (resource.ResourceType == ResourceType.Catering)
        {
            viewModel.GuestCapacity = resource.ResourceCatering.GuestCapacity;
            viewModel.MenuItems = JsonSerializer.Deserialize<List<string>>(resource.ResourceCatering.MenuItems);
        }
        else if (resource.ResourceType == ResourceType.Personnel)
        {
            viewModel.Position = resource.ResourcePersonnel.Position;
            viewModel.ShiftStart = resource.ResourcePersonnel.ShiftStart;
            viewModel.ShiftEnd = resource.ResourcePersonnel.ShiftEnd;
            viewModel.Experience = resource.ResourcePersonnel.Experience;
        }

        return viewModel;
    }
}
