using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
namespace Schedify.ViewModels;

public class AddResourceViewModel
{
    [Required(ErrorMessage = "This field is required.")]
    public required string ResourceName { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(300)]
    [DefaultValue("No description")]
    public required string Description { get; set; }

    public ResourceType Type { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [DataType(DataType.Currency)]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public required decimal Cost { get; set; }

    public required string CostType { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public required int Quantity { get; set; }
}