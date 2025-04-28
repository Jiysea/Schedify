using Schedify.Models;

namespace Schedify.ViewModels;

public class ViewEventViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public TimeSpan TimeStart { get; set; }
    public TimeSpan TimeEnd { get; set; }
    public EventStatus Status { get; set; }
    public string EntryFee { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // To Check whether the event has a venue or not (for Draft only)
    public bool EventHasVenue { get; set; }
    public bool IsEventOpenable { get; set; }

    // Only for attendees
    public bool IsEventBooked { get; set; }
    public string? ImageFileName { get; set; }
    public string? FullAddress { get; set; }
}