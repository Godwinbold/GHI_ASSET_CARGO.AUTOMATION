namespace GHI_ASSET_CARGO.Domain.Entities
{
    public class ShipmentNote : BaseEntity
    {
        public Guid ShipmentId { get; set; } = default!;
        public string Content { get; set; } = default!;
        public Shipment Shipment { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
