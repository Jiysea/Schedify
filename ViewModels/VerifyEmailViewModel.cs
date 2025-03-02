
using System.ComponentModel.DataAnnotations;

namespace Schedify.ViewModels;

public class VerifyEmailViewModel
{
    [Required(ErrorMessage = "This field is required.")]
    [EmailAddress]
    public required string Email { get; set; }

}