



using System.ComponentModel.DataAnnotations;

namespace Schedify.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "This field is required.")]
    [EmailAddress]
    public required string Email { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(50, MinimumLength = 12)]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}
