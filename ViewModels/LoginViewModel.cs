



using System.ComponentModel.DataAnnotations;

namespace Schedify.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "This field is required.")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", 
    ErrorMessage = "Value is not a valid email address.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(50, MinimumLength = 10, ErrorMessage = "Password should be at least 10 characters.")]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}
