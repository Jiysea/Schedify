

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
    private readonly BookingService _bookingService;
    private readonly StripeService _stripeService;
    public AttendeeController(ApplicationDbContext context, EventService eventService, ResourceService resourceService, BookingService bookingService, StripeService stripeService)
    {
        _context = context;
        _eventService = eventService;
        _resourceService = resourceService;
        _bookingService = bookingService;
        _stripeService = stripeService;
    }

    // ---------------------------------------------------------------------------------------------------------------------------
    // # Bookings
    // ---------------------------------------------------------------------------------------------------------------------------

    [HttpGet("attendee/bookings")]
    public IActionResult Bookings()
    {
        var viewModel = new BookingViewModel
        {
            // PublishedEvents = [],
            // EventsCount = events.Count,
        };

        return View(viewModel);
    }

    // ---------------------------------------------------------------------------------------------------------------------------
    // # Events
    // ---------------------------------------------------------------------------------------------------------------------------

    [HttpGet("attendee/events")]
    public IActionResult Events()
    {
        var openedEvents = _eventService.GetOpenedEvents(null, null);
        var bookedEvents = _bookingService.GetBookingsByUser();
        var eventVenueImages = _eventService.GetEventVenueImages(openedEvents);
        var organizerImages = _eventService.GetOrganizerImages(openedEvents);
        var attendeeCounts = _eventService.GetAttendeeCounts(openedEvents);

        var viewModel = new EventsViewModel
        {
            OpenedEvents = openedEvents,
            BookedEvents = bookedEvents,
            EventImages = eventVenueImages,
            OrganizerImages = organizerImages,
            EventAttendeeCounts = attendeeCounts,
        };

        return View(viewModel);
    }

    [HttpGet("attendee/checkout/{EventId}")]
    public async Task<IActionResult> Checkout(Guid EventId)
    {
        // 1. Book the event internally (or just prepare data, depends on logic)
        var booking = await _bookingService.PrepareBookingDetailsAsync(EventId);
        if (booking == null) return NotFound();

        // 2. Create Stripe Checkout Session
        var sessionUrl = await _stripeService.CreateCheckoutSessionAsync(booking, EventId);
        if (sessionUrl == null) return NotFound();

        Response.Headers.Append("HX-Redirect", sessionUrl);
        return Content(string.Empty);
    }

    // [HttpPost("attendee/book-event/{EventId}")]
    // public async Task<IActionResult> BookEvent(Guid EventId)
    // {

    //     var openedEvents = _eventService.GetOpenedEvents(null, null);
    //     var eventVenueImages = _eventService.GetEventVenueImages(openedEvents);
    //     var organizerImages = _eventService.GetOrganizerImages(openedEvents);
    //     var attendeeCounts = _eventService.GetAttendeeCounts(openedEvents);

    //     var viewModel = new EventsViewModel
    //     {
    //         OpenedEvents = openedEvents,
    //         EventImages = eventVenueImages,
    //         OrganizerImages = organizerImages,
    //         EventAttendeeCounts = attendeeCounts,
    //     };

    //     return PartialView("~/Views/Attendee/Partials/_UpdateEventsListPartial.cshtml", viewModel);
    // }

    // ---------------------------------------------------------------------------------------------------------------------------
    // # View Event Modal
    // ---------------------------------------------------------------------------------------------------------------------------

    [HttpGet("attendee/view-event/{EventId}")]
    public async Task<IActionResult> ViewEvent(Guid EventId)
    {
        var evt = await _eventService.GetEventByIdAsync(EventId);

        if (evt == null) return NotFound();

        var book = await _bookingService.GetBookingByIdAsync(evt.Id);
        bool IsEventBooked = false;
        if (book != null)
            IsEventBooked = true;

        var viewModel = new ViewEventViewModel
        {
            Id = evt.Id,
            Name = evt.Name,
            Description = evt.Description,
            StartAt = evt.StartAt,
            EndAt = evt.EndAt,
            Status = evt.Status,
            EntryFee = evt.EntryFee.ToString("N2"),
            CreatedAt = evt.CreatedAt,
            UpdatedAt = evt.UpdatedAt,
            IsEventBooked = IsEventBooked
        };

        return PartialView("~/Views/Attendee/Partials/_ViewEventPartial.cshtml", viewModel);
    }
}