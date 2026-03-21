using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHI_ASSET_CARGO.Domain.Entities
{
    public class ShipmentDocument : BaseEntity
    {
        public Guid ShipmentId { get; set; }
        public Shipment Shipment { get; set; }
        public string? FileName { get; set; } = default!;
        public string? FileType { get; set; } = default!;
        public string FileId { get; set; } = default!;

    }
}
