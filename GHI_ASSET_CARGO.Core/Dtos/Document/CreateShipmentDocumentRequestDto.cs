namespace GHI_ASSET_CARGO.Core.Dtos.Document
{
    public class CreateShipmentDocumentRequestDto
    {
        public string FileName { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public long FileSizeBytes { get; set; }
        public string StoragePath { get; set; } = default!;
        public int UploadedByUserId { get; set; }
    }
}