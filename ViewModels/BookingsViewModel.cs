
using Schedify.Models;

namespace Schedify.ViewModels;

public class BookingsViewModel
{
    // Initialize
    public ViewBookingViewModel ViewBookingViewModel = new ViewBookingViewModel();
    public FeedbackViewModel FeedbackViewModel = new FeedbackViewModel();

    // Important Stuffs
    public List<EventBooking> BookedEvents { get; set; } = new List<EventBooking>();
    public List<Event> Events { get; set; } = new List<Event>();
    public Dictionary<Guid, Resource?> EventResources { get; set; } = new Dictionary<Guid, Resource?>();
    public Dictionary<Guid, string?> BookingImages { get; set; } = new Dictionary<Guid, string?>();
    
}