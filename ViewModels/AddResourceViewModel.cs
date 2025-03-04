using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
namespace Schedify.ViewModels;

public class AddResourceViewModel
{
    [Required(ErrorMessage = "This field is required.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(300)]
    [DefaultValue("No description")]
    public string Description { get; set; } = null!;

    public ResourceType Type { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [DataType(DataType.Currency)]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Cost { get; set; }

    public string CostType { get; set; } = null!;

    [Required(ErrorMessage = "This field is required.")]
    public int Quantity { get; set; }

    public IEnumerable<ResourceType> ResourceTypes { get; set; } = Enum.GetValues<ResourceType>();
}