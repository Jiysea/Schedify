using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Globalization;
using Schedify.Models;
namespace Schedify.ViewModels;

public class AddResourceViewModel
{
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

    public ResourceType Type { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public string Cost { get; set; } = null!;

    [Required(ErrorMessage = "This field is required.")]
    public string CostType { get; set; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Value must be at least 1.")]
    public int Quantity { get; set; } = 1;

    public int Capacity { get; set; }

    public string? Size { get; set; }

    [StringLength(150)]
    public string? AddressLine1 { get; set; }

    [StringLength(150)]
    public string? AddressLine2 { get; set; }

    [StringLength(50)]
    public string? CityMunicipality { get; set; }

    [StringLength(50)]
    public string? Province { get; set; }

    [DefaultValue("No Brand")]
    public string? Brand { get; set; }

    public Dictionary<string, string> Specifications { get; set; } = [];

    public string? Material { get; set; }

    public string? Dimensions { get; set; }

    public string? MenuItems { get; set; }

    public string? PriceItems { get; set; }

    public string? Position { get; set; }

    [StringLength(30)]
    public string? Shift { get; set; }

    [StringLength(30)]
    [DefaultValue("None")]
    public string? Experience { get; set; }

    public IEnumerable<ResourceType> ResourceTypes { get; set; } = Enum.GetValues<ResourceType>();

    [DataType(DataType.Currency)]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal CostAsDecimal
    {
        get
        {
            if (decimal.TryParse(Cost, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture, out decimal parsedValue))
            {
                return parsedValue;
            }
            return 0; // Or throw an error if necessary
        }
    }
}