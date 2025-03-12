using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Schedify.Models;

namespace Schedify.ViewModels;

public class EventBookingViewModel
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public BookingStatus Status { get; set; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}