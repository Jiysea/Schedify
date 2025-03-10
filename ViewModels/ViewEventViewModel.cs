using Schedify.Models;

namespace Schedify.ViewModels;

public class ViewEventViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public EventStatus Status { get; set; }
    public string EntryFee { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // To Check whether the event has a venue or not (for Draft only)
    public bool EventHasVenue { get; set; }
}