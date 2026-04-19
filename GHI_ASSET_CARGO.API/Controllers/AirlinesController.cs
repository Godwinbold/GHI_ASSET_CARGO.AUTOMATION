using GHI_ASSET_CARGO.API.Dtos;
using GHI_ASSET_CARGO.API.Extensions;
using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Airline;
using GHI_ASSET_CARGO.Domain.Constants;
using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GHI_ASSET_CARGO.API.Controllers
{
    /// <summary>
    /// Airlines list and details. 
    /// </summary>
    [ApiController]
    [Route("api/airlines")]
    public class AirlinesController : ControllerBase
    {
        private readonly IRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public AirlinesController(IRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// List all airlines
        /// </summary>
        [HttpGet("get-all-airlines")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<IEnumerable<Airline>>>> GetAll()
        {
            var airlines = await _repository.GetAll<Airline>().ToListAsync();
            return Ok(ResponseDto<IEnumerable<Airline>>.Success(airlines));
        }

        /// <summary>
        /// Get one airline by id 
        /// </summary>
        [HttpGet("{id}/get-airline-by-id")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<Airline>>> GetById(string id)
        {
            var airline = await _repository.FindById<Airline>(Guid.Parse(id));
            if (airline == null)
                return NotFound(ResponseDto<Airline>.Failure(
                    new[] { new Error("Airline.NotFound", "Airline not found.") }, 404));

            return Ok(ResponseDto<Airline>.Success(airline));
        }

        /// <summary>
        /// Create a new airline. Only accessible by Admin and Executive.
        /// </summary>
        [HttpPost("create-airline")]
        [Authorize(Roles = $"{RolesConstant.Admin},{RolesConstant.Executive}")]
        public async Task<ActionResult<ResponseDto<Airline>>> CreateAirline([FromBody] CreateAirlineDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ResponseDto<Airline>.Failure(ModelState.GetErrors()));
            }

            var airline = new Airline
            {
                AirlineName = dto.AirlineName
            };

            await _repository.Add(airline);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = airline.Id }, ResponseDto<Airline>.Success(airline));
        }

        /// <summary>
        /// Update an airline. Only accessible by Admin and Executive.
        /// </summary>
        [HttpPut("{id}/update-airline")]
        [Authorize(Roles = $"{RolesConstant.Admin},{RolesConstant.Executive}")]
        public async Task<ActionResult<ResponseDto<Airline>>> UpdateAirline(Guid id, [FromBody] UpdateAirlineDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ResponseDto<Airline>.Failure(ModelState.GetErrors()));
            }

            var airline = await _repository.FindById<Airline>(id);
            if (airline == null)
                return NotFound(ResponseDto<Airline>.Failure(
                    new[] { new Error("Airline.NotFound", "Airline not found.") }, 404));

            airline.AirlineName = dto.AirlineName;
            airline.UpdatedDate = DateTimeOffset.UtcNow;

            _repository.Update(airline);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ResponseDto<Airline>.Success(airline));
        }

        /// <summary>
        /// Delete an airline. Only accessible by Admin and Executive.
        /// </summary>
        [HttpDelete("{id}/delete-airline")]
        [Authorize(Roles = $"{RolesConstant.Admin},{RolesConstant.Executive}")]
        public async Task<IActionResult> DeleteAirline(Guid id)
        {
            var airline = await _repository.FindById<Airline>(id);
            if (airline == null)
                return NotFound(ResponseDto<object>.Failure(
                    new[] { new Error("Airline.NotFound", "Airline not found.") }, 404));

            _repository.Remove(airline);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}
