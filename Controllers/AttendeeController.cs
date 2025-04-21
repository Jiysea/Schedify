

using System.Text;
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
        var bookedEvents = _bookingService.GetBookingsByUser();
        var events = _bookingService.GetEventsByBooking(bookedEvents);
        var resources = _bookingService.GetResourceByEvents(events);
        var bookedImages = _bookingService.GetEventVenueImages(bookedEvents);

        var viewModel = new BookingsViewModel
        {
            BookedEvents = bookedEvents,
            Events = events,
            EventResources = resources,
            BookingImages = bookedImages,
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

    [HttpGet("attendee/view-ticket/{EventBookingId}")]
    public IActionResult ViewTicket(Guid EventBookingId)
    {
        var plainBytes = Encoding.UTF8.GetBytes(EventBookingId.ToString());
        var base64Token = Convert.ToBase64String(plainBytes);

        Response.Headers.Append("HX-Redirect", Url.Action("ViewTicket", "Payment", new { token = base64Token }));
        return Content(string.Empty);
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

    // ---------------------------------------------------------------------------------------------------------------------------
    // # View Event Modal
    // ---------------------------------------------------------------------------------------------------------------------------

    [HttpGet("attendee/view-event/{EventId}")]
    public async Task<IActionResult> ViewEvent(Guid EventId)
    {
        var evt = await _bookingService.GetEventByIdAsync(EventId);

        if (evt == null) return NotFound();

        var venue = evt.Resources.FirstOrDefault()!.ResourceVenue;

        string? fullAddress = venue.AddressLine1 + ", " + (venue.AddressLine2
            != null
            ? venue.AddressLine2 + ", " : "") + venue.CityMunicipality + ", " +
            venue.Province;
        
        string? ImageFileName = await _bookingService.GetImageByResourceIdAsync(venue.ResourceId);

        var book = await _bookingService.GetBookingByEventIdAsync(evt.Id);
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
            IsEventBooked = IsEventBooked,
            ImageFileName = ImageFileName,
            FullAddress = fullAddress,
        };

        return PartialView("~/Views/Attendee/Partials/_ViewEventPartial.cshtml", viewModel);
    }

    // ---------------------------------------------------------------------------------------------------------------------------
    // # End of View Event Modal
    // ---------------------------------------------------------------------------------------------------------------------------

    // ---------------------------------------------------------------------------------------------------------------------------
    // # View Booking Modal
    // ---------------------------------------------------------------------------------------------------------------------------
    [HttpGet("attendee/view-booking/{EventBookingId}")]
    public async Task<IActionResult> ViewBooking(Guid EventBookingId)
    {
        var booking = await _bookingService.GetBookingByIdAsync(EventBookingId);
        if (booking == null) return NotFound();

        var payment = await _bookingService.GetPaymentByIdAsync(EventBookingId);
        if (payment == null) return NotFound();

        var evt = await _bookingService.GetEventByIdAsync(booking.EventId);
        if (evt == null) return NotFound();

        var venue = evt.Resources.FirstOrDefault()!.ResourceVenue;

        string? fullAddress = venue.AddressLine1 + ", " + (venue.AddressLine2
            != null
            ? venue.AddressLine2 + ", " : "") + venue.CityMunicipality + ", " +
            venue.Province;

        string? ImageFileName = await _bookingService.GetImageByResourceIdAsync(venue.ResourceId);

        var viewModel = new ViewBookingViewModel
        {
            Id = booking.Id,
            EventId = evt.Id,
            Name = evt.Name,
            ShortName = evt.ShortName,
            Description = evt.Description,
            PaymentMethod = payment.PaymentMethod,
            PANLastDigits = payment.PANLastDigits,
            StartAt = evt.StartAt,
            EndAt = evt.EndAt,
            Status = evt.Status,
            TotalCost = booking.TotalPrice.ToString("N2"),
            FullAddress = fullAddress,
            CreatedAt = evt.CreatedAt,
            UpdatedAt = evt.UpdatedAt,

            ImageFileName = ImageFileName,
        };

        return PartialView("~/Views/Attendee/Partials/_ViewBookingPartial.cshtml", viewModel);
    }
    // ---------------------------------------------------------------------------------------------------------------------------
    // # End of View Booking Modal
    // ---------------------------------------------------------------------------------------------------------------------------

}