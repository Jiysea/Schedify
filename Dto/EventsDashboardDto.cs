using Schedify.Models;

namespace Schedify.Dto;

public class EventsDashboardDto
{
    public List<Event> GetEvents { get; set; } = new();

    // Counters
    public int TotalEvents { get; set; }
    public int TotalEventsDrafted { get; set; }
    public int TotalEventsOpened { get; set; }
    public int TotalEventsOngoing { get; set; }
    public int TotalEventsCompleted { get; set; }
    public int TotalEventsCancelled { get; set; }
    public int TotalEventsPostponed { get; set; }
}