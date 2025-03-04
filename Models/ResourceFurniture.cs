using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Schedify.Models;

public class ResourceFurniture
{
    [Key]
    public Guid Id { get; set; } = new Guid();

    public Guid ResourceId { get; set; }

    [Required]
    public string Material { get; set; } = null!;

    [Required]
    public string Dimensions { get; set; } = null!; // comma-separated key-value pairs (height: 0, weight: 0)

    // Dependents
    public Resource Resource { get; set; } = null!;
}