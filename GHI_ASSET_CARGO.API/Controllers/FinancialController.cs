using GHI_ASSET_CARGO.API.Dtos;
using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Financial;
using GHI_ASSET_CARGO.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GHI_ASSET_CARGO.API.Controllers
{
    /// <summary>
    /// Financial records scoped by airline and shipment. A single financial record is tied to a single shipment.
    /// </summary>
    [ApiController]
    [Route("api/airlines/{airlineId}/shipments/{shipmentId}/financials")]
    [Authorize]
    public class FinancialController : ControllerBase
    {
        private readonly IFinancialService _financialService;

        public FinancialController(IFinancialService financialService)
        {
            _financialService = financialService;
        }

        /// <summary>
        /// Ensures the current user can access the given airline.
        /// </summary>
        private IActionResult? EnsureUserCanAccessAirline(string airlineId)
        {
            // Admin and Executive can access any airline
            if (User.IsInRole(RolesConstant.Admin) || User.IsInRole(RolesConstant.Executive))
                return null;

            var userAirlineId = User.FindFirstValue("AirlineId");
            if (string.IsNullOrWhiteSpace(userAirlineId))
                return StatusCode(403, ResponseDto<object>.Failure(new[] { new Error("Auth.AirlineRequired", "User is not associated with an airline.") }, 403));

            if (!string.Equals(userAirlineId, airlineId, StringComparison.OrdinalIgnoreCase))
                return StatusCode(403, ResponseDto<object>.Failure(new[] { new Error("Auth.AirlineForbidden", "You do not have access to this airline.") }, 403));

            return null;
        }

        /// <summary>Get the financial record for the shipment (must belong to the airline in the route).</summary>
        [HttpGet("get-financial")]
        public async Task<IActionResult> GetFinancial(string airlineId, Guid shipmentId)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

            var result = await _financialService.GetFinancialByShipmentIdAsync(shipmentId, airlineId);
            if (result.IsFailure)
                return NotFound(ResponseDto<object>.Failure(result.Errors, 404));

            return Ok(ResponseDto<object>.Success(result.Data));
        }

        /// <summary>Get all financial records for an airline.</summary>
        [HttpGet("~/api/airlines/{airlineId}/financials")]
        public async Task<IActionResult> GetAllForAirline(string airlineId)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

            var result = await _financialService.GetFinancialsByAirlineAsync(airlineId);
            if (result.IsFailure)
                return NotFound(ResponseDto<object>.Failure(result.Errors, 404));

            return Ok(ResponseDto<object>.Success(result.Data));
        }

        /// <summary>Create a new financial record for the shipment.</summary>
        [HttpPost("create-financial")]
        public async Task<IActionResult> Create(string airlineId, Guid shipmentId, [FromBody] CreateFinancialRequestDto dto)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

            var result = await _financialService.CreateFinancialAsync(shipmentId, airlineId, dto);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return CreatedAtAction(nameof(GetFinancial), new { airlineId, shipmentId }, ResponseDto<object>.Success(result.Data));
        }

        /// <summary>Update an existing financial record.</summary>
        [HttpPut("{financialId}/update-financial")]
        public async Task<IActionResult> Update(string airlineId, Guid shipmentId, Guid financialId, [FromBody] UpdateFinancialRequestDto dto)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

            if (dto.Id != financialId)
                return BadRequest(ResponseDto<object>.Failure(new[] { new Error("Financial.IdMismatch", "Financial ID in URL does not match the ID in the request body.") }));

            var result = await _financialService.UpdateFinancialAsync(financialId, airlineId, dto);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success(result.Data));
        }

        /// <summary>Delete a financial record.</summary>
        [HttpDelete("{financialId}/delete-financial")]
        public async Task<IActionResult> Delete(string airlineId, Guid shipmentId, Guid financialId)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

            var result = await _financialService.DeleteFinancialAsync(financialId, airlineId);
            if (result.IsFailure)
                return NotFound(ResponseDto<object>.Failure(result.Errors, 404));

            return Ok(ResponseDto<object>.Success("Financial record deleted successfully."));
        }
    }
}
