using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Schedify.Models;

public class ResourceEquipment
{
    [Key]
    public Guid Id { get; set; } = new Guid();

    public Guid ResourceId { get; set; }

    [Required]
    [DefaultValue("No Brand")]
    public required string Brand { get; set; }

    [Required]
    public required string Specifications { get; set; } // comma-separated key-value pairs

    // Dependents
    public Resource Resource { get; set; } = null!;

}