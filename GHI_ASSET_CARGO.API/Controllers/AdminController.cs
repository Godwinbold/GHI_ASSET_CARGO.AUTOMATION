using GHI_ASSET_CARGO.API.Dtos;
using GHI_ASSET_CARGO.API.Extensions;
using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos.Auth;
using GHI_ASSET_CARGO.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHI_ASSET_CARGO.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = RolesConstant.Admin)]
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

            var result = await _authService.InviteUser(inviteUserDto);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success("Invitation sent successfully"));
        }

        [HttpPost("users/{userId}/airlines/{airlineId}")]
        public async Task<IActionResult> AddUserToAirline(Guid userId, Guid airlineId)
        {
            var result = await _authService.AddUserToAirline(userId, airlineId);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success("User airline membership added successfully"));
        }

        [HttpDelete("users/{userId}/airlines/{airlineId}")]
        public async Task<IActionResult> RemoveUserFromAirline(Guid userId, Guid airlineId)
        {
            var result = await _authService.RemoveUserFromAirline(userId, airlineId);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return NoContent();
        }
    }
}
