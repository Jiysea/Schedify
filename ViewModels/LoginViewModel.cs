using System.ComponentModel.DataAnnotations;

namespace ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public required string Email { get; set; }


        [Required(ErrorMessage = "Password is required")]
        [StringLength(50, MinimumLength = 12)]
        public required string Password { get; set; }
    }
}