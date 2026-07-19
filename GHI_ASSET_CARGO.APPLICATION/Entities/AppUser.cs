
using Microsoft.AspNetCore.Identity;

namespace GHI_ASSET_CARGO.Domain.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? MiddleName { get; set; }
        public override string? PhoneNumber { get; set; }
        public string? IdNumber { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
        public string? AirlineId { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}