using GHI_ASSET_CARGO.Domain.Enums;

namespace GHI_ASSET_CARGO.Domain.Entities
{
    public class Shipment : BaseEntity
    {
        public Guid AirlineId { get; set; } = default!;
        public Airline Airline { get; set; }
        public string AirwayBillNumber { get; set; } = default!;
        public ShipmentStatus Status { get; set; }
        public DateTimeOffset ShipmentDate { get; set; }
        public bool HasFinancial { get; set; } = false;
        public Financial? Financial { get; set; }
        public ICollection<ShipmentNote> ShipmentNotes { get; set; } = new List<ShipmentNote>();
        public ICollection<ShipmentDocument> ShipmentDocuments { get; set; } = new List<ShipmentDocument>();
    }
}
