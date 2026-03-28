using System.Collections.Generic;

namespace GHI_ASSET_CARGO.Core.Dtos.Executive
{
    public class ExecutiveDashboardResponseDto
    {
        public List<CargoUnitSummaryDto> CargoUnits { get; set; } = new List<CargoUnitSummaryDto>();
        public ExecutiveMetricsDto FilteredResult { get; set; } = new ExecutiveMetricsDto();
    }
}
