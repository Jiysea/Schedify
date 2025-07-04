



using System.ComponentModel.DataAnnotations;
using Schedify.Attributes;

namespace Schedify.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "This field is required.")]
    [Display(Name = "First Name")]
    [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Value should have no numbers and symbols.")]
    public string? FirstName { get; set; }

    [Display(Name = "Middle Name")]
    [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Value should have no numbers and symbols.")]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [Display(Name = "Last Name")]
    [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Value should have no numbers and symbols.")]
    public string? LastName { get; set; }

    [Display(Name = "Extension Name")]
    [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Value should have no numbers and symbols.")]
    public string? ExtensionName { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    public DateTime? Birthdate { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [EmailAddress]
    public required string Email { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(50, MinimumLength = 10, ErrorMessage = "Password should be at least 10 characters.")]
    [DataType(DataType.Password)]
    [Compare("ConfirmPassword", ErrorMessage = "Password does not match.")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(50, MinimumLength = 10, ErrorMessage = "Password should be at least 10 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string? ConfirmPassword { get; set; }

    [Range(1, 2, ErrorMessage = "Invalid role.")]
    public int Role { get; set; } = 1;

    
    public IFormFile? ImageFile { get; set; }
}