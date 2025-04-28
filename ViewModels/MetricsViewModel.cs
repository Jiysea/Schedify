
using Schedify.Models;

namespace Schedify.ViewModels;

public class MetricsViewModel
{
    // Avatar
    public string? AvatarFileName { get; set; }

    // Global
    public int GlobalTotalEvents { get; set; }
    public int GlobalTotalEventsDrafted { get; set; }
    public int GlobalTotalEventsOpened { get; set; }
    public int GlobalTotalEventsCompleted { get; set; }
    public int GlobalTotalEventsCancelled { get; set; }
    public int GlobalTotalEventsPostponed { get; set; }
    public int GlobalTotalBooked { get; set; }
    public int GlobalTotalPaidBookings { get; set; }
    public int GlobalTotalConfirmedBookings { get; set; }
    public int GlobalTotalCancelledBookings { get; set; }
    public int GlobalTotalRefundedBookings { get; set; }
    public decimal GlobalTotalNetWorth { get; set; }
    public decimal GlobalTotalResourcesWorth { get; set; }
    public decimal GlobalTotalGainedPercentage { get; set; }
    public decimal GlobalTotalResourceEfficiency { get; set; }

    // Per Event
}