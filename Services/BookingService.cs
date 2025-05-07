using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Schedify.Data;
using Schedify.Models;
using Schedify.ViewModels;
using Stripe.Checkout;

namespace Schedify.Services;

public class BookingService
{
    private readonly ApplicationDbContext _context;
    private readonly UserService _userService;
    private readonly IWebHostEnvironment _environment;

    public BookingService(ApplicationDbContext context, UserService userService, IWebHostEnvironment environment)
    {
        _context = context;
        _userService = userService;
        _environment = environment;
    }

    public List<EventBooking> GetBookingsByUser()
    {
        return _context.EventBookings
            .Include(eb => eb.Event)
            .Where(e => e.UserId == Guid.Parse(_userService.GetUserId()!))
            .OrderByDescending(e => e.CreatedAt)
            .ToList();
    }

    public List<Event> GetEventsByBooking(List<EventBooking> bookings) // Assuming the Bookings are filtered by user
    {
        var eventIds = bookings
            .Select(b => b.EventId)
            .ToList();

        return _context.Events
            .Include(e => e.Resources)
            .Where(e => eventIds.Contains(e.Id))
            .ToList();
    }

    public Dictionary<Guid, Resource?> GetResourceByEvents(List<Event> events)
    {
        var eventIds = events
            .Select(e => e.Id)
            .ToList();

        var resources = _context.Resources
            .Include(r => r.ResourceVenue)
            .Where(r => eventIds.Contains(r.EventId))
            .ToList();

        var result = events
            .ToDictionary(
                e => e.Id,
                e => resources.FirstOrDefault(r => r.EventId == e.Id)
            );

        return result;
    }

    public Dictionary<Guid, string?> GetEventVenueImages(List<EventBooking> bookings)
    {
        // Get (EventId, VenueResourceId)
        var eventIds = bookings
            .Select(b => b.EventId)
            .ToList();

        var events = _context.Events
            .Where(e => eventIds.Contains(e.Id))
            .ToList();

        var eventVenueResources = events
            .Select(e => new
            {
                EventId = e.Id,
                VenueResourceId = e.Resources.FirstOrDefault(r => r.ResourceType == ResourceType.Venue)?.Id
            })
            .Where(x => x.VenueResourceId.HasValue)
            .ToList();

        var venueResourceIds = eventVenueResources
        .Select(x => x.VenueResourceId!.Value)
        .ToList();

        // Get images for those venue resource IDs
        var images = _context.Images
            .Where(img => img.ResourceId.HasValue && venueResourceIds.Contains(img.ResourceId.Value))
            .ToList();

        // Match EventId to ImageFileName (can be null if no image found)
        var result = eventVenueResources
            .ToDictionary(
                ev => ev.EventId,
                ev =>
                {
                    var image = images.FirstOrDefault(img => img.ResourceId == ev.VenueResourceId);
                    return image?.ImageFileName;
                });

        return result;
    }

    public async Task<string?> GetImageByResourceIdAsync(Guid ResourceId)
    {
        var image = await _context.Images
             .FirstOrDefaultAsync(e => e.ResourceId == ResourceId);
        return image != null ? image.ImageFileName : null;
    }

    public async Task<EventBooking?> GetBookingByIdAsync(Guid EventBookingId)
    {
        return await _context.EventBookings.FindAsync(EventBookingId);
    }

    public async Task<EventBooking?> GetBookingByEventIdAsync(Guid EventId)
    {
        return await _context.EventBookings.FirstOrDefaultAsync(eb => eb.EventId == EventId && eb.UserId == Guid.Parse(_userService.GetUserId()!));
    }

    public async Task<Event?> GetEventByIdAsync(Guid EventId)
    {
        return await _context.Events
            .Include(e => e.Resources)
                .ThenInclude(r => r.ResourceVenue)
            .FirstOrDefaultAsync(e => e.Id == EventId);
    }

    public async Task<Payment?> GetPaymentByIdAsync(Guid EventBookingId)
    {
        return await _context.Payments.FirstOrDefaultAsync(eb => eb.EventBookingId == EventBookingId);
    }

    public async Task<CheckoutViewModel?> PrepareBookingDetailsAsync(Guid EventId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Load event info from DB
            var evt = _context.Events.FirstOrDefault(e => e.Id == EventId);
            if (evt == null) return null;

            var user = await _userService.GetUserAsync();
            if (user == null) return null;

            var booking = new EventBooking
            {
                UserId = user.Id,
                EventId = EventId,
                Status = BookingStatus.Pending,
                TotalPrice = evt.EntryFee,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.EventBookings.Add(booking);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return new CheckoutViewModel()
            {
                EventTitle = evt.ShortName,
                Amount = (long)evt.EntryFee, // convert PHP to centavo (Stripe uses smallest currency unit)
                Currency = "php"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            await transaction.RollbackAsync();
            return null;
        }
    }
    public async Task<(bool IsSuccess, Dictionary<string, string>? Error, EventBooking? EventBooking)> CancelBookingByIdAsync(Guid EventBookingId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var booking = await _context.EventBookings
                .FirstOrDefaultAsync(eb => eb.Id == EventBookingId);
            
            // var user = await _userService.GetUserAsync();
            // if (user == null)
            //     return (false, new Dictionary<string, string> { { "Authentication", "User must be authenticated." } }, null);
            
            if (booking == null) return (false, new Dictionary<string, string> { { "NoBooking", "Booking not found." } }, null);

            // if (booking.UserId != user.Id)
            //     return (false, new Dictionary<string, string> { { "Authorization", "Invalid user." } }, null);

            if (booking.Status != BookingStatus.Paid)
            {
                return (false, new Dictionary<string, string> { { "NotPaid", "Your booking is not paid." } }, null);
            }

            booking.Status = BookingStatus.Cancelled;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return (true, null, booking);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, new Dictionary<string, string> { { "Exception", $"An error occurred: {ex.Message}" } }, null);
        }
    }

    // public async Task<(bool IsSuccess, Dictionary<string, string>? Error, Event? Event)> BookEventAsync(CUBookingViewModel model)
    // {
    //     using var transaction = await _context.Database.BeginTransactionAsync();
    //     try
    //     {

    //     }
    //     catch (Exception ex)
    //     {
    //         await transaction.RollbackAsync();
    //         return (false, new Dictionary<string, string> { { "Exception", $"An error occurred: {ex.Message}" } }, null);
    //     }
    // }
}