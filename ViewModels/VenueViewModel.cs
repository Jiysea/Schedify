

using System.ComponentModel.DataAnnotations;

namespace Schedify.ViewModels;
public class VenueViewModel : AddResourceViewModel
{
    [Required(ErrorMessage = "This field is required.")]
    public int Capacity { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public string Size { get; set; } = null!;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(150)]
    public string AddressLine1 { get; set; } = null!;

    [StringLength(150)]
    public string? AddressLine2 { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(50)]
    public string CityMunicipality { get; set; } = null!;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(50)]
    public string Province { get; set; } = null!;
}