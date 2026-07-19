using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Financial;
using System.Collections.Generic;

namespace GHI_ASSET_CARGO.Core.Abstractions
{
    public interface IFinancialService
    {
        Task<Result<FinancialResponseDto>> GetFinancialByShipmentIdAsync(Guid shipmentId, string airlineId);
        Task<Result<FinancialResponseDto>> GetFinancialByIdAsync(Guid financialId, string airlineId);
        Task<Result<List<FinancialResponseDto>>> GetFinancialsByAirlineAsync(string airlineId);
        Task<Result<FinancialResponseDto>> CreateFinancialAsync(Guid shipmentId, string airlineId, CreateFinancialRequestDto dto, string? userId = null, string? userEmail = null, string? userName = null, string? ipAddress = null);
        Task<Result<FinancialResponseDto>> UpdateFinancialAsync(Guid financialId, string airlineId, UpdateFinancialRequestDto dto, string? userId = null, string? userEmail = null, string? userName = null, string? ipAddress = null);
        Task<Result> DeleteFinancialAsync(Guid financialId, string airlineId, string? userId = null, string? userEmail = null, string? userName = null, string? ipAddress = null);
    }
}