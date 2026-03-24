using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Financial;

namespace GHI_ASSET_CARGO.Core.Abstractions
{
    public interface IFinancialService
    {
        Task<Result<FinancialResponseDto>> GetFinancialByShipmentIdAsync(Guid shipmentId, string airlineId);
        Task<Result<FinancialResponseDto>> GetFinancialByIdAsync(Guid financialId, string airlineId);
        Task<Result<FinancialResponseDto>> CreateFinancialAsync(Guid shipmentId, string airlineId, CreateFinancialRequestDto dto);
        Task<Result<FinancialResponseDto>> UpdateFinancialAsync(Guid financialId, string airlineId, UpdateFinancialRequestDto dto);
        Task<Result> DeleteFinancialAsync(Guid financialId, string airlineId);
    }
}