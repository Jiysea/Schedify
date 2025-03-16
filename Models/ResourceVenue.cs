using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Size { get; set; }

    [Required]
    [StringLength(150)]
    public string AddressLine1 { get; set; } = null!;

    [StringLength(150)]
    public string? AddressLine2 { get; set; }

    [Required]
    [StringLength(50)]
    public string CityMunicipality { get; set; } = "City of Davao";

    [Required]
    [StringLength(50)]
    public string Province { get; set; } = "Davao del Sur";
}