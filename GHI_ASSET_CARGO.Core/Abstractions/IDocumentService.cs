using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Document;
using GHI_ASSET_CARGO.Core.Dtos.Shipment;
using GHI_ASSET_CARGO.Domain.Enums;

namespace GHI_ASSET_CARGO.Core.Abstractions
{
    public interface IDocumentService
    {
        Task<Result<PagedResultDto<ShipmentDocumentResponseDto>>> GetDocumentsByShipmentAsync(Guid shipmentId, string airlineId, int page = 1, int pageSize = 10);
        Task<Result<ShipmentDocumentResponseDto>> GetDocumentByIdAsync(Guid documentId, string airlineId);
        Task<Result<ShipmentDocumentResponseDto>> UploadDocumentAsync(Guid shipmentId, string airlineId, CreateShipmentDocumentRequestDto dto);
        Task<Result<ShipmentDocumentResponseDto>> UpdateDocumentAsync(Guid documentId, string airlineId, UpdateShipmentDocumentRequestDto dto);
        Task<Result> DeleteDocumentAsync(Guid documentId, string airlineId);
    }
}