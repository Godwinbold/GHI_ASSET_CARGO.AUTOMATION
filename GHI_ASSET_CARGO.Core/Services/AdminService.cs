using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Admin;
using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GHI_ASSET_CARGO.Core.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public AdminService(UserManager<AppUser> userManager, IRepository repository, IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _userManager = userManager;
            _repository = repository;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<Result<PagedResult<UserListDto>>> GetUsers(int pageNumber = 1, int pageSize = 20, string? search = null)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = _userManager.Users.Where(u => !u.IsDeleted).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search) || u.Email.Contains(search) || (u.IdNumber != null && u.IdNumber.Contains(search)));
            }

            var total = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = new List<UserListDto>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                items.Add(new UserListDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    MiddleName = u.MiddleName,
                    Email = u.Email!,
                    PhoneNumber = u.PhoneNumber,
                    AirlineId = u.AirlineId,
                    IdNumber = u.IdNumber,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate,
                    Roles = roles
                });
            }

            var result = new PagedResult<UserListDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };

            return Result.Success(result);
        }

        public async Task<Result<object>> DeleteUserAsync(string userId, string? adminUserId = null, string? adminEmail = null, string? adminName = null, string? ipAddress = null)
        {
            // Validate user ID
            if (!Guid.TryParse(userId, out var userGuid))
                return new Error[] { new("User.Invalid", "Invalid user ID format.") };

            // Find the user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
                return new Error[] { new("User.NotFound", "User not found or already deleted.") };

            // Get user roles to check they are a regular user
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("USER"))
                return new Error[] { new("User.InvalidRole", "Only users with the 'USER' role can be deleted through this endpoint.") };

            // Soft delete: record deleted user and mark as deleted
            var userJson = JsonSerializer.Serialize(user, new JsonSerializerOptions { WriteIndented = false });
            var deletedEntity = new DeletedEntity
            {
                EntityType = "AppUser",
                EntityId = user.Id,
                EntityData = userJson,
                DeletedByUserId = string.IsNullOrEmpty(adminUserId) ? null : Guid.Parse(adminUserId),
                DeletedByEmail = adminEmail ?? "unknown@email.com",
                DeletedByName = adminName ?? "Unknown",
                DeletedDate = DateTimeOffset.UtcNow,
                IpAddress = ipAddress ?? "",
                DeletionReason = "not specified"
            };
            await _repository.Add(deletedEntity);

            // Mark as deleted instead of removing
            user.IsDeleted = true;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return new Error[] { new("User.DeleteFailed", "Failed to delete the user.") };

            await _unitOfWork.SaveChangesAsync();

            // Log audit event (fire and forget)
            if (!string.IsNullOrEmpty(adminUserId) && Guid.TryParse(adminUserId, out var adminGuid))
            {
                _ = _auditService.LogAuditAsync(
                    adminGuid,
                    adminName ?? "Unknown",
                    adminEmail ?? "unknown@email.com",
                    "DELETE",
                    "AppUser",
                    user.Id,
                    $"User {user.Email} deleted",
                    "",
                    userJson,
                    ipAddress ?? ""
                ).ConfigureAwait(false);
            }

            return Result<object>.Success(new { Message = "User deleted successfully" });
        }
    }
}
