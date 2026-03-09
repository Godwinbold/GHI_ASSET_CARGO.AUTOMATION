using System.ComponentModel.DataAnnotations;

namespace GHI_ASSET_CARGO.Core.Dtos.Auth
{
    public class ChangePasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Password and confirm password do not match")]
        public string ConfirmPassword { get; set; }
    }
}
