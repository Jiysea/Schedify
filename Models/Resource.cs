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
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    [StringLength(300)]
    [DefaultValue("No description")]
    public required string Description { get; set; }

    public ResourceType Type { get; set; }

    [Required]
    [DataType(DataType.Currency)]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
    [Column(TypeName = "decimal(18, 2)")]
    public required decimal Cost { get; set; }

    [Required]
    public required string CostType { get; set; }

    [Required]
    public required int Quantity { get; set; }

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Dependents
    public User User { get; set; } = null!;
    public ICollection<EventResource> EventResources { get; } = new List<EventResource>();
    public ICollection<Image> Images { get; } = new List<Image>();
    public ResourceVenue ResourceVenue { get; set; } = null!;
    public ResourcePersonnel ResourcePersonnel { get; set; } = null!;
    public ResourceCatering ResourceCatering { get; set; } = null!;
    public ResourceEquipment ResourceEquipment { get; set; } = null!;
    public ResourceFurniture ResourceFurniture { get; set; } = null!;
}
