namespace GHI_ASSET_CARGO.Core.Dtos.Financial
{
    public class FinancialResponseDto
    {
        public Guid Id { get; set; }
        public Guid ShipmentId { get; set; }
        public string MAWB { get; set; } = default!;
        public DateTime DateOfIssue { get; set; }
        public string? AgentsOrClients { get; set; } = default!;
        public string? Product { get; set; } = default!;
        public string? Routing { get; set; } = default!;
        public string? FlightNo { get; set; } = default!;
        public int? Pieces { get; set; }
        public decimal? ChargeableWeightKg { get; set; }
        public decimal? GrossWeightKg { get; set; }
        public decimal? SpotRate { get; set; }
        public decimal? PublishedRates { get; set; }
        public decimal? ROE { get; set; }
        public decimal? FreightAmountNGN { get; set; }
        public decimal? NCAACharges5Percent { get; set; }
        public decimal? TotalChargeNGN { get; set; }
        public decimal? ChargesCollect { get; set; }
        public decimal? FuelSurcharge { get; set; }
        public decimal? SECSurcharge { get; set; }
        public decimal? HandlingSurcharge { get; set; }
        public decimal? SurchargeDueAgent { get; set; }
        public decimal? AWBFee { get; set; }
        public decimal? GSACommissionNGN { get; set; }
        public decimal? VATOnCommission { get; set; }
        public decimal? AmtDueAirline { get; set; }
        public decimal? DueAPGInc { get; set; }
        public decimal? DueSLC { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }
    }
}