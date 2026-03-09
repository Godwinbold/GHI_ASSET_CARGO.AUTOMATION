namespace GHI_ASSET_CARGO.Core.Dtos.Shipment
{
    public class ShipmentNoteDto
    {
        public string Id { get; set; } = default!;
        public string Content { get; set; } = default!;
        public DateTimeOffset CreatedDate { get; set; }
    }
}
