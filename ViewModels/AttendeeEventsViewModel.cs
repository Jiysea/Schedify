using Schedify.Models;

namespace Schedify.ViewModels;

public class AttendeeEventsViewModel
{
     public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<Event> DraftEvents { get; set; } = new List<Event>();
}