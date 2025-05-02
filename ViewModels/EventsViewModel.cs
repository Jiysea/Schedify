using Schedify.Models;

namespace Schedify.ViewModels;

public class EventsViewModel
{
    // Chat
    public ChatViewModel ChatViewModel { get; set; } = new ChatViewModel();

    // Avatar
    public string? AvatarFileName { get; set; }

    public List<Event> DraftEvents { get; set; } = new List<Event>();
    public List<Event> PublishedEvents { get; set; } = new List<Event>();
    public List<Event> ConcludedEvents { get; set; } = new List<Event>();
    public CUEventViewModel CUEventViewModel { get; set; } = new CUEventViewModel();
    public ViewEventViewModel ViewEventViewModel { get; set; } = new ViewEventViewModel();
    public CheckoutViewModel CheckoutViewModel { get; set; } = new CheckoutViewModel();

    // Dictionary to store EventId and the number of attendees for that event
    public Dictionary<Guid, int> EventAttendeeCounts { get; set; } = [];

    // Check if there's already resources in DraftEvents
    public Dictionary<Guid, bool> HasVenues { get; set; } = [];
    public Dictionary<Guid, bool> IsOpenable { get; set; } = [];

    // Attendee
    public List<Event> OpenedEvents { get; set; } = new List<Event>();
    public List<EventBooking> BookedEvents { get; set; } = new List<EventBooking>();
    public Dictionary<Guid, string?> EventImages { get; set; } = new Dictionary<Guid, string?>();
    public Dictionary<Guid, string?> OrganizerImages { get; set; } = new Dictionary<Guid, string?>();
}