using GHI_ASSET_CARGO.API.Dtos;
using GHI_ASSET_CARGO.API.Extensions;
using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GHI_ASSET_CARGO.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

       

        /// <summary>Register a user. When called from an airline portal, use the route with airlineId so the user is tied to that airline.</summary>
        [HttpPost("airlines/{airlineId}/register-user")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string airlineId, RegisterRequestDto request)
        {
            var result = await _authService.RegisterUser(request, airlineId);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success(result.Data));
        }


        /// <summary>Login for a specific airline portal. User must belong to this airline (user.AirlineId == airlineId).</summary>
        [HttpPost("airlines/{airlineId}/login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginForAirline(string airlineId, [FromBody] LoginRequestDto loginRequestDto)
        {
            var result = await _authService.Login(loginRequestDto, airlineId);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success(result.Data));
        }


        [HttpPost("register-admin")]
        public async Task<IActionResult> AdminRegister([FromBody] AdminRegisterDTO registerAdminDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ResponseDto<object>.Failure(ModelState.GetErrors()));
            }

            var result = await _authService.RegisterAdmin(registerAdminDto);

            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success());
        }

        [HttpPost("register-executive")]
        public async Task<IActionResult> ExecutiveRegister([FromBody] ExecutiveRegisterDTO registerExecutiveDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ResponseDto<object>.Failure(ModelState.GetErrors()));
            }

            var result = await _authService.RegisterExecutive(registerExecutiveDto);

            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success());
        }

       
       

       

        [HttpGet("get-current-loggedin-user")]
        [Authorize]
        public async Task<ActionResult> GetCurrentLoggedInUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _authService.GetCurrentLoggedInUser(userId);

            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success(result.Data));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ResponseDto<object>.Failure(ModelState.GetErrors()));
            }

            var resetPasswordResult = await _authService.ResetPasswordAsync(resetPasswordDto);

            if (resetPasswordResult.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(resetPasswordResult.Errors));

            return Ok(ResponseDto<object>.Success(resetPasswordResult));
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var result = await _authService.ForgotPassword(resetPasswordDto);

            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success());
        }

        [HttpPost("accept-invite")]
        public async Task<IActionResult> AcceptInvite([FromBody] AcceptInviteDto acceptInviteDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ResponseDto<object>.Failure(ModelState.GetErrors()));
            }

            var result = await _authService.AcceptInvite(acceptInviteDto);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success("Invitation accepted successfully"));
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
        {
            var result = await _authService.ConfirmEmail(email, token);

            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success());
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ResponseDto<object>.Failure(ModelState.GetErrors()));
            }
            var changePasswordResult = await _authService.ChangePasswordAsync(changePasswordDto);
            if (changePasswordResult.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(changePasswordResult.Errors));

            return Ok(ResponseDto<object>.Success(changePasswordResult));
        }

        

       
    }

}
