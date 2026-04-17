using System.ComponentModel.DataAnnotations;

namespace GHI_ASSET_CARGO.Core.Dtos.Auth
{
    public class AcceptInviteDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Token { get; set; } = default!;

        [Required]
        public string FirstName { get; set; } = default!;

        [Required]
        public string LastName { get; set; } = default!;

        public string? MiddleName { get; set; }

        [Required]
        public string PhoneNumber { get; set; } = default!;

        public string? IdNumber { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
        public string ConfirmPassword { get; set; } = default!;

        public string? AirlineId { get; set; }
        public string? Role { get; set; }
    }
}
