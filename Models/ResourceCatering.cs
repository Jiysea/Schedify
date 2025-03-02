using System.ComponentModel.DataAnnotations;

namespace Schedify.Models;

public class ResourceCatering
{
    [Key]
    public Guid Id { get; set; } = new Guid();

    public Guid ResourceId { get; set; }

    [Required]
    public required string MenuItems { get; set; } // Comma-separated items

    [Required]
    public required string PriceItems { get; set; } // Comma-separated values

    // Dependents
    public Resource Resource { get; set; } = null!;

}