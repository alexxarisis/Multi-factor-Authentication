using System.ComponentModel.DataAnnotations;

namespace Client.Shared.Models.DTO
{
    public class OTPVerification
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter valid integer Number")]
        [StringLength(6, ErrorMessage = "Please type 6-digit password correctly", MinimumLength = 6)]
        public string Password { get; set; }

    }
}