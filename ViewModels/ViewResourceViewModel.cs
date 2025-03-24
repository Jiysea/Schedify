
namespace Schedify.ViewModels;

public class ViewResourceViewModel
{
    public bool IsUsed { get; set; }
    public Guid? Id { get; set; }
    public string? ImageFileName { get; set; }

    // General Resource
    public string ProviderName { get; set; } = null!;
    public string ProviderPhoneNumber { get; set; } = null!;
    public string ProviderEmail { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public ResourceType ResourceType { get; set; }
    public decimal Cost { get; set; }
    public string CostType { get; set; } = null!;

    // Venue
    public int Capacity { get; set; }
    public decimal Size { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? CityMunicipality { get; set; }
    public string? Province { get; set; }

    // Equipment
    public string? Brand { get; set; }
    public Dictionary<string, string> Specifications { get; set; } = [];
    public int Quantity { get; set; } // Equipment & Furniture
    public DateTime? Warranty { get; set; } // Equipment & Furniture

    // Furniture
    public FurnitureMaterial Material { get; set; }
    public string? OtherMaterial { get; set; }
    public string? Dimensions { get; set; }

    // Catering
    public int GuestCapacity { get; set; }
    public List<string>? MenuItems { get; set; }

    // Personnel
    public string? Position { get; set; }
    public TimeSpan ShiftStart { get; set; }
    public TimeSpan ShiftEnd { get; set; }
    public string? Experience { get; set; }


    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}