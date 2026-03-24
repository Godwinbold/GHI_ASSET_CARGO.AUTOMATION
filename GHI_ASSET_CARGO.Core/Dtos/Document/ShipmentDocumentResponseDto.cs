namespace GHI_ASSET_CARGO.Core.Dtos.Document
{
    public class ShipmentDocumentResponseDto
    {
        public Guid Id { get; set; }
        public Guid ShipmentId { get; set; }
        public string AirwayBillNumber { get; set; } = default!;
        public DateTime ShipmentDate { get; set; }
        public string FileName { get; set; } = default!;
        public string? ContentType { get; set; }
        public long? FileSizeBytes { get; set; }
        public string StoragePath { get; set; } = default!;
        public DateTime UploadedAt { get; set; }
        public int UploadedByUserId { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
    }
}