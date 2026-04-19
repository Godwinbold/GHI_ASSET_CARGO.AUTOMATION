using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHI_ASSET_CARGO.Core.Dtos.Auth
{
    public class AdminRegisterDTO
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
