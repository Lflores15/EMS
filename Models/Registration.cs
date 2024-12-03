using System.ComponentModel.DataAnnotations;

namespace EMS.Models
{
    public class Registration
    {
        [Required]
        [StringLength(256, ErrorMessage = "Username must be less than 256 characters.")]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

    }
}
