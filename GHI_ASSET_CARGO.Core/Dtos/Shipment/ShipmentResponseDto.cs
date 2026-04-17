using GHI_ASSET_CARGO.Domain.Enums;

namespace GHI_ASSET_CARGO.Core.Dtos.Shipment
{
    public class ShipmentResponseDto
    {
        public string Id { get; set; } = default!;
        public string AirlineId { get; set; } = default!;
        public string AirwayBillNumber { get; set; } = default!;
        public ShipmentStatus Status { get; set; }
        public string StatusDisplay { get; set; } = default!;
        public DateTimeOffset ShipmentDate { get; set; }
        public bool HasFinancial { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
        public List<ShipmentNoteDto> Notes { get; set; } = new();
    }
}
