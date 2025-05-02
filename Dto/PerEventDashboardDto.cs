using Schedify.Models;

namespace Schedify.Dto;

public class PerEventDashboardDto
{
    public Event? SelectedEvent { get; set; }
    public string? Time { get; set; }
    public string? OrganizerName { get; set; }
    public string? OrganizerAvatar { get; set; }
    public List<Resource> GetResources { get; set; } = new();
    public List<Feedback> GetFeedbacks { get; set; } = new();
    public Dictionary<Guid, string?> ResourceImage { get; set; } = new();

    public int TotalBooked { get; set; }
    public int TotalPaidBookings { get; set; }
    public int TotalConfirmedBookings { get; set; }
    public int TotalCancelledBookings { get; set; }
    public int TotalRefundedBookings { get; set; }

    public int TotalFeedbacks { get; set; }
    public decimal OverallRating { get; set; }
    public Dictionary<Guid, string?> FeedbacksAvatar { get; set; } = new();

    public decimal TotalEntryFee { get; set; }
    public decimal TotalResourceCosts { get; set; }

}