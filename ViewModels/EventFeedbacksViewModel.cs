
using Schedify.Models;

namespace Schedify.ViewModels;

public class EventFeedbacksViewModel
{
    // Avatar
    public string? AvatarFileName { get; set; }
    
    // For dropdown
    public string? SelectedName { get; set; } = null!;
    public Guid EventId { get; set; }
    public List<Event> Events { get; set; } = new List<Event>();

    // For feedback body (list)
    public List<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public Dictionary<Guid, string?> AvatarImages { get; set; } = []; // Maps FeedbackId → ImageFileName
    public Dictionary<Guid, string?> UserFullname { get; set; } = []; // Maps FeedbackId → User

}