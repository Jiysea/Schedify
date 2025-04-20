using System;

namespace Schedify.ViewModels;

public class ReceiptViewModel
{
    public string? ReferenceNumber { get; set; } // Essentially some part of EventBookingId
    public decimal Amount { get; set; }
    public string? EventShortName { get; set; }
    public string? PaymentMethod { get; set; }
    public string? CardBrand { get; set; }
    public string? Last4 { get; set; }
    public DateTime CreatedAt { get; set; }
}
