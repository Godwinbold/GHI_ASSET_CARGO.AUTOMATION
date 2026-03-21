namespace GHI_ASSET_CARGO.Core.Dtos.Shipment
{
    public class PagedResultDto<T> where T : class
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
