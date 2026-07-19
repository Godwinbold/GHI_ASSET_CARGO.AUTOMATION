using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.AuditLog;
using GHI_ASSET_CARGO.Core.Dtos.Shipment;
using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHI_ASSET_CARGO.Core.Services
{
    public class AuditService : IAuditService
    {
        private readonly IRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public AuditService(IRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task LogAuditAsync(
            Guid userId,
            string userName,
            string userEmail,
            string action,
            string entityName,
            Guid entityId,
            string changes,
            string oldValues = "",
            string newValues = "",
            string ipAddress = "")
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    UserName = userName,
                    UserEmail = userEmail,
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    Changes = changes,
                    OldValues = oldValues,
                    NewValues = newValues,
                    IpAddress = ipAddress,
                    Timestamp = DateTimeOffset.UtcNow
                };

                await _repository.Add(auditLog);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the error but don't throw - we don't want audit failures to break the main operation
                System.Diagnostics.Debug.WriteLine($"Audit logging failed: {ex.Message}");
            }
        }

        public async Task<Result<PagedResultDto<AuditLogResponseDto>>> GetAuditLogsAsync(AuditLogFilterDto filter)
        {
            try
            {
                var query = _repository.GetAll<AuditLog>();

                // Apply filters
                if (filter.UserId.HasValue)
                    query = query.Where(a => a.UserId == filter.UserId.Value);

                if (!string.IsNullOrWhiteSpace(filter.Action))
                    query = query.Where(a => a.Action == filter.Action);

                if (!string.IsNullOrWhiteSpace(filter.EntityName))
                    query = query.Where(a => a.EntityName == filter.EntityName);

                if (filter.FromDate.HasValue)
                    query = query.Where(a => a.Timestamp >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(a => a.Timestamp <= filter.ToDate.Value);

                // Get total count before pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var auditLogs = await query
                    .OrderByDescending(a => a.Timestamp)
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                // Map to DTO
                var auditLogDtos = auditLogs.Select(a => new AuditLogResponseDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.UserName,
                    UserEmail = a.UserEmail,
                    Action = a.Action,
                    EntityName = a.EntityName,
                    EntityId = a.EntityId,
                    Changes = a.Changes,
                    Timestamp = a.Timestamp,
                    IpAddress = a.IpAddress
                }).ToList();

                var result = new PagedResultDto<AuditLogResponseDto>
                {
                    Items = auditLogDtos,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

                return Result<PagedResultDto<AuditLogResponseDto>>.Success(result);
            }
            catch (Exception ex)
            {
                return new Error[] { new("Error", $"Failed to retrieve audit logs: {ex.Message}") };
            }
        }
    }
}
