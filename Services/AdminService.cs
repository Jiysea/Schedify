using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Schedify.Data;
using Schedify.Dto;
using Schedify.Models;
using Schedify.ViewModels;
using Stripe.Checkout;
using static Schedify.Dto.PerEventDashboardDto;

namespace Schedify.Services;

public class AdminService
{
    private readonly ApplicationDbContext _context;
    private readonly UserService _userService;
    private readonly IWebHostEnvironment _environment;

    public AdminService(ApplicationDbContext context, UserService userService, IWebHostEnvironment environment)
    {
        _context = context;
        _userService = userService;
        _environment = environment;
    }

    public async Task<SummaryDashboardDto> GetDashboardSummaryAsync(DateTime? startDate, DateTime? endDate)
    {
        var utcNow = DateTime.UtcNow;
        var start = startDate.HasValue
            ? new DateTime(startDate.Value.Year, startDate.Value.Month, 1, 0, 0, 0, DateTimeKind.Utc)
            : new DateTime(utcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var end = endDate.HasValue
            ? new DateTime(endDate.Value.Year, endDate.Value.Month, 1, 23, 59, 59, DateTimeKind.Utc)
                .AddMonths(1).AddDays(-1) // end of the month
            : utcNow.Date.AddHours(23).AddMinutes(59).AddSeconds(59); // default: today end

        // For Events
        var rawEvents = await _context.Events
            .Where(e => e.CreatedAt >= start && e.CreatedAt <= end)
            .ToListAsync();

        var groupedEvents = rawEvents
            .GroupBy(e => new { e.CreatedAt.Year, e.CreatedAt.Month })
            .Select(g => new
            {
                Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                Count = g.Count()
            })
            .ToList();

        var totalMonths = (end.Year - start.Year) * 12 + end.Month - start.Month + 1;

        var monthlyEvents = Enumerable.Range(0, totalMonths)
            .Select(i => start.AddMonths(i))
            .Select(d => new MonthlyEventCountDto
            {
                Month = d.ToString("MMM", CultureInfo.InvariantCulture),
                Count = groupedEvents.FirstOrDefault(x => x.Month.Month == d.Month && x.Month.Year == d.Year)?.Count ?? 0
            })
            .ToList();

        // For Users
        var rawUsers = await _context.Users
            .Where(u => u.CreatedAt >= start && u.CreatedAt <= end)
            .ToListAsync();

        var groupedUsers = rawUsers
            .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
            .Select(g => new
            {
                Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                Count = g.Count()
            })
            .ToList();

        totalMonths = (end.Year - start.Year) * 12 + end.Month - start.Month + 1;

        var monthlyUsers = Enumerable.Range(0, totalMonths)
            .Select(i => start.AddMonths(i))
            .Select(d => new MonthlyUserCountDto
            {
                Month = d.ToString("MMM", CultureInfo.InvariantCulture),
                Count = groupedUsers.FirstOrDefault(x => x.Month.Month == d.Month && x.Month.Year == d.Year)?.Count ?? 0
            })
            .ToList();

        // For Bookings
        var rawEventBookings = await _context.EventBookings
            .Where(eb => eb.CreatedAt >= start && eb.CreatedAt <= end)
            .ToListAsync();

        var groupedEventBookings = rawEventBookings
            .GroupBy(eb => new { eb.CreatedAt.Year, eb.CreatedAt.Month })
            .Select(g => new
            {
                Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                Count = g.Count()
            })
            .ToList();

        totalMonths = (end.Year - start.Year) * 12 + end.Month - start.Month + 1;

        var monthlyEventBookings = Enumerable.Range(0, totalMonths)
            .Select(i => start.AddMonths(i))
            .Select(d => new MonthlyBookingCountDto
            {
                Month = d.ToString("MMM", CultureInfo.InvariantCulture),
                Count = groupedEventBookings.FirstOrDefault(x => x.Month.Month == d.Month && x.Month.Year == d.Year)?.Count ?? 0
            })
            .ToList();

        // For Revenue
        var rawPayments = await _context.Payments
            .Where(p => p.CreatedAt >= start && p.CreatedAt <= end)
            .ToListAsync();

        var groupedPayments = rawPayments
            .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
            .Select(g => new
            {
                Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                Total = g.Sum(x => x.Amount)
            })
            .ToList();

        totalMonths = (end.Year - start.Year) * 12 + end.Month - start.Month + 1;

        var monthlyRevenue = Enumerable.Range(0, totalMonths)
            .Select(i => start.AddMonths(i))
            .Select(d => new MonthlyRevenueCountDto
            {
                Month = d.ToString("MMM", CultureInfo.InvariantCulture),
                NetWorth = groupedPayments.FirstOrDefault(x => x.Month.Month == d.Month && x.Month.Year == d.Year)?.Total ?? 0.0m
            })
            .ToList();

        return new SummaryDashboardDto
        {
            TotalEvents = rawEvents.Count,
            TotalUsers = rawUsers.Count,
            TotalBooked = rawEventBookings.Count,
            TotalRevenue = rawPayments.Sum(p => p.Amount),
            MonthlyEvents = monthlyEvents,
            MonthlyUsers = monthlyUsers,
            MonthlyBookings = monthlyEventBookings,
            MonthlyRevenue = monthlyRevenue,
        };
    }

    // If the user changes the date range, this will be called
    // It only affects the main Event counters and the Dropdown List in Per Event
    public async Task<EventsDashboardDto> GetDashboardEventsAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Events
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(e => e.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.CreatedAt <= endDate.Value);

        var events = await query.ToListAsync();

        var dto = new EventsDashboardDto
        {
            GetEvents = events!,
            TotalEvents = events == null ? 0 : events.Count,
            TotalEventsDrafted = events?.Count(e => e.Status == EventStatus.Draft) ?? 0,
            TotalEventsOpened = events?.Count(e => e.Status == EventStatus.Open) ?? 0,
            TotalEventsOngoing = events?.Count(e => e.Status == EventStatus.Ongoing) ?? 0,
            TotalEventsCompleted = events?.Count(e => e.Status == EventStatus.Completed) ?? 0,
            TotalEventsCancelled = events?.Count(e => e.Status == EventStatus.Cancelled) ?? 0,
            TotalEventsPostponed = events?.Count(e => e.Status == EventStatus.Postponed) ?? 0,
        };

        return dto;
    }

