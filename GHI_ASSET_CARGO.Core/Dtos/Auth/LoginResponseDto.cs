namespace GHI_ASSET_CARGO.Core.Dtos.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = default!;
        public DateTimeOffset ExpiresAt { get; set; }
        public string UserId { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string? MiddleName { get; set; }
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string AirlineId { get; set; } = default!;
        public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
    }
}
