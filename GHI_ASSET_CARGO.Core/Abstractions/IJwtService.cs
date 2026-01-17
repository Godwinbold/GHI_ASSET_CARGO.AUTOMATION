using GHI_ASSET_CARGO.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHI_ASSET_CARGO.Core.Abstractions
{
    public interface IJwtService
    {
        public string GenerateToken(AppUser user, IList<string> roles);
    }
}
