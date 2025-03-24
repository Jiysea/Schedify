using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Schedify.Models;

public class ResourceEquipment
{
    [Key]
    public Guid Id { get; set; } = new Guid();
    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Value must be at least 1.")]
    public required int Quantity { get; set; }

    [DefaultValue("No Brand")]
    public string? Brand { get; set; }
    
    [DefaultValue("No Specifications")]
    public string? Specifications { get; set; } // comma-separated key-value pairs

    [Column(TypeName = "date")]
    public DateTime Warranty { get; set; }
}