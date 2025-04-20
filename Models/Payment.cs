using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Schedify.Models;

public class Payment
{
    [Key]
    public Guid Id { get; set; } = new Guid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid EventBookingId { get; set; }
    public EventBooking EventBooking { get; set; } = null!;

    [StringLength(255)]
    public string? SessionId { get; set; }

    [DataType(DataType.Currency)]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [StringLength(20)]
    public string? PaymentMethod { get; set; }

    [StringLength(20)]
    public string? CardBrand { get; set; }

    [StringLength(4)]
    public string? PANLastDigits { get; set; }

    [StringLength(20)]
    public string? EventShortName { get; set; }

    [StringLength(20)]
    public string? Status { get; set; } = "Pending";

    [Column(TypeName = "datetime2")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}