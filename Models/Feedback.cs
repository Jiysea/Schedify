using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Schedify.Models;

public class Feedback
{
    [Key]
    public Guid Id { get; set; } = new Guid();

    public Guid UserId { get; set; }

    public Guid EventId { get; set; }

    [Required]
    public required int Rating { get; set; }

    [StringLength(500)]
    public string? Comments { get; set; }

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Dependents
    public User User { get; set; } = null!;
    public Event Event { get; set; } = null!;
}
