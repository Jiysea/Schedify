
using Schedify.Models;

namespace Schedify.ViewModels;

public class CheckoutViewModel
{
    public string EventTitle { get; set; } = string.Empty;
    public long Amount { get; set; } // In pesos
    public string Currency { get; set; } = "php";
    public int Quantity { get; set; } = 1;
}