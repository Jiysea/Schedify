using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Schedify.Models;

public class User : IdentityUser<Guid>
{

    [Required]
    [StringLength(50)]
    public required string FirstName { get; set; }

    [StringLength(35)]
    public string? MiddleName { get; set; }

    [Required]
    [StringLength(35)]
    public required string LastName { get; set; }

    [StringLength(10)]
    public string? ExtensionName { get; set; }

    [Required]
    [Column(TypeName = "datetime2")]
    public required DateTime Birthdate { get; set; }

    public UserRoles Role { get; set; }

    [Required]
    [StringLength(11, MinimumLength = 11)]
    public required string PhoneNum { get; set; }

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Dependents
    public ICollection<Event> Events { get; } = new List<Event>();
    public ICollection<ActivityLog> ActivityLogs { get; } = new List<ActivityLog>();
    public ICollection<BillingAddress> BillingAddresses { get; } = new List<BillingAddress>();
    public ICollection<ConversationUser> ConversationUsers { get; } = new List<ConversationUser>();
    public ICollection<Feedback> Feedbacks { get; } = new List<Feedback>();
    public ICollection<Message> Messages { get; } = new List<Message>();
}
