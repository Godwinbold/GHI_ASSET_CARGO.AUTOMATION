using GHI_ASSET_CARGO.API.Dtos;
using GHI_ASSET_CARGO.API.Extensions;
using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.AuditLog;
using GHI_ASSET_CARGO.Core.Dtos.Auth;
using GHI_ASSET_CARGO.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHI_ASSET_CARGO.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = RolesConstant.Admin + "," + RolesConstant.Executive)]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAdminService _adminService;
        private readonly IAuditService _auditService;

        public AdminController(IAuthService authService, IAdminService adminService, IAuditService auditService)
        {
            _authService = authService;
            _adminService = adminService;
            _auditService = auditService;
        }

        [HttpPost("invite-user")]
        public async Task<IActionResult> InviteUser([FromBody] InviteUserDto inviteUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ResponseDto<object>.Failure(ModelState.GetErrors()));
            }

            var normalizedRole = inviteUserDto.Role?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(normalizedRole))
                return BadRequest(ResponseDto<object>.Failure(new[] { new Error("Invitation.Error", "Role is required") }));

            if (User.IsInRole(RolesConstant.Executive))
            {
                if (normalizedRole != RolesConstant.Executive)
                    return Forbid();
            }
            else if (User.IsInRole(RolesConstant.Admin))
            {
                if (normalizedRole != RolesConstant.Admin && normalizedRole != RolesConstant.User)
                    return Forbid();
            }
            else
            {
                return Forbid();
            }

            var (userId, email, fullName) = User.GetAuditUserInfo();
            var ipAddress = HttpContext.GetClientIpAddress();

            var result = await _authService.InviteUser(inviteUserDto, userId, email, fullName, ipAddress);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success("Invitation sent successfully"));
        }

        [HttpGet("get-app-users")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        {
            var result = await _adminService.GetUsers(pageNumber, pageSize, search);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success(result.Data));
        }

        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] Guid? userId = null,
            [FromQuery] string? action = null,
            [FromQuery] string? entityName = null,
            [FromQuery] DateTimeOffset? fromDate = null,
            [FromQuery] DateTimeOffset? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var filter = new AuditLogFilterDto
            {
                UserId = userId,
                Action = action,
                EntityName = entityName,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize
            };

            var result = await _auditService.GetAuditLogsAsync(filter);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success(result.Data));
        }

        [HttpDelete("delete-user/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var (adminUserId, adminEmail, adminName) = User.GetAuditUserInfo();
            var ipAddress = HttpContext.GetClientIpAddress();

            var result = await _adminService.DeleteUserAsync(userId, adminUserId, adminEmail, adminName, ipAddress);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success(result.Data));
        }
    }
}

