using GHI_ASSET_CARGO.API.Dtos;
using GHI_ASSET_CARGO.API.Extensions;
using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Auth;
using GHI_ASSET_CARGO.Domain.Constants;
using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
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

        public AdminController(IAuthService authService, IAdminService adminService)
        {
            _authService = authService;
            _adminService = adminService;
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

            var result = await _authService.InviteUser(inviteUserDto);
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
    }
}
