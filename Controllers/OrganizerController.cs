using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedify.Data;
using Schedify.Services;
using Schedify.ViewModels;

namespace Schedify.Controllers;

[Authorize(Roles = "Organizer")]
public class OrganizerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly EventService _eventService;
    public OrganizerController(ApplicationDbContext context, EventService eventService)
    {
        _eventService = eventService;
        _context = context;
    }

    [Route("organizer/events")]
    [HttpGet]
    public IActionResult Events(DateTime? startDate, DateTime? endDate)
    {
        // Set default values if not provided
        startDate ??= new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc); // Jan 1st, 00:00 UTC
        endDate ??= DateTime.UtcNow.Date.AddHours(23).AddMinutes(59); // Today at 23:59 UTC

        var viewModel = new OrganizerEventsViewModel
        {
            DraftEvents = _eventService.GetEventsByOrganizerDraft(startDate, endDate),
            PublishedEvents = _eventService.GetEventsByOrganizerPublished(startDate, endDate),
            ConcludedEvents = _eventService.GetEventsByOrganizerConcluded(startDate, endDate),
        };

        if (Request.Headers["HX-Request"].Any())
        {
            return PartialView("~/Views/Organizer/Partials/_EventsListPartial.cshtml", viewModel);
        }

        return View(viewModel);
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

    [Route("organizer/view-event/{id}")]
    [HttpGet]
    public async Task<IActionResult> ViewEvent(Guid id)
    {
        var _event = await _eventService.GetEventByIdAsync(id);

        if (_event == null)
        {
            return NotFound();
        }

        return PartialView("~/Views/Organizer/Partials/_ViewEventPartial.cshtml", _event);
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
    public async Task<IActionResult> UpdateEvent(Guid Id, UpdateEventViewModel model)
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

    [Route("organizer/resources")]
    [HttpGet]
    public ActionResult Resources() => View();

    [Route("organizer/feedbacks")]
    [HttpGet]
    public ActionResult Feedbacks() => View();

    [Route("organizer/stats")]
    [HttpGet]
    public ActionResult Stats() => View();


    [Route("organizer/venues")]
    public IActionResult Venues()
    {
        if (Request.Headers["HX-Request"] == "true")
        {
            return PartialView("~/Views/Organizer/Partials/_VenuesPartial.cshtml", new { });
        }

        return RedirectToAction("Resources", "Organizer");
    }
    [Route("organizer/equipments")]
    public IActionResult Equipments()
    {
        if (Request.Headers["HX-Request"] == "true")
        {
            return PartialView("~/Views/Organizer/Partials/_EquipmentsPartial.cshtml", new { });
        }

        return RedirectToAction("Resources", "Organizer");

    }
    [Route("organizer/furnitures")]
    public IActionResult Furnitures()
    {

        if (Request.Headers["HX-Request"] == "true")
        {
            return PartialView("~/Views/Organizer/Partials/_FurnituresPartial.cshtml", new { });
        }

        return RedirectToAction("Resources", "Organizer");

    }
    [Route("organizer/personnels")]
    public IActionResult Personnels()
    {

        if (Request.Headers["HX-Request"] == "true")
        {
            return PartialView("~/Views/Organizer/Partials/_PersonnelsPartial.cshtml", new { });
        }

        return RedirectToAction("Resources", "Organizer");

    }
    [Route("organizer/caterings")]
    public IActionResult Caterings()
    {

        if (Request.Headers["HX-Request"] == "true")
        {
            return PartialView("~/Views/Organizer/Partials/_CateringsPartial.cshtml", new { });
        }

        return RedirectToAction("Resources", "Organizer");

    }

    [Route("organizer/by-events")]
    public IActionResult ByEvents()
    {

        if (Request.Headers["HX-Request"] == "true")
        {
            return PartialView("~/Views/Organizer/Partials/_CateringsPartial.cshtml", new { });
        }

        return RedirectToAction("Resources", "Organizer");

    }
}
