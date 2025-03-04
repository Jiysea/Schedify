using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Schedify.Models;

public class ResourcePersonnel
{
    [Key]
    public Guid Id { get; set; } = new Guid();

    public Guid ResourceId { get; set; }

    [Required]
    public string Position { get; set; } = null!;

    [Required]
    [StringLength(30)]
    public string Shift { get; set; } = null!;

    [Required]
    [StringLength(30)]
    [DefaultValue("None")]
    public string Experience { get; set; } = null!;

    // Dependents
    public Resource Resource { get; set; } = null!;
}