namespace GHI_ASSET_CARGO.Core.Dtos.Auth
{
    public class ExecutiveRegisterDTO
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string? MiddleName { get; set; }
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string? IdNumber { get; set; }
        public string? AirlineId { get; set; }
        public string Password { get; set; } = default!;
    }
}
