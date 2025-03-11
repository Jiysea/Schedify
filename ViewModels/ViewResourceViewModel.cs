using Schedify.Models;

namespace Schedify.ViewModels;

public class ViewResourceViewModel
{
    public Guid? Id { get; set; }
    public string? ImageFileName { get; set; }
    public string ProviderName { get; set; } = null!;
    public string ProviderPhoneNumber { get; set; } = null!;
    public string ProviderEmail { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public ResourceType Type { get; set; }
    public string Cost { get; set; } = null!;
    public string CostType { get; set; } = null!;
    public int Quantity { get; set; }
    public int Capacity { get; set; }
    public string? Size { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? CityMunicipality { get; set; }
    public string? Province { get; set; }
    public string? Brand { get; set; }
    public Dictionary<string, string> Specifications { get; set; } = [];
    public string? Material { get; set; }
    public string? Dimensions { get; set; }
    public string? MenuItems { get; set; }
    public string? Position { get; set; }
    public string? Shift { get; set; }
    public DateTime ShiftFromDate { get; set; }
    public DateTime ShiftToDate { get; set; }
    public string? Experience { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}