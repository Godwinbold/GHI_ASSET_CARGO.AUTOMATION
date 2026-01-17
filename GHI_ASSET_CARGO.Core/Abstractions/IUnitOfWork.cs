using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHI_ASSET_CARGO.Core.Abstractions
{
    public interface IUnitOfWork
    {
        public Task<int> SaveChangesAsync();
    }
}
