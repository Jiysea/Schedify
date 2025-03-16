using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Schedify.Models;

public class ResourcePersonnel
{
    [Key]
    public Guid Id { get; set; } = new Guid();
    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;

    [Required]
    public string Position { get; set; } = null!;

    [Required]
    [Column(TypeName = "time")]
    public TimeSpan ShiftStart { get; set; }

    [Required]
    [Column(TypeName = "time")]
    public TimeSpan ShiftEnd { get; set; }

    public string? Experience { get; set; }
}