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
        public bool IsDeleted { get; set; } = false;
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
        public ICollection<Financial> Financials { get; set; } = new List<Financial>();
    }
}
