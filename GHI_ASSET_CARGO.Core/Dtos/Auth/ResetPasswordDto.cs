using System.ComponentModel.DataAnnotations;

namespace GHI_ASSET_CARGO.Core.Dtos.Auth
{
    public class ResetPasswordDto
    {
        [Required][EmailAddress] public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required] public string Token { get; set; }
    }


}
