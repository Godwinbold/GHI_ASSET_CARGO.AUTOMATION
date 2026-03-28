using System;

namespace GHI_ASSET_CARGO.Core.Dtos.Executive
{
    public class ExecutiveDashboardRequestDto
    {
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public Guid? AirlineId { get; set; }
    }
}
