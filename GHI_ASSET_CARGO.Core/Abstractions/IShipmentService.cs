using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Shipment;

namespace GHI_ASSET_CARGO.Core.Abstractions
{
    public interface IShipmentService
    {
        Task<Result<PagedResultDto<ShipmentResponseDto>>> GetShipmentsForAirlineAsync(string airlineId, int page, int pageSize, string? awbSearch = null);
        Task<Result<ShipmentResponseDto>> GetShipmentByIdAsync(string shipmentId, string airlineId);
        Task<Result<ShipmentResponseDto>> GetShipmentByAwbAsync(string airwayBillNumber, string airlineId);
        Task<Result<ShipmentResponseDto>> CreateShipmentAsync(string airlineId, CreateShipmentRequestDto dto);
        Task<Result> AddNoteAsync(string shipmentId, string airlineId, AddNoteRequestDto dto);
    }
}
