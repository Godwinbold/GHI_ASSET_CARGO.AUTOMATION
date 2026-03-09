using GHI_ASSET_CARGO.Domain.Enums;

namespace GHI_ASSET_CARGO.Core.Dtos.Shipment
{
    public class CreateShipmentRequestDto
    {
        public string AirwayBillNumber { get; set; } = default!;
        public ShipmentStatus Status { get; set; }
        public DateTimeOffset ShipmentDate { get; set; }
    }
}
