using GHI_ASSET_CARGO.API.Dtos;
using GHI_ASSET_CARGO.API.Extensions;
using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Document;
using GHI_ASSET_CARGO.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GHI_ASSET_CARGO.API.Controllers
{
    [ApiController]
    [Route("api/airlines/{airlineId}/shipments/{shipmentId}/documents")]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ICloudinaryService _cloudinaryService;

        public DocumentController(IDocumentService documentService, ICloudinaryService cloudinaryService)
        {
            _documentService = documentService;
            _cloudinaryService = cloudinaryService;
        }

        private IActionResult? EnsureUserCanAccessAirline(string airlineId)
        {
            // Admin and Executive can access any airline
            if (User.IsInRole(RolesConstant.Admin) || User.IsInRole(RolesConstant.Executive))
                return null;

            var userAirlineIds = User.GetAirlineIds();
            if (!userAirlineIds.Any())
                return StatusCode(403, ResponseDto<object>.Failure(new[] { new Error("Auth.AirlineRequired", "User is not associated with an airline.") }, 403));

            if (!userAirlineIds.Contains(airlineId, StringComparer.OrdinalIgnoreCase))
                return StatusCode(403, ResponseDto<object>.Failure(new[] { new Error("Auth.AirlineForbidden", "You do not have access to this airline.") }, 403));

            return null;
        }

        /// <summary>Try to get the logged-in user's numeric id from the claims.</summary>
        private bool TryGetLoggedInUserId(out Guid userId)
        {
            userId = Guid.Empty;
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(idClaim))
                return false;

            return Guid.TryParse(idClaim, out userId);
        }

        [HttpGet("get-documents")]
        public async Task<IActionResult> GetDocuments(string airlineId, Guid shipmentId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

            var result = await _documentService.GetDocumentsByShipmentAsync(shipmentId, airlineId, page, pageSize);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success(result.Data));
        }

        /// <summary>Get all documents for an airline (not tied to a shipment).</summary>
        [HttpGet("~/api/airlines/{airlineId}/get-documents-for-airline")]
        public async Task<IActionResult> GetDocumentsForAirline(string airlineId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

            var result = await _documentService.GetDocumentsByAirlineAsync(airlineId, page, pageSize);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success(result.Data));
        }

        [HttpGet("{id}/get-document-by-id")]
        public async Task<IActionResult> GetDocumentById(string airlineId, Guid shipmentId, Guid id)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

            var result = await _documentService.GetDocumentByIdAsync(id, airlineId);
            if (result.IsFailure)
                return NotFound(ResponseDto<object>.Failure(result.Errors, 404));

            return Ok(ResponseDto<object>.Success(result.Data));
        }

        [HttpPost("upload-document")]
        /// <summary>
        /// Upload a document for a shipment. 
        /// </summary>
        public async Task<IActionResult> UploadDocument(string airlineId, Guid shipmentId, [FromForm] UploadDocumentRequestDto dto)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

             if (!TryGetLoggedInUserId(out var uploaderId))
                return StatusCode(403, ResponseDto<object>.Failure(new[] { new Error("Auth.UserRequired", "Unable to determine the logged-in user.") }, 403));

            var file = dto.File;
            if (file == null || file.Length == 0)
                return BadRequest(ResponseDto<object>.Failure(new[] { new Error("Document.InvalidFile", "File is required.") }));

            var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/jpg", "image/png" };
            if (!allowedTypes.Contains(file.ContentType?.ToLowerInvariant()))
                return BadRequest(ResponseDto<object>.Failure(new[] { new Error("Document.UnsupportedType", "Only PDF, JPG, JPEG and PNG are allowed.") }));

            // Upload file to Cloudinary
            string storagePath;
            try
            {
                var folder = $"{airlineId}/shipments/{shipmentId}";
                storagePath = await _cloudinaryService.UploadFileAsync(file, folder);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseDto<object>.Failure(new[] { new Error("Document.UploadFailed", $"Failed to upload file to cloud storage: {ex.Message}") }));
            }

            var createDto = new CreateShipmentDocumentRequestDto
            {
                FileName = file.FileName,
                ContentType = file.ContentType ?? string.Empty,
                FileSizeBytes = file.Length,
                StoragePath = storagePath,
                UploadedByUserId = uploaderId
            };

            var result = await _documentService.UploadDocumentAsync(shipmentId, airlineId, createDto);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return CreatedAtAction(nameof(GetDocumentById), new { airlineId, shipmentId, id = result.Data.Id }, ResponseDto<object>.Success(result.Data));
        }

        [HttpPut("{id}/update-document")]
        public async Task<IActionResult> UpdateDocument(string airlineId, Guid shipmentId, Guid id, [FromBody] UpdateShipmentDocumentRequestDto dto)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

            if (dto.Id != id)
                return BadRequest(ResponseDto<object>.Failure(new[] { new Error("Document.IdMismatch", "Document ID in URL does not match request body.") }));

            var result = await _documentService.UpdateDocumentAsync(id, airlineId, dto);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success(result.Data));
        }

        [HttpDelete("{id}/delete-document")]
        public async Task<IActionResult> DeleteDocument(string airlineId, Guid shipmentId, Guid id)
        {
            var forbidden = EnsureUserCanAccessAirline(airlineId);
            if (forbidden != null) return forbidden;

            var result = await _documentService.DeleteDocumentAsync(id, airlineId);
            if (result.IsFailure)
                return NotFound(ResponseDto<object>.Failure(result.Errors, 404));

            return Ok(ResponseDto<object>.Success("Document deleted successfully."));
        }
    }
}

