
namespace Schedify.ViewModels;

public class ViewBookingViewModel
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = null!;
    public string ShortName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? PaymentMethod { get; set; }
    public string? PANLastDigits { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public EventStatus Status { get; set; }
    public string TotalCost { get; set; } = null!;
    public string FullAddress { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string? ImageFileName { get; set; }
}