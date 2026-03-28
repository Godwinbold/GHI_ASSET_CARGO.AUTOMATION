using System;

namespace GHI_ASSET_CARGO.Core.Dtos.Executive
{
    public class CargoUnitSummaryDto
    {
        public Guid AirlineId { get; set; }
        public string AirlineName { get; set; } = default!;
        public int TotalShipments { get; set; }
        public decimal TotalWeightKg { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
