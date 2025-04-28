using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Schedify.Data;
using Schedify.Dto;
using Schedify.Models;
using Schedify.ViewModels;
using Stripe.Checkout;

namespace Schedify.Services;

public class MetricService
{
    private readonly ApplicationDbContext _context;
    private readonly UserService _userService;
    private readonly IWebHostEnvironment _environment;

    public MetricService(ApplicationDbContext context, UserService userService, IWebHostEnvironment environment)
    {
        _context = context;
        _userService = userService;
        _environment = environment;
    }

    public async Task<GlobalMetricsDto> GetGlobalMetricsAsync()
    {
        var UserId = Guid.Parse(_userService.GetUserId()!);

        // Events
        var Events = await _context.Events
            .Where(e => e.UserId == UserId)
            .Include(e => e.EventBookings)
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
            .ToListAsync();
        int TotalEvents = Events.Count;
        int TotalEventsDrafted = Events.Count(e => e.Status == EventStatus.Draft);
        int TotalEventsOpened = Events.Count(e => e.Status == EventStatus.Open);
        int TotalEventsCompleted = Events.Count(e => e.Status == EventStatus.Completed);
        int TotalEventsPostponed = Events.Count(e => e.Status == EventStatus.Postponed);
        int TotalEventsCancelled = Events.Count(e => e.Status == EventStatus.Cancelled);

        // Bookings
        int TotalBooked = Events.Sum(e => e.EventBookings.Count);
        int TotalPaidBookings = Events.SelectMany(e => e.EventBookings).Count(eb => eb.Status == BookingStatus.Paid);
        int TotalConfirmedBookings = Events.SelectMany(e => e.EventBookings).Count(eb => eb.Status == BookingStatus.Confirmed);
        int TotalCancelledBookings = Events.SelectMany(e => e.EventBookings).Count(eb => eb.Status == BookingStatus.Cancelled);
        int TotalRefundedBookings = Events.SelectMany(e => e.EventBookings).Count(eb => eb.Status == BookingStatus.Refunded);

        // Money
        decimal TotalNetWorth = Events.Sum(e => e.EventBookings.Count * e.EntryFee);

        // Compute total cost for *all* resources across *all* events
        decimal TotalResourcesWorth = Events.Sum(ev =>
        {
            if (ev.EventBookings.Count == 0)
                return 0m; // No bookings, no resource cost

            return ev.Resources.Sum(r =>
            {
                // shorthand to get event duration
                var duration = ev.EndAt - ev.StartAt;

                var totalHoursPerDay = (ev.EndAt - ev.StartAt).TotalHours;
                var StartHour = ev.StartAt.Hour;
                var EndHour = ev.EndAt.Hour;

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
                    "Per Serving" => r.ResourceCatering.GuestCapacity * r.Cost,
                    // anything else:
                    _ => 0m
                };
            });
        });

        decimal Ratio = 0.0m;
        decimal TotalGainedPercentage = 0.0m;
        decimal TotalResourceEfficiency = 0.0m;

        if (TotalBooked != 0)
        {
            Ratio = TotalNetWorth / TotalResourcesWorth;
            TotalGainedPercentage = (Ratio - 1) * 100;
            TotalResourceEfficiency = 1 / Ratio;
        }

        return new GlobalMetricsDto
        {
            TotalEvents = TotalEvents,
            TotalEventsDrafted = TotalEventsDrafted,
            TotalEventsOpened = TotalEventsOpened,
            TotalEventsCompleted = TotalEventsCompleted,
            TotalEventsCancelled = TotalEventsCancelled,
            TotalEventsPostponed = TotalEventsPostponed,
            TotalBooked = TotalBooked,
            TotalPaidBookings = TotalPaidBookings,
            TotalConfirmedBookings = TotalConfirmedBookings,
            TotalCancelledBookings = TotalCancelledBookings,
            TotalRefundedBookings = TotalRefundedBookings,
            TotalNetWorth = TotalNetWorth,
            TotalResourcesWorth = TotalResourcesWorth,
            TotalGainedPercentage = TotalGainedPercentage,
            TotalResourceEfficiency = TotalResourceEfficiency
        };
    }

    public async Task<PerEventMetricsDto> GetPerEventMetricsAsync()
    {
        var UserId = Guid.Parse(_userService.GetUserId()!);
        var Events = await _context.Events.Where(e => e.UserId == UserId).Include(e => e.EventBookings).ToListAsync();
        var TotalBookedPerEvent = Events.ToDictionary(
            e => e.Id,
            e => e.EventBookings.Count
        );

        return new PerEventMetricsDto
        {

        };
    }
}