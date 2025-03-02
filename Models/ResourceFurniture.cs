using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Schedify.Models;

public class ResourceFurniture
{
    [Key]
    public Guid Id { get; set; } = new Guid();

    public Guid ResourceId { get; set; }

    [Required]
    public required string Material { get; set; }

    [Required]
    public required string Dimensions { get; set; } // comma-separated key-value pairs (height: 0, weight: 0)

    // Dependents
    public Resource Resource { get; set; } = null!;
}