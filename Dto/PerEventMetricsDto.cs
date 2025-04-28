namespace Schedify.Dto;

public class PerEventMetricsDto
{
    public int TotalEvents { get; set; }
    public int TotalEventsOpened { get; set; }
    public int TotalEventsCompleted { get; set; }
    public int TotalEventsCancelled { get; set; }
    public int TotalEventsPostponed { get; set; }
    public int TotalBooked { get; set; }
    public decimal TotalNetWorth { get; set; }
    public decimal TotalResourcesSpentWorth { get; set; }
    public decimal TotalGainedPercentage { get; set; }
    public decimal TotalResourceEfficiency { get; set; }
}