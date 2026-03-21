using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHI_ASSET_CARGO.Domain.Entities
{
    public class Financial : BaseEntity
    {
        public Guid AirlineId { get; set; }
        public Airline Airline { get; set; }
    }
}
