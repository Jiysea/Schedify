namespace Schedify.Dto;

public class GlobalMetricsDto
{
    public int TotalEvents { get; set; }
    public int TotalEventsDrafted { get; set; }
    public int TotalEventsOpened { get; set; }
    public int TotalEventsCompleted { get; set; }
    public int TotalEventsCancelled { get; set; }
    public int TotalEventsPostponed { get; set; }
    public int TotalBooked { get; set; }
    public int TotalPaidBookings { get; set; }
    public int TotalConfirmedBookings { get; set; }
    public int TotalCancelledBookings { get; set; }
    public int TotalRefundedBookings { get; set; }
    public decimal TotalNetWorth { get; set; }
    public decimal TotalResourcesWorth { get; set; }
    public decimal TotalGainedPercentage { get; set; }
    public decimal TotalResourceEfficiency { get; set; }
}