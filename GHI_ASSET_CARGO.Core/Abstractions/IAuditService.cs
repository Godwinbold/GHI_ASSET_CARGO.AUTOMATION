using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.AuditLog;
using GHI_ASSET_CARGO.Core.Dtos.Shipment;

namespace GHI_ASSET_CARGO.Core.Abstractions
{
    public interface IAuditService
    {
        /// <summary>
        /// Logs an audit entry for write operations
        /// </summary>
        /// <param name="userId">The user performing the action</param>
        /// <param name="userName">The user's full name</param>
        /// <param name="userEmail">The user's email</param>
        /// <param name="action">Action type: Create, Update, Delete</param>
        /// <param name="entityName">Name of the entity (e.g., "Shipment", "Airline")</param>
        /// <param name="entityId">ID of the affected entity</param>
        /// <param name="changes">Human-readable description of changes</param>
        /// <param name="oldValues">Previous values in JSON format</param>
        /// <param name="newValues">New values in JSON format</param>
        /// <param name="ipAddress">IP address of the request</param>
        Task LogAuditAsync(
            Guid userId,
            string userName,
            string userEmail,
            string action,
            string entityName,
            Guid entityId,
            string changes,
            string oldValues = "",
            string newValues = "",
            string ipAddress = ""
        );

        /// <summary>
        /// Retrieves audit logs with optional filtering
        /// </summary>
        Task<Result<PagedResultDto<AuditLogResponseDto>>> GetAuditLogsAsync(AuditLogFilterDto filter);
    }
}
