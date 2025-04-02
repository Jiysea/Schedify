using Schedify.Models;

namespace Schedify.ViewModels;

public class EventsViewModel
{
    public List<Event> DraftEvents { get; set; } = new List<Event>();
    public List<Event> PublishedEvents { get; set; } = new List<Event>();
    public List<Event> ConcludedEvents { get; set; } = new List<Event>();
    public CUEventViewModel CUEventViewModel { get; set; } = new CUEventViewModel();
    public ViewEventViewModel ViewEventViewModel { get; set; } = new ViewEventViewModel();

    // Dictionary to store EventId and the number of attendees for that event
    public Dictionary<Guid, int> EventAttendeeCounts { get; set; } = [];

    // Check if there's already resources in DraftEvents
    public Dictionary<Guid, bool> HasVenues { get; set; } = [];
    public Dictionary<Guid, bool> IsOpenable { get; set; } = [];
}