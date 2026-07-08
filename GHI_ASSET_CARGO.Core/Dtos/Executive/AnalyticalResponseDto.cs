using System.Collections.Generic;

namespace GHI_ASSET_CARGO.Core.Dtos.Executive
{
    public class AnalyticalResponseDto
    {
        public MetricWithComparisonDto TotalShipments { get; set; } = new MetricWithComparisonDto();
        public MetricWithComparisonDto TotalRevenue { get; set; } = new MetricWithComparisonDto();
        public MetricWithComparisonDto AverageWeight { get; set; } = new MetricWithComparisonDto();
        public List<MonthlyDataDto> MonthlyData { get; set; } = new List<MonthlyDataDto>();
        public List<CargoUnitSummaryDto> CargoUnits { get; set; } = new List<CargoUnitSummaryDto>();
    }
}
