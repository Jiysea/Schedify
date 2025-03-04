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
    public string Brand { get; set; } = null!;

    [Required]
    public string Specifications { get; set; } = null!; // comma-separated key-value pairs

    // Dependents
    public Resource Resource { get; set; } = null!;

}