using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHI_ASSET_CARGO.Domain.Entities
{
    public class Financial : BaseEntity
    {
        public Guid AirlineId { get; set; }
        public Airline Airline { get; set; } = null!;

        public Guid ShipmentId { get; set; }
        public Shipment Shipment { get; set; } = null!;

        public string MAWB { get; set; } = null!;

        public DateTime? DateOfIssue { get; set; }

        public string? AgentsOrClients { get; set; }

        public string? Product { get; set; }

        public string? Routing { get; set; }

        public string? FlightNo { get; set; }

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
    }

}
