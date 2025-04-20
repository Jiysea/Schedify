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
            .Where(e => e.UserId == Guid.Parse(_userService.GetUserId()!))
            .OrderByDescending(e => e.CreatedAt)
            .ToList();
    }

    public async Task<EventBooking?> GetBookingByIdAsync(Guid EventId)
    {
        return await _context.EventBookings.FirstOrDefaultAsync(eb => eb.EventId == EventId);
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