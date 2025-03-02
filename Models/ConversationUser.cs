using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Schedify.Models;

public class ConversationUser
{
    [Key]
    public Guid Id { get; set; } = new Guid();
    public Guid UserId { get; set; }

    public Guid ConversationId { get; set; }

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Dependents
    public User User { get; set; } = null!;
    public Conversation Conversation { get; set; } = null!;
}
