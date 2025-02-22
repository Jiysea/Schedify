using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Schedify.Models;

public class Event
{
    [Key]
    public Guid Id { get; set; } = new Guid();

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

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

    [Required]
    public required string Status { get; set; }

    [Required]
    [DataType(DataType.Currency)]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
    [Column(TypeName = "decimal(18, 2)")]
    public required decimal EntryFee { get; set; }

    [Required]
    [StringLength(150)]
    public required string AddressLine1 { get; set; }

    [StringLength(150)]
    public string? AddressLine2 { get; set; }

    [Required]
    [StringLength(50)]
    public required string CityMunicipality { get; set; }

    [Required]
    [StringLength(50)]
    public required string Province { get; set; }

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Dependents
    public ICollection<EventBooking> EventBookings { get; } = new List<EventBooking>();
    public ICollection<Conversation> Conversations { get; } = new List<Conversation>();
    public ICollection<Feedback> Feedbacks { get; } = new List<Feedback>();
    public ICollection<Resource> Resources { get; } = new List<Resource>();


}