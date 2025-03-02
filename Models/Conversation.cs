using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Schedify.Models;

public class Conversation
{
    [Key]
    public Guid Id { get; set; } = new Guid();

    public Guid EventId { get; set; }

    [Required]
    [StringLength(100)]
    public required string Title { get; set; }

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Dependents
    public Event Event { get; set; } = null!;
    public ICollection<ConversationUser> ConversationUsers { get; } = new List<ConversationUser>();
    public ICollection<Message> Messages { get; } = new List<Message>();
}
