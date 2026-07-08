namespace GHI_ASSET_CARGO.Core.Dtos.Document
{
    public class UpdateShipmentDocumentRequestDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = default!;
        public string? ContentType { get; set; }
        public long? FileSizeBytes { get; set; }
        public string? StoragePath { get; set; }
    }
}