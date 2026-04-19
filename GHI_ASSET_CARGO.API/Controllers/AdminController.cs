using GHI_ASSET_CARGO.API.Dtos;
using GHI_ASSET_CARGO.API.Extensions;
using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
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

        public AdminController(IAuthService authService)
        {
            _authService = authService;
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
    }
}
