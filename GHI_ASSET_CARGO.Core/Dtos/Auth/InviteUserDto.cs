using System.ComponentModel.DataAnnotations;

namespace GHI_ASSET_CARGO.Core.Dtos.Auth
{
    public class InviteUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        public string? AirlineId { get; set; }

        [Required]
        public string Role { get; set; } = default!;
    }
}
