

using System.Text;
using System.Threading.Tasks;
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
    private readonly FeedbackService _feedbackService;
    private readonly StripeService _stripeService;
    public AttendeeController(ApplicationDbContext context, EventService eventService, ResourceService resourceService, BookingService bookingService, FeedbackService feedbackService, StripeService stripeService)
    {
        _context = context;
        _eventService = eventService;
        _resourceService = resourceService;
        _bookingService = bookingService;
        _feedbackService = feedbackService;
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

        var feedback = await _feedbackService.GetFeedbackByEventIdAsync(evt.Id);

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
            CardBrand = payment.CardBrand,
            PANLastDigits = payment.PANLastDigits,
            StartAt = evt.StartAt,
            EndAt = evt.EndAt,
            Status = evt.Status,
            TotalCost = booking.TotalPrice.ToString("N2"),
            CreatedAt = evt.CreatedAt,
            UpdatedAt = evt.UpdatedAt,

            ImageFileName = ImageFileName,
            FullAddress = fullAddress,
            Feedback = feedback,
            IsFeedbackGiven = feedback == null ? false : true,
        };

        return PartialView("~/Views/Attendee/Partials/_ViewBookingPartial.cshtml", viewModel);
    }
    // ---------------------------------------------------------------------------------------------------------------------------
    // # End of View Booking Modal
    // ---------------------------------------------------------------------------------------------------------------------------

    // ---------------------------------------------------------------------------------------------------------------------------
    // # Create Feedback Modal
    // ---------------------------------------------------------------------------------------------------------------------------

    [HttpGet("attendee/open-create-feedback-modal/{EventId}")]
    public IActionResult OpenCreateFeedback(Guid EventId)
    {
        var viewModel = new FeedbackViewModel
        {
            EventId = EventId,
        };

        var closeJson = $"{{\"closeModal\": true }}";
        Response.Headers.Append("HX-Trigger", closeJson);
        return PartialView("~/Views/Attendee/Partials/_CreateFeedbackPartial.cshtml", viewModel);
    }

    [HttpPost("attendee/create-feedback")]
    public async Task<IActionResult> CreateFeedback(FeedbackViewModel model)
    {
        var result = await _feedbackService.CreateFeedbackAsync(model);
        var viewModel = GetBookingsViewModel();

        if (!result.IsSuccess)
        {
            // Split error messages and add them to ModelState
            foreach (var error in result.Error!)
            {
                if (error.Key == "InvalidRating" || error.Key == "NoRating")
                {
                    var errorJson = $"{{\"clearValidations\": true, \"showToast\": {{ \"title\": \"{error.Value}\", \"icon\": \"error\", \"timer\": 3000 }} }}";
                    Response.Headers.Append("HX-Trigger", errorJson);
                    return Content(string.Empty);
                }

                if (error.Key == "Authentication" || error.Key == "NoEvent" || error.Key == "Authorization" || error.Key == "Exception")
                {
                    var errorJson = $"{{\"closeModal\": true, \"clearValidations\": true, \"showToast\": {{ \"title\": \"{error.Value}\", \"icon\": \"error\", \"timer\": 3000 }} }}";
                    Response.Headers.Append("HX-Trigger", errorJson);
                    return PartialView("~/Views/Attendee/Partials/_UpdateBookingsListPartial.cshtml", viewModel);
                }
            }
        }

        ModelState.Clear();

        // Define the Toastr event in the HX-Trigger header
        var toastrJson = "{\"closeModal\": true, \"clearValidations\": true, \"showToast\": { \"title\": \"Successfully added a feedback!\", \"icon\": \"success\", \"timer\": 3000 }}";
        Response.Headers.Append("HX-Trigger", toastrJson);
        return PartialView("~/Views/Attendee/Partials/_UpdateBookingsListPartial.cshtml", viewModel);
    }

    // ---------------------------------------------------------------------------------------------------------------------------
    // # End of Create Feedback Modal
    // ---------------------------------------------------------------------------------------------------------------------------

    // ---------------------------------------------------------------------------------------------------------------------------
    // # Update Feedback Modal
    // ---------------------------------------------------------------------------------------------------------------------------

    [HttpGet("attendee/open-edit-feedback-modal/{FeedbackId}")]
    public async Task<IActionResult> OpenEditFeedback(Guid FeedbackId)
    {
        var feedback = await _feedbackService.GetFeedbackByIdAsync(FeedbackId);
        if (feedback == null) return NotFound();

        var viewModel = new FeedbackViewModel
        {
            Id = FeedbackId,
            Rating = feedback.Rating,
            Comments = feedback.Comments,
        };

        var closeJson = $"{{\"closeModal\": true }}";
        Response.Headers.Append("HX-Trigger", closeJson);
        return PartialView("~/Views/Attendee/Partials/_UpdateFeedbackPartial.cshtml", viewModel);
    }

    [HttpPost("attendee/update-feedback")]
    public async Task<IActionResult> UpdateFeedback(FeedbackViewModel model)
    {
        var result = await _feedbackService.UpdateFeedbackAsync(model);
        var viewModel = GetBookingsViewModel();

        if (!result.IsSuccess)
        {
            // Split error messages and add them to ModelState
            foreach (var error in result.Error!)
            {
                if (error.Key == "InvalidRating" || error.Key == "NoRating")
                {
                    var errorJson = $"{{\"clearValidations\": true, \"showToast\": {{ \"title\": \"{error.Value}\", \"icon\": \"error\", \"timer\": 3000 }} }}";
                    Response.Headers.Append("HX-Trigger", errorJson);
                    return Content(string.Empty);
                }

                if (error.Key == "Authentication" || error.Key == "NoEvent" || error.Key == "NoFeedback" || error.Key == "Authorization" || error.Key == "Exception")
                {
                    var errorJson = $"{{\"closeModal\": true, \"clearValidations\": true, \"showToast\": {{ \"title\": \"{error.Value}\", \"icon\": \"error\", \"timer\": 3000 }} }}";
                    Response.Headers.Append("HX-Trigger", errorJson);
                    return PartialView("~/Views/Attendee/Partials/_UpdateBookingsListPartial.cshtml", viewModel);
                }
            }
        }

        ModelState.Clear();

        // Define the Toastr event in the HX-Trigger header
        var toastrJson = "{\"closeModal\": true, \"clearValidations\": true, \"showToast\": { \"title\": \"Successfully updated a feedback!\", \"icon\": \"success\", \"timer\": 3000 }}";
        Response.Headers.Append("HX-Trigger", toastrJson);
        return PartialView("~/Views/Attendee/Partials/_UpdateBookingsListPartial.cshtml", viewModel);
    }

    // ---------------------------------------------------------------------------------------------------------------------------
    // # End of Update Feedback Modal
    // ---------------------------------------------------------------------------------------------------------------------------


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

    // Redirects to View Ticket page
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

        var book = await _bookingService.GetBookingByEventIdAsync(evt.Id);
        bool IsEventBooked = false;

        var venue = evt.Resources.FirstOrDefault()!.ResourceVenue;

        string? fullAddress = venue.AddressLine1 + ", " + (venue.AddressLine2
            != null
            ? venue.AddressLine2 + ", " : "") + venue.CityMunicipality + ", " +
            venue.Province;

        string? ImageFileName = await _bookingService.GetImageByResourceIdAsync(venue.ResourceId);

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
    // # Private Methods
    // ---------------------------------------------------------------------------------------------------------------------------

    private BookingsViewModel GetBookingsViewModel()
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

        return viewModel;
    }
}