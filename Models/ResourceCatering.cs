using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Schedify.Models;

public class ResourceCatering
{
    [Key]
    public Guid Id { get; set; } = new Guid();
    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Value must be at least 1.")]
    public required int GuestCapacity { get; set; }
    
    [Required]
    public string MenuItems { get; set; } = null!;
}