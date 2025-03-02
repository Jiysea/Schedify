using System.ComponentModel.DataAnnotations;

namespace Schedify.Models;

public class ResourceVenue
{
    [Key]
    public Guid Id { get; set; } = new Guid();

    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;

    [Required]
    public required int Capacity { get; set; }

    [Required]
    public required string Size { get; set; }

    [Required]
    [StringLength(150)]
    public required string AddressLine1 { get; set; }

    [StringLength(150)]
    public string? AddressLine2 { get; set; }

    [Required]
    [StringLength(50)]
    public required string CityMunicipality { get; set; }

    [Required]
    [StringLength(50)]
    public required string Province { get; set; }
}