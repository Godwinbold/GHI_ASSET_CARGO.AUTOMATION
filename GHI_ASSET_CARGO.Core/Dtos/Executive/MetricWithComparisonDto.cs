namespace GHI_ASSET_CARGO.Core.Dtos.Executive
{
    public class MetricWithComparisonDto
    {
        public decimal CurrentValue { get; set; }
        public decimal PreviousValue { get; set; }
        public decimal GrowthValue { get; set; }
        public decimal GrowthPercentage { get; set; }
    }
}
