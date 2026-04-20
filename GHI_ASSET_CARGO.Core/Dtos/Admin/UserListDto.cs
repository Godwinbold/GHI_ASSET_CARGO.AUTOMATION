namespace GHI_ASSET_CARGO.Core.Dtos.Admin
{
    public class UserListDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string? MiddleName { get; set; }
        public string Email { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public string? AirlineId { get; set; }
        public string? IdNumber { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
        public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
    }
}
