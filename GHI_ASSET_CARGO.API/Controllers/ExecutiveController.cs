using GHI_ASSET_CARGO.API.Dtos;
using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos.Executive;
using GHI_ASSET_CARGO.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHI_ASSET_CARGO.API.Controllers
{
    [ApiController]
    [Route("api/executive")]
    [Authorize(Roles = RolesConstant.Executive)]
    public class ExecutiveController : ControllerBase
    {
        private readonly IExecutiveService _executiveService;

        public ExecutiveController(IExecutiveService executiveService)
        {
            _executiveService = executiveService;
        }

        /// <summary>
        /// Get the executive dashboard data. Accepts optional query filters `startDate`, `endDate`, and `airlineId`.
        /// </summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ResponseDto<ExecutiveDashboardResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDashboard([FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate, [FromQuery] Guid? airlineId)
        {
            var request = new ExecutiveDashboardRequestDto
            {
                StartDate = startDate,
                EndDate = endDate,
                AirlineId = airlineId
            };

            var result = await _executiveService.GetExecutiveDashboardAsync(request);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success(result.Data));
        }
    }
}
