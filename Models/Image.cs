using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Schedify.Models;

public class Image
{
    [Key]
    public Guid Id { get; set; } = new Guid();
    public Guid? UserId { get; set; }
    public Guid? ResourceId { get; set; }
    public string? ImageFileName { get; set; }

    // Dependents
    public User? User { get; set; }
    public Resource? Resource { get; set; }
}