    public async Task<PerEventDashboardDto> GetEventDashboardByIdAsync(Guid? EventId)
    {
        Event? selectedEvent = await _context.Events
            .Where(e => e.Id == EventId)
            .Include(e => e.Resources)
                .ThenInclude(r => r.ResourceVenue)
            .Include(e => e.Resources)
                .ThenInclude(r => r.ResourceEquipment)
            .Include(e => e.Resources)
                .ThenInclude(r => r.ResourceFurniture)
            .Include(e => e.Resources)
                .ThenInclude(r => r.ResourceCatering)
            .Include(e => e.Resources)
                .ThenInclude(r => r.ResourcePersonnel)
            .Include(e => e.EventBookings)
            .Include(e => e.Feedbacks)
            .FirstOrDefaultAsync();

        Guid userId = selectedEvent?.UserId ?? Guid.Empty;

        var user = await _context.Users
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync();

        var avatar = await _context.Images
            .Where(i => i.UserId == userId)
            .Select(i => i.ImageFileName)
            .FirstOrDefaultAsync();

        var FullName = user != null ? GetFullName(user) : "N/A";

        var resources = selectedEvent?.Resources?.ToList() ?? new();
        List<Guid> resourceIds = resources.Select(r => r.Id).ToList();

        var images = await _context.Images
            .Where(img => img.ResourceId.HasValue && resourceIds.Contains(img.ResourceId.Value))
            .ToListAsync();

        var resourceImage = images
            .Where(img => img.ResourceId.HasValue)
            .ToDictionary(
                img => img.ResourceId!.Value,
                img => img.ImageFileName
            );

        var Time = selectedEvent != null
        ? $"{selectedEvent.StartAt:MMM dd, yyyy} - {selectedEvent.EndAt:MMM dd, yyyy} ({selectedEvent.TimeStart} - {selectedEvent.TimeEnd})"
        : "N/A";

        // Get total Entry Fee per EventBooking and Resources Input Cost
        decimal totalEntryFee = selectedEvent?.EventBookings
        .Where(eb => eb.Status == BookingStatus.Paid || eb.Status == BookingStatus.Confirmed)
        .Sum(eb => eb.TotalPrice) ?? 0m;

        decimal totalResourceCosts = 0m;

        if (selectedEvent != null && selectedEvent.Resources != null && selectedEvent.EventBookings.Any())
        {
            var duration = selectedEvent.EndAt - selectedEvent.StartAt;

            totalResourceCosts = selectedEvent.Resources.Sum(r =>
            {
                return r.CostType switch
                {
                    // Per Hour: hours × cost
                    "Per Hour" => (decimal)duration.TotalHours * r.Cost,

                    // Per Day: days × cost
                    "Per Day" => (decimal)duration.TotalDays * r.Cost,

                    // Fixed Rate / In Bulk: cost itself
                    "Fixed Rate" or "In Bulk" => r.Cost,

                    // Per Unit: quantity × cost
                    "Per Unit" => r.ResourceEquipment != null
                                    ? r.ResourceEquipment.Quantity * r.Cost
                                    : r.ResourceFurniture != null
                                        ? r.ResourceFurniture.Quantity * r.Cost
                                        : 0m,

                    // Per Serving: guest-capacity × cost
                    "Per Serving" => r.ResourceCatering?.GuestCapacity * r.Cost ?? 0m,

                    // Default fallback
                    _ => 0m
                };
            });
        }

        var Feedbacks = selectedEvent?.Feedbacks?.ToList() ?? new();
        List<Guid> feedbackIds = Feedbacks.Select(f => f.Id).ToList();

        var images2 = await _context.Images
                    .Where(img => img.UserId.HasValue && feedbackIds.Contains(img.UserId.Value))
                    .ToListAsync();

        var feedbackAvatars = images2
            .Where(img => img.UserId.HasValue)
            .ToDictionary(
                img => img.UserId!.Value,
                img => img.ImageFileName
            );

        // decimal ratio = totalEntryFee > 0
        //     ? totalResourceCosts / totalEntryFee * 100
        //     : 0m;

        // decimal profitMargin = totalEntryFee > 0
        //     ? (totalEntryFee - totalResourceCosts) / totalEntryFee * 100
        //     : 0m;

        var dto = new PerEventDashboardDto
        {
            OrganizerName = FullName,
            OrganizerAvatar = avatar,
            SelectedEvent = selectedEvent,
            GetResources = resources,
            GetFeedbacks = Feedbacks,
            ResourceImage = resourceImage,
            FeedbacksAvatar = feedbackAvatars,
            Time = Time,
            TotalBooked = selectedEvent?.EventBookings.Count ?? 0,
            TotalPaidBookings = selectedEvent?.EventBookings.Count(b => b.Status == BookingStatus.Paid) ?? 0,
            TotalConfirmedBookings = selectedEvent?.EventBookings.Count(b => b.Status == BookingStatus.Confirmed) ?? 0,
            TotalCancelledBookings = selectedEvent?.EventBookings.Count(b => b.Status == BookingStatus.Cancelled) ?? 0,
            TotalRefundedBookings = selectedEvent?.EventBookings.Count(b => b.Status == BookingStatus.Refunded) ?? 0,
            TotalFeedbacks = selectedEvent?.Feedbacks.Count ?? 0,
            OverallRating = selectedEvent?.Feedbacks
                .Select(f => (decimal?)f.Rating)
                .Average() ?? 0.0m,
            TotalEntryFee = totalEntryFee,
            TotalResourceCosts = totalResourceCosts,
        };

        return dto;
    }

    private static string GetFullName(User user)
    {
        var parts = new List<string> { user.FirstName! };

        if (!string.IsNullOrWhiteSpace(user.MiddleName))
            parts.Add(user.MiddleName);

        parts.Add(user.LastName!);

        if (!string.IsNullOrWhiteSpace(user.ExtensionName))
            parts.Add(user.ExtensionName);

        return string.Join(" ", parts);
    }
}