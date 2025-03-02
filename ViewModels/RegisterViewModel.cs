



using System.ComponentModel.DataAnnotations;

namespace Schedify.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "This field is required.")]
    [Display(Name = "First Name")]
    public required string FirstName { get; set; }

    [Display(Name = "Middle Name")]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [Display(Name = "Last Name")]
    public required string LastName { get; set; }

    [Display(Name = "Extension Name")]
    public string? ExtensionName { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [DataType(DataType.DateTime, ErrorMessage = "Invalid date format.")]
    public required DateTime Birthdate { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [DataType(DataType.PhoneNumber)]
    [Display(Name = "Phone Number")]
    public required string PhoneNumber { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [EmailAddress]
    public required string Email { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(50, MinimumLength = 12, ErrorMessage = "The {0} must be at {2} and at max {1} characters")]
    [DataType(DataType.Password)]
    [Compare("ConfirmPassword", ErrorMessage = "Password does not match.")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(50, MinimumLength = 12)]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public required string ConfirmPassword { get; set; }

    [Range(1, 2, ErrorMessage = "Invalid role.")]
    public int Role { get; set; } = 1;
}