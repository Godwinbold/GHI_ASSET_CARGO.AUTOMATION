

namespace GHI_ASSET_CARGO.Domain.Entities
{
    public class ShipmentDocument : BaseEntity
    {
        public string AirwayBillNumber { get; set; }
        public DateTime ShipmentDate { get; set; }
        public string FileName { get; set; }
        public string? ContentType { get; set; }
        public long? FileSizeBytes { get; set; }
        public string StoragePath { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public Guid UploadedByUserId { get; set; }
        public Guid ShipmentId { get; set; }
        public Shipment Shipment { get; set; }
    }
}
