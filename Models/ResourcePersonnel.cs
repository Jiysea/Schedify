using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Schedify.Models;

public class ResourcePersonnel
{
    [Key]
    public Guid Id { get; set; } = new Guid();

    public Guid ResourceId { get; set; }

    [Required]
    public required string Position { get; set; }

    [Required]
    [StringLength(30)]
    public required string Shift { get; set; }

    [Required]
    [StringLength(30)]
    [DefaultValue("None")]
    public required string Experience { get; set; }

    // Dependents
    public Resource Resource { get; set; } = null!;
}