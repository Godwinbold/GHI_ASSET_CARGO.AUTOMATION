using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHI_ASSET_CARGO.Domain.Entities
{
    public class Airline : BaseEntity
    {
        public string AirlineName { get; set; }
        public ICollection<Shipment>? Shipments { get; set; }
        public ICollection<Financial>? Financials { get; set; }
    }
}
