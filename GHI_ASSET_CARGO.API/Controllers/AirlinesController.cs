using GHI_ASSET_CARGO.API.Dtos;
using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
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

        public AirlinesController(IRepository repository)
        {
            _repository = repository;
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
    }
}
