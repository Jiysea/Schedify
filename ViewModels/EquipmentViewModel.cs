using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Schedify.ViewModels;
public class EquipmentViewModel : AddResourceViewModel
{
    [Required(ErrorMessage = "This field is required.")]
    public string Brand { get; set; } = null!;


    public Dictionary<string, string> Specifications { get; set; } = new();

    [Required(ErrorMessage = "This field is required.")]
    public string SpecificationString { get; set; } = null!;
}