using System.ComponentModel.DataAnnotations;

namespace Schedify.Models;

public class ResourceCatering
{
    [Key]
    public Guid Id { get; set; } = new Guid();

    public Guid ResourceId { get; set; }

    [Required]
    public string MenuItems { get; set; } = null!; // Comma-separated items

    [Required]
    public string PriceItems { get; set; } = null!; // Comma-separated values

    // Dependents
    public Resource Resource { get; set; } = null!;

}