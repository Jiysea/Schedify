using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Schedify.Models;

public class Resource
{
    [Key]
    public Guid Id { get; set; } = new Guid();

    public Guid UserId { get; set; }

    [Required]
    [StringLength(250)]
    public required string ProviderName { get; set; }

    [Required]
    public required string ProviderPhoneNumber { get; set; }

    [Required]
    [EmailAddress]
    public required string ProviderEmail { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    [StringLength(300)]
    [DefaultValue("No description")]
    public required string Description { get; set; }

    public ResourceType ResourceType { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    [Column(TypeName = "decimal(18, 2)")]
    public required decimal Cost { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public required string CostType { get; set; }

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Dependents
    public ICollection<EventResource> EventResources { get; } = new List<EventResource>();
    public Image? Image { get; set; }
    public ResourceVenue ResourceVenue { get; set; } = null!;
    public ResourceEquipment ResourceEquipment { get; set; } = null!;
    public ResourceFurniture ResourceFurniture { get; set; } = null!;
    public ResourceCatering ResourceCatering { get; set; } = null!;
    public ResourcePersonnel ResourcePersonnel { get; set; } = null!;
}
