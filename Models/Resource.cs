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
    public required string ProviderEmail { get; set; }

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

    [Required(ErrorMessage = "This field is required.")]
    public required string CostType { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Value must be at least 1.")]
    public required int Quantity { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Value must be at least 1.")]
    public int Capacity { get; set; }

    public string? Size { get; set; }

    [StringLength(150)]
    public string? AddressLine1 { get; set; }

    [StringLength(150)]
    public string? AddressLine2 { get; set; }

    [StringLength(50)]
    public string? CityMunicipality { get; set; }

    [StringLength(50)]
    public string? Province { get; set; }

    [DefaultValue("No Brand")]
    public string? Brand { get; set; }

    public string? Specifications { get; set; } // comma-separated key-value pairs

    public string? Material { get; set; }

    public string? Dimensions { get; set; }

    public string? MenuItems { get; set; }

    public string? PriceItems { get; set; }

    public string? Position { get; set; }

    [StringLength(30)]
    public string? Shift { get; set; }

    [StringLength(30)]
    [DefaultValue("None")]
    public string? Experience { get; set; }

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Dependents
    public ICollection<EventResource> EventResources { get; } = new List<EventResource>();
    public ICollection<Image> Images { get; } = new List<Image>();
}
