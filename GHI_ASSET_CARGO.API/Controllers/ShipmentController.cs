using GHI_ASSET_CARGO.API.Dtos;
using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Shipment;
using GHI_ASSET_CARGO.Domain.Constants;
using GHI_ASSET_CARGO.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GHI_ASSET_CARGO.API.Controllers
{
    /// <summary>
    /// Shipments scoped by airline. Use airlineId in the route so the same API works when users are tied to one or multiple airlines.
    /// </summary>
    [ApiController]
    [Route("api/airlines/{airlineId}/shipments")]
    [Authorize]
    public class ShipmentController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;

        public ShipmentController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        /// <summary>
        /// Ensures the current user can access the given airline. Today: user's AirlineId must match. Later: can be extended to allow multiple airlines per user.
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


        /// <summary>List shipments for the airline in the route (user must have access to that airline).</summary>
        [HttpGet("get-shipments")]
        public async Task<IActionResult> GetShipments(string airlineId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? awbSearch = null)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

            var result = await _shipmentService.GetShipmentsForAirlineAsync(airlineId, page, pageSize, awbSearch);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success(result.Data));
        }

        /// <summary>Get shipments for an airline with flexible filters: userId, awbNumber, date range (startDate/endDate), status. Date range defaults to last month if not supplied.</summary>
        [HttpGet("filter")]
        public async Task<IActionResult> GetShipmentsFiltered(string airlineId, [FromQuery] Guid? userId = null, [FromQuery] string? awb = null, [FromQuery] DateTimeOffset? startDate = null, [FromQuery] DateTimeOffset? endDate = null, [FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

            ShipmentStatus? shipmentStatus = null;
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!Enum.TryParse<ShipmentStatus>(status, true, out var parsed))
                    return BadRequest(ResponseDto<object>.Failure(new[] { new Error("Shipment.InvalidStatus", "Invalid shipment status filter.") }));
                shipmentStatus = parsed;
            }

            var result = await _shipmentService.GetShipmentsForAirlineFilteredAsync(airlineId, userId, awb, startDate, endDate, shipmentStatus, page, pageSize);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success(result.Data));
        }

        [HttpGet("get-by-status")]
        public async Task<IActionResult> GetShipmentsByStatus(string airlineId, [FromQuery] string status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

            if (!Enum.TryParse<ShipmentStatus>(status, true, out var shipmentStatus))
                return BadRequest(ResponseDto<object>.Failure(new[] { new Error("Shipment.InvalidStatus", "Invalid shipment status.") }));

            var result = await _shipmentService.GetShipmentsByStatusAsync(airlineId, shipmentStatus, page, pageSize);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success(result.Data));
        }

        /// <summary>Search by AWB number within the airline in the route.</summary>
        [HttpGet("search-by-awb")]
        public async Task<IActionResult> SearchByAwb(string airlineId, [FromQuery] string awbNumber)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

            if (string.IsNullOrWhiteSpace(awbNumber))
                return BadRequest(ResponseDto<object>.Failure(new[] { new Error("Shipment.InvalidAwb", "AWB number is required.") }));

            var result = await _shipmentService.GetShipmentByAwbAsync(awbNumber.Trim(), airlineId);
            if (result.IsFailure)
                return NotFound(ResponseDto<object>.Failure(result.Errors, 404));

            return Ok(ResponseDto<object>.Success(result.Data));
        }

        /// <summary>Get a single shipment by id (must belong to the airline in the route).</summary>
        [HttpGet("{id}/get-shipment-by-id")]
        public async Task<IActionResult> GetById(string airlineId, string id)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

            var result = await _shipmentService.GetShipmentByIdAsync(id, airlineId);
            if (result.IsFailure)
                return NotFound(ResponseDto<object>.Failure(result.Errors, 404));

            return Ok(ResponseDto<object>.Success(result.Data));
        }

        /// <summary>Create a new shipment for the airline in the route.</summary>
        [HttpPost("create-shipment")]
        public async Task<IActionResult> Create(string airlineId, [FromBody] CreateShipmentRequestDto dto)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

            var result = await _shipmentService.CreateShipmentAsync(airlineId, dto);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return CreatedAtAction(nameof(GetById), new { airlineId, id = result.Data.Id }, ResponseDto<object>.Success(result.Data));
        }

        /// <summary>Add a note to a shipment.</summary>
        [HttpPost("{id}/add-notes")]
        public async Task<IActionResult> AddNote(string airlineId, string id, [FromBody] AddNoteRequestDto dto)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

            var result = await _shipmentService.AddNoteAsync(id, airlineId, dto);
            if (result.IsFailure)
                return NotFound(ResponseDto<object>.Failure(result.Errors, 404));

            return Ok(ResponseDto<object>.Success("Note added successfully."));
        }

        /// <summary>Delete a shipment (and its related notes/documents) for the airline.</summary>
        [HttpDelete("{id}/delete-shipment")]
        public async Task<IActionResult> Delete(string airlineId, string id)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

            var result = await _shipmentService.DeleteShipmentAsync(id, airlineId);
            if (result.IsFailure)
                return NotFound(ResponseDto<object>.Failure(result.Errors, 404));

            return Ok(ResponseDto<object>.Success("Shipment deleted successfully."));
        }

        

    }
}
