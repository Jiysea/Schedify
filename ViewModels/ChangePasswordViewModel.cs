using System.ComponentModel.DataAnnotations;

namespace Schedify.ViewModels;

public class ChangePasswordViewModel
{

    [Required(ErrorMessage = "This field is required.")]
    [EmailAddress]
    public required string Email { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(50, MinimumLength = 12, ErrorMessage = "The {0} must be at {2} and at max {1} characters")]
    [DataType(DataType.Password)]
    [Display(Name = "Old Password")]
    public required string OldPassword { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(50, MinimumLength = 12, ErrorMessage = "The {0} must be at {2} and at max {1} characters")]
    [DataType(DataType.Password)]
    [Compare("ConfirmNewPassword", ErrorMessage = "Password does not match.")]
    [Display(Name = "New Password")]
    public required string NewPassword { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(50, MinimumLength = 12)]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    public required string ConfirmNewPassword { get; set; }
}