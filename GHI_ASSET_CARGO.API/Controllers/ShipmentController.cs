using GHI_ASSET_CARGO.API.Dtos;
using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Shipment;
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
    }
}
