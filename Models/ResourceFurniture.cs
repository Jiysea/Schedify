using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Schedify.Models;

public class ResourceFurniture
{
    [Key]
    public Guid Id { get; set; } = new Guid();
    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Value must be at least 1.")]
    public required int Quantity { get; set; }

    public FurnitureMaterial Material { get; set; }
    public string? OtherMaterial { get; set; }
    public string? Dimensions { get; set; }

    [Column(TypeName = "date")]
    public DateTime Warranty { get; set; } // comma-separated key-value pairs
}