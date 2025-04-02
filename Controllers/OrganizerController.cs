using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
    public async Task<IActionResult> Events()
    {
        var viewModel = await GetEventsViewModel(null, null);

        // if (Request.Headers.ContainsKey("HX-Request"))
        // {
        //     return PartialView("~/Views/Organizer/Partials/_EventsListPartial.cshtml", viewModel);
        // }

        return View(viewModel);
    }

    [HttpGet("organizer/view-event/{id}")]
    public async Task<IActionResult> ViewEvent(Guid id)
    {
        var evt = await _eventService.GetEventByIdAsync(id);

        if (evt == null) return NotFound();

        bool hasVenue = await _eventService.IsEventHasVenue(evt.Id);
        bool isOpenable = await _eventService.IsEventOpenable(evt.Id);

        var viewModel = new ViewEventViewModel()
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
            IsEventOpenable = isOpenable,
        };

        return PartialView("~/Views/Organizer/Partials/_ViewEventPartial.cshtml", viewModel);
    }

    [HttpPost("organizer/create-event")]
    public async Task<IActionResult> CreateEvent(CUEventViewModel model)
    {
        var result = await _eventService.CreateEventAsync(model);
        var viewModel = await GetEventsViewModel(null, null);
        if (!result.IsSuccess)
        {
            // Split error messages and add them to ModelState
            foreach (var error in result.Error!)
            {
                if (error.Key == "Authentication")
                {
                    var errorJson = $"{{\"closeModal\": true, \"clearValidations\": true, \"showToast\": {{ \"title\": \"{error.Value}\", \"icon\": \"error\", \"timer\": 3000 }} }}";
                    Response.Headers.Append("HX-Trigger", errorJson);
                    return PartialView("~/Views/Organizer/Partials/_UpdateEventsListPartial.cshtml", viewModel);
                }
            }

            return PartialView("_ValidationMessages", ModelState);
        }

        ModelState.Clear();

        // Define the Toastr event in the HX-Trigger header
        var toastrJson = "{\"closeModal\": true, \"clearValidations\": true, \"showToast\": { \"title\": \"Successfully created an event!\", \"icon\": \"success\", \"timer\": 3000 }}";
        Response.Headers.Append("HX-Trigger", toastrJson);
        return PartialView("~/Views/Organizer/Partials/_UpdateEventsListPartial.cshtml", viewModel);
    }

    [HttpGet("organizer/show-update-event/{id}")]
    public async Task<IActionResult> ShowUpdateEvent(Guid id)
    {
        var evt = await _eventService.GetEventByIdAsync(id);

        if (evt == null) return NotFound();
        

        if (evt.Status != EventStatus.Draft)
        {
            // Redirect to events list when event status is not Draft
            Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer"));
            return Content(string.Empty);
        }

        var viewModel = new CUEventViewModel()
        {
            Id = evt.Id,
            Name = evt.Name,
            Description = evt.Description,
            StartAtString = evt.StartAt.ToString("yyyy-MM-dd HH:mm"),
            EndAtString = evt.EndAt.ToString("yyyy-MM-dd HH:mm"),
            Status = evt.Status,
            EntryFeeString = evt.EntryFee.ToString("N2"),
        };

        return PartialView("~/Views/Organizer/Partials/_UpdateEventPartial.cshtml", viewModel);
    }

    [HttpPost("organizer/update-event")]
    public async Task<IActionResult> UpdateEvent(CUEventViewModel model)
    {
        var result = await _eventService.UpdateEventAsync(model);
        var viewModel = await GetEventsViewModel(null, null);

        if (!result.IsSuccess)
        {
            // Split error messages and add them to ModelState
            foreach (var error in result.Error!)
            {
                ModelState.AddModelError(error.Key, error.Value);
                if (error.Key == "Authentication" || error.Key == "NoChanges" || error.Key == "InvalidStatus" || error.Key == "NotFound")
                {
                    var errorJson = $"{{\"closeModal\": true, \"clearValidations\": true, \"showToast\": {{ \"title\": \"{error.Value}\", \"icon\": \"error\", \"timer\": 3000 }} }}";
                    Response.Headers.Append("HX-Trigger", errorJson);
                    return PartialView("~/Views/Organizer/Partials/_UpdateEventsListPartial.cshtml", viewModel);
                }
            }

            return PartialView("_ValidationEditMessages", ModelState);
        }

        ModelState.Clear();

        // Define the Toastr event in the HX-Trigger header
        var toastrJson = "{\"closeModal\": true, \"clearValidations\": true, \"showToast\": { \"title\": \"Successfully updated an event!\", \"icon\": \"success\", \"timer\": 3000 }}";

        Response.Headers.Append("HX-Trigger", toastrJson);
        return PartialView("~/Views/Organizer/Partials/_UpdateEventsListPartial.cshtml", viewModel);
    }

    [HttpPost("organizer/delete-event/{EventId}")]
    public async Task<IActionResult> DeleteEvent(Guid EventId)
    {
        var success = await _eventService.DeleteEventAsync(EventId);
        var viewModel = await GetEventsViewModel(null, null);

        if (!success)
        {
            var errorJson = $"{{\"closeModal\": true, \"clearValidations\": true, \"showToast\": {{ \"title\": \"Event does not exist.\", \"icon\": \"error\", \"timer\": 3000 }} }}";
            Response.Headers.Append("HX-Trigger", errorJson);
            return PartialView("~/Views/Organizer/Partials/_UpdateEventsListPartial.cshtml", viewModel);
        }

        // If the EventId to delete matches the same EventId stored in session, remove that session prop
        if (HttpContext.Session.GetString("SelectedEventId") == EventId.ToString())
            HttpContext.Session.Remove("SelectedEventId");

        // Define the Toastr event in the HX-Trigger header
        var toastrJson = "{\"closeModal\": true, \"clearValidations\": true, \"showToast\": { \"title\": \"Successfully deleted a resource!\", \"icon\": \"success\", \"timer\": 3000 }}";
        Response.Headers.Append("HX-Trigger", toastrJson);
        return PartialView("~/Views/Organizer/Partials/_EventsListPartial.cshtml", viewModel);
    }

    [HttpPatch("organizer/open-event/{EventId}")]
    public async Task<IActionResult> OpenEvent(Guid EventId)
    {
        var result = await _eventService.OpenEventByIdAsync(EventId);
        var viewModel = await GetEventsViewModel(null, null);

        if (!result.IsSuccess)
        {
            // Split error messages and add them to ModelState
            foreach (var error in result.Error!)
            {
                // Define the Toastr event in the HX-Trigger header
                var errorJson = $"{{\"closeModal\": true, \"clearValidations\": true, \"showToast\": {{ \"title\": \"{error.Value}\", \"icon\": \"error\", \"timer\": 3000 }} }}";
                Response.Headers.Append("HX-Trigger", errorJson);
            }

            return PartialView("~/Views/Organizer/Partials/_UpdateEventsListPartial.cshtml", viewModel);
        }

        ModelState.Clear();

        // Define the Toastr event in the HX-Trigger header
        var toastrJson = "{\"closeModal\": true, \"clearValidations\": true, \"showToast\": { \"title\": \"Successfully opened the event!\", \"icon\": \"success\", \"timer\": 3000 }}";
        Response.Headers.Append("HX-Trigger", toastrJson);
        return PartialView("~/Views/Organizer/Partials/_UpdateEventsListPartial.cshtml", viewModel);
    }

    [HttpGet("organizer/event-filter-date")]
    public async Task<IActionResult> FilterDate(DateTime? startDate, DateTime? endDate)
    {
        var viewModel = await GetEventsViewModel(startDate, endDate);
        return PartialView("~/Views/Organizer/Partials/_UpdateEventsListPartial.cshtml", viewModel);
    }

    [HttpPatch("organizer/draft-event/{EventId}")]
    public async Task<IActionResult> DraftEvent(Guid EventId)
    {
        var result = await _eventService.DraftEventByIdAsync(EventId);
        var viewModel = await GetEventsViewModel(null, null);

        if (!result.IsSuccess)
        {
            // Split error messages and add them to ModelState
            foreach (var error in result.Error!)
            {
                // Define the Toastr event in the HX-Trigger header
                var errorJson = $"{{\"closeModal\": true, \"clearValidations\": true, \"showToast\": {{ \"title\": \"{error.Value}\", \"icon\": \"error\", \"timer\": 3000 }} }}";
                Response.Headers.Append("HX-Trigger", errorJson);
            }

            return PartialView("~/Views/Organizer/Partials/_UpdateEventsListPartial.cshtml", viewModel);
        }

        ModelState.Clear();

        // Define the Toastr event in the HX-Trigger header
        var toastrJson = "{\"closeModal\": true, \"clearValidations\": true, \"showToast\": { \"title\": \"Successfully drafted an event!\", \"icon\": \"success\", \"timer\": 3000 }}";
        Response.Headers.Append("HX-Trigger", toastrJson);
        return PartialView("~/Views/Organizer/Partials/_UpdateEventsListPartial.cshtml", viewModel);
    }

    [HttpPatch("organizer/cancel-event/{EventId}")]
    public async Task<IActionResult> CancelEvent(Guid EventId)
    {
        var result = await _eventService.CancelEventByIdAsync(EventId);
        var viewModel = await GetEventsViewModel(null, null);

        if (!result.IsSuccess)
        {
            // Split error messages and add them to ModelState
            foreach (var error in result.Error!)
            {
                // Define the Toastr event in the HX-Trigger header
                var errorJson = $"{{\"closeModal\": true, \"clearValidations\": true, \"showToast\": {{ \"title\": \"{error.Value}\", \"icon\": \"error\", \"timer\": 3000 }} }}";
                Response.Headers.Append("HX-Trigger", errorJson);
            }

            return PartialView("~/Views/Organizer/Partials/_UpdateEventsListPartial.cshtml", viewModel);
        }

        ModelState.Clear();

        // Define the Toastr event in the HX-Trigger header
        var toastrJson = "{\"closeModal\": true, \"clearValidations\": true, \"showToast\": { \"title\": \"Successfully cancelled an event!\", \"icon\": \"success\", \"timer\": 3000 }}";
        Response.Headers.Append("HX-Trigger", toastrJson);
        return PartialView("~/Views/Organizer/Partials/_UpdateEventsListPartial.cshtml", viewModel);
    }

    [HttpGet("organizer/show-postpone-event/{EventId}")]
    public async Task<IActionResult> ShowPostponeEvent(Guid EventId)
    {
        // Changing the status to "Postponed" is techincally an "Edit/Update"
        var evt = await _eventService.GetEventByIdAsync(EventId);

        if (evt == null)
        {
            var viewModel = await GetEventsViewModel(null, null);
            var errorJson = $"{{\"closeModal\": true, \"clearValidations\": true, \"showToast\": {{ \"title\": \"Event does not exist.\", \"icon\": \"error\", \"timer\": 3000 }} }}";
            Response.Headers.Append("HX-Trigger", errorJson);
            return PartialView("~/Views/Organizer/Partials/_UpdateEventsListPartial.cshtml", viewModel);
        }

        if (evt.Status == EventStatus.Open || evt.Status == EventStatus.Ongoing)
        {
            var viewModel = new CUEventViewModel()
            {
                Id = evt.Id,
                Name = evt.Name,
                Description = evt.Description,
                StartAtString = evt.StartAt.ToString("yyyy-MM-dd HH:mm"),
                EndAtString = evt.EndAt.ToString("yyyy-MM-dd HH:mm"),
                Status = evt.Status,
                EntryFeeString = evt.EntryFee.ToString("N2"),
            };

            return PartialView("~/Views/Organizer/Partials/_PostponeEventPartial.cshtml", viewModel);
        }

        // Redirect to events list when event status is not Open or Ongoing
        Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer"));
        return Content(string.Empty);
    }

    [HttpPost("organizer/postpone-event")]
    public async Task<IActionResult> PostponeEvent(CUEventViewModel model)
    {
        var result = await _eventService.PostponeEventByIdAsync(model);
        var viewModel = await GetEventsViewModel(null, null);

        if (!result.IsSuccess)
        {

            // Split error messages and add them to ModelState
            foreach (var error in result.Error!)
            {
                if (error.Key == "Authentication" || error.Key == "NotFound" || error.Key == "InvalidStatus")
                {
                    // Define the Toastr event in the HX-Trigger header
                    var errorJson = $"{{\"closeModal\": true, \"clearValidations\": true, \"showToast\": {{ \"title\": \"{error.Value}\", \"icon\": \"error\", \"timer\": 3000 }} }}";
                    Response.Headers.Append("HX-Trigger", errorJson);
                    return PartialView("~/Views/Organizer/Partials/_UpdateEventsListPartial.cshtml", viewModel);
                }
                else if (error.Key == "NoChanges")
                {
                    ModelState.AddModelError("CustomError", error.Value);
                    break;
                }

                ModelState.AddModelError(error.Key, error.Value);
            }

            return PartialView("_ValidationEditMessages", ModelState);
        }

        ModelState.Clear();

        // Define the Toastr event in the HX-Trigger header
        var toastrJson = "{\"closeModal\": true, \"clearValidations\": true, \"showToast\": { \"title\": \"Successfully postponed an event!\", \"icon\": \"success\", \"timer\": 3000 }}";
        Response.Headers.Append("HX-Trigger", toastrJson);
        return PartialView("~/Views/Organizer/Partials/_UpdateEventsListPartial.cshtml", viewModel);
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
    public async Task<IActionResult> Resources()
    {
        var eventIdString = HttpContext.Session.GetString("SelectedEventId");

        List<Event>? events = await _eventService.GetEventsByUser();

        Event? evt = null;

        // Get the Event Id to pass to the hidden input
        if (eventIdString != null)
        {
            evt = await _eventService.GetEventByIdAsync(Guid.Parse(eventIdString));
        }
        else if (events!.Any() != false)
        {
            evt = await _eventService.GetEventByIdAsync(events!.First().Id);
        }

        // if there's no event, redirect to Events page
        if (evt == null)
        {
            // Define the Toastr event in the HX-Trigger header
            var errorJson = $"{{\"closeModal\": true, \"clearValidations\": true, \"showToast\": {{ \"title\": \"Please create an event first.\", \"icon\": \"error\", \"timer\": 3000 }} }}";
            Response.Headers.Append("HX-Trigger", errorJson);

            // Set HX-Push-Url to the current URL so the history doesn’t change.
            Response.Headers.Append("HX-Push-Url", "events");
            Response.Headers.Append("HX-Reswap", "none");

            if (Request.Headers.ContainsKey("HX-Request"))
                return Content(string.Empty);

            return RedirectToAction("Events", "Organizer");
        }

        SetEventSession(evt.Id);
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

        return PartialView("~/Views/Organizer/Partials/_UpdateResourcesListPartial.cshtml", model);
    }

    // ----------------------------------------------
    // Create Resource Modal requests
    // ----------------------------------------------

    // called when opening the CreateResourceModal
    [HttpGet("organizer/open-create-resource-modal/{EventId}")]
    public IActionResult OpenResourceModal(Guid EventId)
    {
        var model = new CUResourceViewModel
        {
            EventId = EventId
        };

        var evt = _context.Events.Include(e => e.Resources).FirstOrDefault(e => e.Id == EventId);
        bool eventHasVenue = evt?.Resources.Any(r => r.ResourceType == ResourceType.Venue) ?? false;
        var resourceTypes = Enum.GetValues(typeof(ResourceType)).Cast<ResourceType>().Where(r => r != ResourceType.Venue || !eventHasVenue)
            .ToList();

        ViewBag.ResourceTypes = new SelectList(resourceTypes, eventHasVenue ? ResourceType.Venue : ResourceType.Equipment);
        if (eventHasVenue)
        {
            model.ResourceType = ResourceType.Equipment;
        }

        return PartialView("~/Views/Organizer/Partials/CreateResourceModal.cshtml", model);
    }

    // used in CreateResourceModal
    [HttpGet("organizer/select-resource-type")]
    public IActionResult ResourceExtra(ResourceType selectedOption)
    {
        CUResourceViewModel viewModel = selectedOption switch
        {
            ResourceType.Venue => new CUResourceViewModel { ResourceType = ResourceType.Venue },
            ResourceType.Equipment => new CUResourceViewModel { ResourceType = ResourceType.Equipment },
            ResourceType.Furniture => new CUResourceViewModel { ResourceType = ResourceType.Furniture },
            ResourceType.Catering => new CUResourceViewModel { ResourceType = ResourceType.Catering },
            ResourceType.Personnel => new CUResourceViewModel { ResourceType = ResourceType.Personnel },
            _ => new CUResourceViewModel { ResourceType = ResourceType.Venue } // Default
        };
        return PartialView("_SelectResourceTypePartial", viewModel);
    }

    // if Furniture is the resource type
    [HttpGet("organizer/select-furniture-material")]
    public IActionResult SelectFurnitureMaterial(FurnitureMaterial selectedOption)
    {
        CUResourceViewModel viewModel = selectedOption switch
        {
            FurnitureMaterial.Wood => new CUResourceViewModel { Material = FurnitureMaterial.Wood },
            FurnitureMaterial.Metal => new CUResourceViewModel { Material = FurnitureMaterial.Metal },
            FurnitureMaterial.Plastic => new CUResourceViewModel { Material = FurnitureMaterial.Plastic },
            FurnitureMaterial.Glass => new CUResourceViewModel { Material = FurnitureMaterial.Glass },
            FurnitureMaterial.Fabric => new CUResourceViewModel { Material = FurnitureMaterial.Fabric },
            FurnitureMaterial.Other => new CUResourceViewModel { Material = FurnitureMaterial.Other },
            _ => new CUResourceViewModel { Material = FurnitureMaterial.Wood } // Default
        };

        if (selectedOption == FurnitureMaterial.Other)
            viewModel.IsOtherMaterial = true;
        else
            viewModel.IsOtherMaterial = false;

        viewModel.ResourceType = ResourceType.Furniture;
        return PartialView("_SelectResourceTypePartial", viewModel);
    }

    [HttpGet("organizer/cost-types")]
    public IActionResult GetCostTypes(string selectedOption, string savedCostType)
    {
        if (Enum.TryParse<ResourceType>(selectedOption, out var resourceType))
        {
            ViewBag.SavedCostType = savedCostType;
            return PartialView("_CostTypeOptionsPartial", resourceType);
        }

        return BadRequest();
    }

    [HttpPost("organizer/create-resource")]
    public async Task<IActionResult> CreateResource(Guid EventId, CUResourceViewModel model)
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
        List<Event>? events = await _eventService.GetEventsByUser();

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

        // Define the Toastr event in the HX-Trigger header
        var toastrJson = "{\"closeModal\": true, \"clearValidations\": true, \"showToast\": { \"title\": \"Successfully added a resource!\", \"icon\": \"success\", \"timer\": 3000 }}";
        Response.Headers.Append("HX-Trigger", toastrJson);
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

        var viewModel = await _resourceService.GetViewResourceViewModel(resourceWithType.Id);

        return PartialView("~/Views/Organizer/Partials/_ViewResourcePartial.cshtml", viewModel);
    }

    [HttpPost("organizer/delete-resource/{ResourceId}")]
    public async Task<IActionResult> DeleteResource(Guid ResourceId)
    {
        Console.WriteLine("Hello?");
        var resource = await _resourceService.GetResourceByIdAsync(ResourceId);
        if (resource == null) return NotFound();

        Console.WriteLine("Is Anyone There?");
        Event? evt = await _eventService.GetEventByIdAsync(resource.EventId);
        List<Event>? events = await _eventService.GetEventsByUser();

        Console.WriteLine("Oh-oh,");
        var success = await _resourceService.DeleteResourceAsync(ResourceId);
        if (!success) return NotFound(); // Return 404 if resource doesn't exist

        Console.WriteLine("Hi!?");
        // if there's no event, redirect to Events page
        if (evt == null)
        {
            Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer"));
            return RedirectToAction("Events", "Organizer");
        }

        SetEventSession(evt.Id);
        var resources = _resourceService.GetResourcesByEventId(evt.Id);

        var viewModel = new ResourceViewModel
        {
            EventId = evt.Id,
            Events = events!,
            SelectedName = evt.Name,
            Resources = resources!,
            ResourceImages = _resourceService.GetResourceImageFromList(resources!),
        };

        // Define the Toastr event in the HX-Trigger header
        var toastrJson = "{\"closeModal\": true, \"clearValidations\": true, \"showToast\": { \"title\": \"Successfully deleted a resource!\", \"icon\": \"success\", \"timer\": 3000 }}";
        Response.Headers.Append("HX-Trigger", toastrJson);
        return PartialView("~/Views/Organizer/Partials/_UpdateResourcesListPartial.cshtml", viewModel);
    }

    [HttpGet("organizer/show-update-resource/{ResourceId}")]
    public async Task<IActionResult> ShowUpdateResource(Guid ResourceId)
    {
        var viewModel = await _resourceService.GetCUResourceViewModel(ResourceId);

        if(viewModel == null)
        {
            Console.WriteLine("viewModel");
            return NotFound();
        }

        var evt = _context.Events
            .Include(e => e.Resources)
            .FirstOrDefault(e => e.Id == viewModel.EventId);

        bool hasVenue = evt?.Resources.Any(r => r.ResourceType == ResourceType.Venue) ?? false;

        // Exclude Venue only if the event has a Venue and the viewed resource is NOT a Venue
        var resourceTypes = Enum.GetValues(typeof(ResourceType))
            .Cast<ResourceType>()
            .Where(r => !(hasVenue && r == ResourceType.Venue && viewModel.ResourceType != ResourceType.Venue))
            .ToList();

        ViewBag.ResourceTypes = new SelectList(resourceTypes);
        return PartialView("~/Views/Organizer/Partials/_UpdateResourcePartial.cshtml", viewModel);
    }

    [HttpPost("organizer/update-resource")]
    public async Task<IActionResult> UpdateResource(CUResourceViewModel model)
    {
        var result = await _resourceService.UpdateResourceAsync(model);

        if (!result.IsSuccess)
        {
            // Split error messages and add them to ModelState
            foreach (var error in result.Error!)
            {
                ModelState.AddModelError(error.Key, error.Value);
                var errorJson = $"{{\"showToast\": {{ \"title\": \"{error.Value}\", \"icon\": \"error\", \"timer\": 3000 }} }}";
                Response.Headers.Append("HX-Trigger", errorJson);
            }

            return PartialView("_ValidationEditMessages", ModelState);
        }

        // Once everything is clear with no errors, it's time to hx-swap
        ModelState.Clear();
        List<Event>? events = await _eventService.GetEventsByUser();

        Event? evt = _eventService.GetEventById(model.EventId);

        // if there's no event, redirect to Events page
        if (evt == null)
        {
            Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer"));
            return RedirectToAction("Events", "Organizer");
        }

        var resources = _resourceService.GetResourcesByEventId(model.EventId);
        var resource = await _resourceService.GetResourceByIdAsync(model.Id);
        if (resource == null)
        {
            var errorJson = $"{{\"showToast\": {{ \"title\": \"Resource does not exist.\", \"icon\": \"error\", \"timer\": 3000 }} }}";
            Response.Headers.Append("HX-Trigger", errorJson);
            return NotFound();
        }
        var viewResourceViewModel = await _resourceService.GetViewResourceViewModel(resource.Id);
        if (viewResourceViewModel == null)
        {
            var errorJson = $"{{\"showToast\": {{ \"title\": \"Resource does not exist.\", \"icon\": \"error\", \"timer\": 3000 }} }}";
            Response.Headers.Append("HX-Trigger", errorJson);
            return NotFound();
        }

        var viewModel = new ResourceViewModel
        {
            EventId = model.EventId,
            Events = events!,
            SelectedName = evt.Name,
            Resources = resources!,
            ResourceImages = _resourceService.GetResourceImageFromList(resources!),
            ViewResourceViewModel = viewResourceViewModel,
        };

        // Define the Toastr event in the HX-Trigger header
        var toastrJson = "{\"clearValidations\": true, \"showToast\": { \"title\": \"Successfully updated a resource!\", \"icon\": \"success\", \"timer\": 3000 }}";
        Response.Headers.Append("HX-Trigger", toastrJson);
        return PartialView("~/Views/Organizer/Partials/_AfterResourceUpdate.cshtml", viewModel);
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

    private async Task<EventsViewModel> GetEventsViewModel(DateTime? startDate, DateTime? endDate)
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

        var hasVenues = await _eventService.GetEventHasVenueByUser();
        var isOpenable = await _eventService.GetEventIsOpenableByUser();

        var viewModel = new EventsViewModel
        {
            DraftEvents = DraftEvents,
            PublishedEvents = PublishedEvents,
            ConcludedEvents = ConcludedEvents,
            EventAttendeeCounts = MergedAttendeeCounts,
            HasVenues = hasVenues,
            IsOpenable = isOpenable,
        };

        return viewModel;
    }

    // private ViewResourceViewModel? GetViewResourceViewModel(Resource? resource)
    // {
    //     if (resource == null) return null;
    //     Console.WriteLine(resource.Image!.ImageFileName);
    //     Console.WriteLine(resource.Image!.ImageFileName);
    //     Console.WriteLine(resource.Image!.ImageFileName);
    //     Console.WriteLine(resource.Image!.ImageFileName);
    //     Console.WriteLine(resource.Image!.ImageFileName);
    //     var viewModel = new ViewResourceViewModel
    //     {
    //         Id = resource.Id,
    //         EventId = resource.EventId,
    //         ImageFileName = resource.Image!.ImageFileName,
    //         ProviderName = resource.ProviderName,
    //         ProviderEmail = resource.ProviderEmail,
    //         ProviderPhoneNumber = resource.ProviderPhoneNumber,
    //         Name = resource.Name,
    //         Description = resource.Description,
    //         ResourceType = resource.ResourceType,
    //         Cost = resource.Cost,
    //         CostType = resource.CostType,
    //         CreatedAt = resource.CreatedAt,
    //         UpdatedAt = resource.UpdatedAt,
    //     };

    //     if (resource.ResourceType == ResourceType.Venue)
    //     {
    //         viewModel.Capacity = resource.ResourceVenue.Capacity;
    //         viewModel.Size = resource.ResourceVenue.Size;
    //         viewModel.AddressLine1 = resource.ResourceVenue.AddressLine1;
    //         viewModel.AddressLine2 = resource.ResourceVenue.AddressLine2;
    //         viewModel.CityMunicipality = resource.ResourceVenue.CityMunicipality;
    //         viewModel.Province = resource.ResourceVenue.Province;
    //     }
    //     else if (resource.ResourceType == ResourceType.Equipment)
    //     {
    //         viewModel.Quantity = resource.ResourceEquipment.Quantity;
    //         viewModel.Brand = resource.ResourceEquipment.Brand;
    //         viewModel.Warranty = resource.ResourceEquipment.Warranty;
    //         viewModel.Specifications = JsonSerializer.Deserialize<Dictionary<string, string>>(resource.ResourceEquipment.Specifications!)!;
    //     }
    //     else if (resource.ResourceType == ResourceType.Furniture)
    //     {
    //         viewModel.Quantity = resource.ResourceFurniture.Quantity;
    //         viewModel.Material = resource.ResourceFurniture.Material;
    //         viewModel.OtherMaterial = resource.ResourceFurniture.OtherMaterial;
    //         viewModel.Dimensions = resource.ResourceFurniture.Dimensions;
    //         viewModel.Warranty = resource.ResourceFurniture.Warranty;
    //     }
    //     else if (resource.ResourceType == ResourceType.Catering)
    //     {
    //         viewModel.GuestCapacity = resource.ResourceCatering.GuestCapacity;
    //         viewModel.MenuItems = resource.ResourceCatering.MenuItems.Split(",").ToList();
    //     }
    //     else if (resource.ResourceType == ResourceType.Personnel)
    //     {
    //         viewModel.Position = resource.ResourcePersonnel.Position;
    //         viewModel.ShiftStart = resource.ResourcePersonnel.ShiftStart;
    //         viewModel.ShiftEnd = resource.ResourcePersonnel.ShiftEnd;
    //         viewModel.Experience = resource.ResourcePersonnel.Experience;
    //     }

    //     return viewModel;
    // }
}
