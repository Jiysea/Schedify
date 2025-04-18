using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Schedify.Attributes;

namespace Schedify.ViewModels;

public class CUResourceViewModel
{
    public Guid Id { get; set; }
    public string? ImageFileName { get; set; }
    public Guid EventId { get; set; }

    [AllowedExtensions([".jpg", ".jpeg", ".png"])]
    [MaxFileSize(5)]
    public IFormFile? ImageFile { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(250)]
    public string ProviderName { get; set; } = null!;

    [Required(ErrorMessage = "This field is required.")]
    public string ProviderPhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "This field is required.")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
    ErrorMessage = "Value is not a valid email address.")]
    public string ProviderEmail { get; set; } = null!;

    [Required(ErrorMessage = "This field is required.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(300)]
    [DefaultValue("No description")]
    public string Description { get; set; } = null!;
    public ResourceType ResourceType { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public string CostAsString { get; set; } = null!;

    [DataType(DataType.Currency)]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Cost
    {
        get
        {
            if (decimal.TryParse(CostAsString, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture, out decimal parsedValue))
            {
                return parsedValue;
            }
            return 0; // Or throw an error if necessary
        }

    }

    [Required(ErrorMessage = "This field is required.")]
    public string CostType { get; set; } = null!;

    [RequiredRange([ResourceType.Equipment, ResourceType.Furniture])]
    public int Quantity { get; set; } = 1;

    public int Capacity { get; set; }

    public decimal Size { get; set; }

    [StringLength(150)]
    public string? AddressLine1 { get; set; }

    [StringLength(150)]
    public string? AddressLine2 { get; set; }

    [StringLength(50)]
    public string CityMunicipality { get; set; } = "City of Davao";

    [StringLength(50)]
    public string Province { get; set; } = "Davao del Sur";

    [DefaultValue("No Brand")]
    public string? Brand { get; set; }

    public Dictionary<string, string>? Specifications { get; set; } = [];

    public FurnitureMaterial Material { get; set; }
    public string? OtherMaterial { get; set; }

    public string? Dimensions { get; set; }
    public string? Warranty { get; set; }
    public string? WarrantyDuration { get; set; }

    [RequiredRange([ResourceType.Catering])]
    public int GuestCapacity { get; set; }
    public List<string>? MenuItems { get; set; } // List<string>

    public string? Position { get; set; }
    public string? ShiftStartString { get; set; }
    public string? ShiftEndString { get; set; }

    public TimeSpan ShiftStart
    {
        get
        {
            return DateTime.TryParseExact(ShiftStartString, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime shiftDateTime)
                ? shiftDateTime.TimeOfDay
                : TimeSpan.Zero; // Default to 00:00 if parsing fails
        }
    }

    public TimeSpan ShiftEnd
    {
        get
        {
            return DateTime.TryParseExact(ShiftEndString, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime shiftDateTime)
                ? shiftDateTime.TimeOfDay
                : TimeSpan.Zero; // Default to 00:00 if parsing fails
        }
    }
    public string? Experience { get; set; }
    public string? ExperienceType { get; set; }

    public IEnumerable<ResourceType> ResourceTypes { get; set; } = Enum.GetValues<ResourceType>();
    public IEnumerable<FurnitureMaterial> FurnitureMaterials { get; set; } = Enum.GetValues<FurnitureMaterial>();
    public bool IsOtherMaterial = false;

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}