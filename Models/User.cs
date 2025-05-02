using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Schedify.Models;

public class User : IdentityUser<Guid>
{

    [Required]
    [StringLength(50)]
    public string? FirstName { get; set; }

    [StringLength(35)]
    public string? MiddleName { get; set; }

    [Required]
    [StringLength(35)]
    public string? LastName { get; set; }

    [StringLength(10)]
    public string? ExtensionName { get; set; }

    [Required]
    [Column(TypeName = "date")]
    public DateTime? Birthdate { get; set; }

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Dependents
    public Image? Image { get; set; }
    public ICollection<Event> Events { get; } = new List<Event>();
    public ICollection<Payment> Payments { get; } = new List<Payment>();
    public ICollection<ActivityLog> ActivityLogs { get; } = new List<ActivityLog>();
    public ICollection<ConversationUser> ConversationUsers { get; } = new List<ConversationUser>();
    public ICollection<Feedback> Feedbacks { get; } = new List<Feedback>();
    public ICollection<Message> Messages { get; } = new List<Message>();
}
