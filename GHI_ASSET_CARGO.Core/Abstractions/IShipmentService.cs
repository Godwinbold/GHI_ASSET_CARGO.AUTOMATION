using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Shipment;
using GHI_ASSET_CARGO.Domain.Enums;

namespace GHI_ASSET_CARGO.Core.Abstractions
{
    public interface IShipmentService
    {
        Task<Result<PagedResultDto<ShipmentResponseDto>>> GetShipmentsForAirlineAsync(string airlineId, int page, int pageSize, string? awbSearch = null);
        Task<Result<ShipmentResponseDto>> GetShipmentByIdAsync(string shipmentId, string airlineId);
        Task<Result<ShipmentResponseDto>> GetShipmentByAwbAsync(string airwayBillNumber, string airlineId);
        Task<Result<ShipmentResponseDto>> CreateShipmentAsync(string airlineId, CreateShipmentRequestDto dto);
        Task<Result> AddNoteAsync(string shipmentId, string airlineId, AddNoteRequestDto dto);
        Task<Result> DeleteShipmentAsync(string shipmentId, string airlineId);
        Task<Result<PagedResultDto<ShipmentResponseDto>>> GetShipmentsByStatusAsync(string airlineId, ShipmentStatus status, int page = 1, int pageSize = 10);
    }
}
