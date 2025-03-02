using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Schedify.Models;

public class Message
{
    [Key]
    public Guid Id { get; set; } = new Guid();

    public Guid UserId { get; set; }

    public Guid ConversationId { get; set; }

    [Required]
    [StringLength(100)]
    public required string Content { get; set; }

    [Required]
    public MessageStatus Status { get; set; }

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Dependents
    public User User { get; set; } = null!;
    public Conversation Conversation { get; set; } = null!;
}
