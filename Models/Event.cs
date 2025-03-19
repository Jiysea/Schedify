using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Schedify.Models;

public class Event
{
    [Key]
    public Guid Id { get; set; } = new Guid();

    public Guid UserId { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    [StringLength(500)]
    public required string Description { get; set; }

    [Required]
    [Column(TypeName = "datetime2")]
    [DataType(DataType.DateTime)]
    public required DateTime StartAt { get; set; }

    [Required]
    [Column(TypeName = "datetime2")]
    [DataType(DataType.DateTime)]
    public required DateTime EndAt { get; set; }
    public EventStatus Status { get; set; }

    [Required]
    [DataType(DataType.Currency)]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
    [Column(TypeName = "decimal(18, 2)")]
    public required decimal EntryFee { get; set; }

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Dependents
    public User User { get; set; } = null!;
    public ICollection<Resource> Resources { get; } = new List<Resource>();
    public ICollection<EventBooking> EventBookings { get; } = new List<EventBooking>();
    public Conversation Conversation { get; set; } = null!;
    public ICollection<Feedback> Feedbacks { get; } = new List<Feedback>();
}