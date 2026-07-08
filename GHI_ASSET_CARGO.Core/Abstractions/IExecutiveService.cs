using GHI_ASSET_CARGO.Core.Dtos.Executive;
using GHI_ASSET_CARGO.Core.Dtos;

namespace GHI_ASSET_CARGO.Core.Abstractions
{
    public interface IExecutiveService
    {
        Task<Result<ExecutiveDashboardResponseDto>> GetExecutiveDashboardAsync(ExecutiveDashboardRequestDto request);
        Task<Result<AnalyticalResponseDto>> GetAnalyticalAsync(ExecutiveDashboardRequestDto request);
    }
}
