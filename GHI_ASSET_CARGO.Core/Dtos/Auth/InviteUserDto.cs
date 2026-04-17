using System.ComponentModel.DataAnnotations;

namespace GHI_ASSET_CARGO.Core.Dtos.Auth
{
    public class InviteUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string AirlineId { get; set; } = default!;

        [Required]
        public string Role { get; set; } = default!;
    }
}
