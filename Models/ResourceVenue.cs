using System.ComponentModel.DataAnnotations;

namespace Schedify.Models;

public class ResourceVenue
{
    [Key]
    public Guid Id { get; set; } = new Guid();

    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;

    [Required]
    public int Capacity { get; set; }

    [Required]
    public string Size { get; set; } = null!;

    [Required]
    [StringLength(150)]
    public string AddressLine1 { get; set; } = null!;

    [StringLength(150)]
    public string? AddressLine2 { get; set; }

    [Required]
    [StringLength(50)]
    public string CityMunicipality { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string Province { get; set; } = null!;
}