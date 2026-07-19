namespace GHI_ASSET_CARGO.Core.Dtos.AuditLog
{
    public class AuditLogResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = default!;
        public string UserEmail { get; set; } = default!;
        public string Action { get; set; } = default!;
        public string EntityName { get; set; } = default!;
        public Guid EntityId { get; set; }
        public string Changes { get; set; } = default!;
        public DateTimeOffset Timestamp { get; set; }
        public string IpAddress { get; set; } = default!;
    }

    public class AuditLogFilterDto
    {
        public Guid? UserId { get; set; }
        public string? Action { get; set; } // Create, Update, Delete
        public string? EntityName { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
