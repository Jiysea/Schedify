using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Schedify.Data;
using Schedify.Models;
using Schedify.Services;
using Schedify.ViewModels;

namespace Schedify.Controllers;

[Authorize(Roles = "Organizer")]
public class OrganizerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly EventService _eventService;
    private readonly ResourceService _resourceService;
    public OrganizerController(ApplicationDbContext context, EventService eventService, ResourceService resourceService)
    {
        _context = context;
        _eventService = eventService;
        _resourceService = resourceService;
    }

    [Route("organizer/events")]
    [HttpGet]
    public IActionResult Events(DateTime? startDate, DateTime? endDate)
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

        if (Request.Headers["HX-Request"].Any())
        {
            return PartialView("~/Views/Organizer/Partials/_EventsListPartial.cshtml", viewModel);
        }

        return View(viewModel);
    }

    [Route("organizer/view-event/{id}")]
    [HttpGet]
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

    [Route("organizer/create-event")]
    [HttpPost]
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

        Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer"));
        return Content(string.Empty);

    }

    [Route("organizer/show-update-event/{id}")]
    [HttpGet]
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

    [Route("organizer/update-event")]
    [HttpPost]
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

        Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer"));
        return Content(string.Empty);
    }

    [Route("organizer/delete-event/{id}")]
    [HttpDelete]
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

    [Route("organizer/open-event/{id}")]
    [HttpPatch]
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

    [Route("organizer/draft-event/{id}")]
    [HttpPatch]
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

    [Route("organizer/cancel-event/{id}")]
    [HttpPatch]
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

    [Route("organizer/show-postpone-event/{id}")]
    [HttpGet]
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

    [Route("organizer/postpone-event")]
    [HttpPost]
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

    [Route("organizer/resources")]
    [HttpGet]
    public IActionResult Resources()
    {
        var eventIdString = HttpContext.Session.GetString("SelectedEventId");
        Console.WriteLine(eventIdString);
        // int pageSize = 8; // Default page size
        // int newPage = 0; // Calculate new page number

        // if (newPage < 1) newPage = 1; // Prevent negative pages
        // var totalCount = _context.Resources.Count(r => r.ResourceType == ResourceType.Venue);

        List<Event>? events = _eventService.GetEvents();
        Event? evt = null;

        if (eventIdString != null)
        {
            evt = _eventService.GetEventById(Guid.Parse(eventIdString));
        }
        else if (events != null || events!.Any() != false)
        {
            evt = _eventService.GetEventById(events!.First().Id);
        }

        if (evt == null)
        {
            Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer"));
            return RedirectToAction("Events", "Organizer");
        }

        Console.WriteLine(evt);
        Console.WriteLine(evt);
        Console.WriteLine(evt);

        var resources = _resourceService.GetResourcesByEventId(evt.Id);

        var model = new ResourceViewModel
        {
            EventId = evt.Id,
            Events = events!,
            SelectedName = evt.Name,
            Resources = resources!,
            ResourceImages = _resourceService.GetResourceImageFromList(resources!),
            // CurrentPage = newPage,
            // PageSize = pageSize,
            // TotalCount = totalCount
        };

        return View(model);
    }

    [HttpGet("organizer/select-event-dropdown/{EventId}")]
    public async Task<IActionResult> GetEventsDropdown(Guid EventId)
    {
        var events = await _eventService.GetEventsForDropdown();
        var evt = _eventService.GetEventById(EventId);

        var model = new ResourceViewModel
        {

            Events = events,
            SelectedName = events.Any() ? events.First().Name : null
        };

        return PartialView("~/Views/Organizer/Partials/_EventsDropdown.cshtml", model);
    }

    // ----------------------------------------------
    // Create Resource Modal requests
    // ----------------------------------------------

    // Used in CreateResourceModal
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

    [HttpGet("organizer/cost-types")]
    public IActionResult GetCostTypes(string selectedOption)
    {
        if (Enum.TryParse<ResourceType>(selectedOption, out var resourceType))
        {
            return PartialView("_CostTypeOptionsPartial", resourceType);
        }

        return BadRequest();
    }

    [HttpGet("organizer/open-create-resource-modal/{EventId}")]
    public IActionResult OpenResourceModal(Guid EventId)
    {
        // if (Enum.TryParse<ResourceType>(selectedOption, out var resourceType))
        // {
        //     return PartialView("_CostTypeOptionsPartial", resourceType);
        // }

        return BadRequest();
    }

    // ----------------------------------------------
    // Create Resource Modal requests
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

    // [Route("organizer/view-resource/{id}")]
    // [HttpGet]
    // public async Task<IActionResult> ViewResource(Guid id)
    // {
    //     var resource = await _resourceService.GetResourceByIdForOrganizersAsync(id);

    //     if (resource == null)
    //     {
    //         return NotFound();
    //     }

    //     return PartialView("~/Views/Organizer/Partials/_ViewResourcePartial.cshtml", resource);
    // }

    // [Route("organizer/show-add-event-resource/{id}")]
    // [HttpGet]
    // public async Task<IActionResult> ShowAddToEventResource(Guid id)
    // {
    //     var DraftEvents = _eventService.GetEventsByOrganizerDraft(new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc), DateTime.UtcNow.Date.AddHours(23).AddMinutes(59));
    //     var result = await _resourceService.GetResourceAndEvents(id, DraftEvents);

    //     if (result == null)
    //     {
    //         return NotFound();
    //     }

    //     return PartialView("~/Views/Organizer/Partials/_AddToEventResourcePartial.cshtml", result);
    // }

    // [Route("organizer/add-event-resource")]
    // [HttpPost]
    // public async Task<IActionResult> AddToEventResource(EventResourceViewModel model)
    // {
    //     Console.WriteLine(model.CostType);
    //     Console.WriteLine(model.ResourceType);
    //     Console.WriteLine(model.QuantityFromForm);
    //     Console.WriteLine(model.QuantityFromResource);
    //     Console.WriteLine(model.MaxQuantityReached);
    //     Console.WriteLine(model.TotalCost);
    //     Console.WriteLine(model.CostFromResource);
    //     if (!ModelState.IsValid)
    //     {
    //         foreach (var state in ModelState)
    //         {
    //             foreach (var error in state.Value.Errors)
    //             {
    //                 Console.WriteLine(error.ErrorMessage);
    //             }
    //         }
    //         return PartialView("_ValidationMessages", ModelState);
    //     }

    //     var result = await _eventService.AddToEventResourceAsync(model);

    //     if (!result.IsSuccess)
    //     {
    //         // Split error messages and add them to ModelState
    //         foreach (var error in result.Error!)
    //         {
    //             ModelState.AddModelError(error.Key, error.Value);
    //             if (error.Key == "Authentication")
    //             {
    //                 Response.Headers.Append("HX-Redirect", Url.Action("Login", "Auth"));
    //             }
    //         }

    //         return PartialView("_ValidationMessages", ModelState);
    //     }

    //     Response.Headers.Append("HX-Redirect", Url.Action("Resources", "Organizer"));
    //     return Content(string.Empty);
    // }

    // [Route("organizer/update-total-cost")]
    // [HttpPost]
    // public async Task<IActionResult> UpdateTotalCost(EventResourceViewModel model)
    // {
    //     var DraftEvents = _eventService.GetEventsByOrganizerDraft(new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc), DateTime.UtcNow.Date.AddHours(23).AddMinutes(59));
    //     var resource = await _context.Resources
    //         .Include(r => r.Image)
    //         .FirstOrDefaultAsync(r => r.Id == model.ResourceId);

    //     if (resource == null)
    //     {
    //         return NotFound();
    //     }

    //     var updatedModel = new EventResourceViewModel
    //     {
    //         ResourceId = model.ResourceId,
    //         Resource = resource,
    //         DraftEvents = DraftEvents,
    //         EventStartAt = DraftEvents.First().StartAt,
    //         EventEndAt = DraftEvents.First().EndAt,
    //         CostType = resource.CostType,
    //         ResourceType = resource.ResourceType,
    //         // QuantityFromResource = resource.Quantity,
    //         CostFromResource = resource.Cost,
    //         // Shift = resource.ResourceType == ResourceType.Personnel ? resource.Shift : null,
    //         QuantityFromForm = model.QuantityFromForm,
    //     };

    //     return PartialView("~/Views/Organizer/Partials/_TotalCostUpdatePartial.cshtml", updatedModel.TotalCost);
    // }

    // [Route("organizer/update-selected-event")]
    // [HttpPost]
    // public async Task<IActionResult> UpdateSelectedEvent(Guid EventId, EventResourceViewModel model)
    // {
    //     var DraftEvents = _eventService.GetEventsByOrganizerDraft(new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc), DateTime.UtcNow.Date.AddHours(23).AddMinutes(59));
    //     var selectedEvent = await _context.Events.FindAsync(EventId);

    //     if (selectedEvent == null) return NotFound();

    //     var resource = await _context.Resources
    //         .Include(r => r.Image)
    //         .FirstOrDefaultAsync(r => r.Id == model.ResourceId);

    //     if (resource == null)
    //     {
    //         return NotFound();
    //     }

    //     var updatedModel = new EventResourceViewModel
    //     {
    //         EventId = EventId,
    //         ResourceId = model.ResourceId,
    //         Resource = resource,
    //         DraftEvents = DraftEvents,
    //         EventStartAt = DraftEvents.First().StartAt,
    //         EventEndAt = DraftEvents.First().EndAt,
    //         SelectedEvent = selectedEvent.Name,
    //         CostType = resource.CostType,
    //         ResourceType = resource.ResourceType,
    //         // QuantityFromResource = resource.Quantity,
    //         CostFromResource = resource.Cost,
    //         // Shift = resource.ResourceType == ResourceType.Personnel ? resource.Shift : null,
    //         QuantityFromForm = model.QuantityFromForm,
    //     };

    //     return PartialView("~/Views/Organizer/Partials/_AddToEventResourcePartial.cshtml", updatedModel);
    // }

    // [Route("organizer/by-events")]
    // public IActionResult ByEvents()
    // {

    //     if (Request.Headers["HX-Request"] == "true")
    //     {
    //         return PartialView("~/Views/Organizer/Partials/_CateringsPartial.cshtml", new { });
    //     }

    //     return RedirectToAction("Resources", "Organizer");

    // }

    // [Route("organizer/venues")]
    // public IActionResult Venues()
    // {
    //     if (Request.Headers["HX-Request"] == "true")
    //     {
    //         int pageSize = 8; // Default page size
    //         int newPage = 0; // Calculate new page number

    //         if (newPage < 1) newPage = 1; // Prevent negative pages
    //         var totalCount = _context.Resources.Count(r => r.ResourceType == ResourceType.Venue);
    //         var resources = _resourceService.GetResourcesByType(ResourceType.Venue, newPage, pageSize);

    //         var model = new ResourceViewModel
    //         {
    //             Resources = resources!,
    //             ResourceImages = _resourceService.GetResourceImageFromList(resources!),
    //             CurrentPage = newPage,
    //             PageSize = pageSize,
    //             TotalCount = totalCount
    //         };
    //         return PartialView("~/Views/Organizer/Partials/_TypeVenuesPartial.cshtml", model);
    //     }

    //     return RedirectToAction("Resources", "Organizer");
    // }

    // [Route("organizer/equipments")]
    // public IActionResult Equipments()
    // {
    //     if (Request.Headers["HX-Request"] == "true")
    //     {
    //         int pageSize = 8; // Default page size
    //         int newPage = 0; // Calculate new page number

    //         if (newPage < 1) newPage = 1; // Prevent negative pages
    //         var totalCount = _context.Resources.Count(r => r.ResourceType == ResourceType.Equipment);
    //         var resources = _resourceService.GetResourcesByType(ResourceType.Equipment, newPage, pageSize);

    //         var model = new ResourceViewModel
    //         {
    //             Resources = resources!,
    //             ResourceImages = _resourceService.GetResourceImageFromList(resources!),
    //             CurrentPage = newPage,
    //             PageSize = pageSize,
    //             TotalCount = totalCount
    //         };
    //         return PartialView("~/Views/Organizer/Partials/_TypeEquipmentsPartial.cshtml", model);
    //     }

    //     return RedirectToAction("Resources", "Organizer");

    // }

    // [Route("organizer/furnitures")]
    // public IActionResult Furnitures()
    // {

    //     if (Request.Headers["HX-Request"] == "true")
    //     {
    //         int pageSize = 8; // Default page size
    //         int newPage = 0; // Calculate new page number

    //         if (newPage < 1) newPage = 1; // Prevent negative pages
    //         var totalCount = _context.Resources.Count(r => r.ResourceType == ResourceType.Furniture);
    //         var resources = _resourceService.GetResourcesByType(ResourceType.Furniture, newPage, pageSize);

    //         var model = new ResourceViewModel
    //         {
    //             Resources = resources!,
    //             ResourceImages = _resourceService.GetResourceImageFromList(resources!),
    //             CurrentPage = newPage,
    //             PageSize = pageSize,
    //             TotalCount = totalCount
    //         };
    //         return PartialView("~/Views/Organizer/Partials/_TypeFurnituresPartial.cshtml", model);
    //     }

    //     return RedirectToAction("Resources", "Organizer");

    // }

    // [Route("organizer/caterings")]
    // public IActionResult Caterings()
    // {

    //     if (Request.Headers["HX-Request"] == "true")
    //     {
    //         int pageSize = 8; // Default page size
    //         int newPage = 0; // Calculate new page number

    //         if (newPage < 1) newPage = 1; // Prevent negative pages
    //         var totalCount = _context.Resources.Count(r => r.ResourceType == ResourceType.Catering);
    //         var resources = _resourceService.GetResourcesByType(ResourceType.Catering, newPage, pageSize);

    //         var model = new ResourceViewModel
    //         {
    //             Resources = resources!,
    //             ResourceImages = _resourceService.GetResourceImageFromList(resources!),
    //             CurrentPage = newPage,
    //             PageSize = pageSize,
    //             TotalCount = totalCount
    //         };
    //         return PartialView("~/Views/Organizer/Partials/_TypeCateringsPartial.cshtml", model);
    //     }

    //     return RedirectToAction("Resources", "Organizer");

    // }

    // [Route("organizer/personnels")]
    // public IActionResult Personnels()
    // {

    //     if (Request.Headers["HX-Request"] == "true")
    //     {
    //         int pageSize = 8; // Default page size
    //         int newPage = 0; // Calculate new page number

    //         if (newPage < 1) newPage = 1; // Prevent negative pages
    //         var totalCount = _context.Resources.Count(r => r.ResourceType == ResourceType.Personnel);
    //         var resources = _resourceService.GetResourcesByType(ResourceType.Personnel, newPage, pageSize);

    //         var model = new ResourceViewModel
    //         {
    //             Resources = resources!,
    //             ResourceImages = _resourceService.GetResourceImageFromList(resources!),
    //             CurrentPage = newPage,
    //             PageSize = pageSize,
    //             TotalCount = totalCount
    //         };
    //         return PartialView("~/Views/Organizer/Partials/_TypePersonnelsPartial.cshtml", model);
    //     }

    //     return RedirectToAction("Resources", "Organizer");

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
}
