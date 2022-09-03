using System.ComponentModel.DataAnnotations;

namespace Client.Shared.Models.DTO
{
    public class RegistrationUser
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(16, ErrorMessage = "Name is too long.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(32, ErrorMessage = "Must be between 4 and 32 characters", MinimumLength = 4)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Password confirmation is required.")]
        [StringLength(32, ErrorMessage = "Must be between 4 and 32 characters", MinimumLength = 4)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}