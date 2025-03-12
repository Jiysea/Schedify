

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedify.Data;
using Schedify.Services;
using Schedify.ViewModels;

namespace Schedify.Controllers;

[Authorize(Roles = "Attendee")]
public class AttendeeController : Controller
{

    private readonly ApplicationDbContext _context;
    private readonly EventService _eventService;
    private readonly ResourceService _resourceService;
    public AttendeeController(ApplicationDbContext context, EventService eventService, ResourceService resourceService)
    {
        _context = context;
        _eventService = eventService;
        _resourceService = resourceService;
    }
    
    [Route("attendee/events")]
    public ActionResult Events()
    {
        var events = _eventService.GetEventsPublished();

        var model = new AttendeeEventsViewModel
        {
            PublishedEvents = events,
            EventsCount = events.Count,
        };
        
        return View(model);
    }
}