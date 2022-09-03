using System.ComponentModel.DataAnnotations;

namespace Client.Shared.Models.DTO
{
    public class LoginUser
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(32, ErrorMessage = "Name is too long.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(32, ErrorMessage = "Must be between 4 and 32 characters", MinimumLength = 4)]
        public string Password { get; set; }
    }
}