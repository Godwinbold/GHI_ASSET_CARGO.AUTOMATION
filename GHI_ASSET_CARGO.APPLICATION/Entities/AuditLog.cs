namespace GHI_ASSET_CARGO.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = default!;
        public string UserEmail { get; set; } = default!;
        public string Action { get; set; } = default!; 
        public string EntityName { get; set; } = default!; 
        public Guid EntityId { get; set; }
        public string OldValues { get; set; } = string.Empty; 
        public string NewValues { get; set; } = string.Empty; 
        public string Changes { get; set; } = string.Empty; 
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public string IpAddress { get; set; } = string.Empty;
    }
}
