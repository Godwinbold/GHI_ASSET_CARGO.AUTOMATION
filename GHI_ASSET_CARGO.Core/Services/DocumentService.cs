using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Document;
using GHI_ASSET_CARGO.Core.Dtos.Shipment;
using GHI_ASSET_CARGO.Core.Utilities;
using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GHI_ASSET_CARGO.Core.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public DocumentService(IRepository repository, IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<Result<PagedResultDto<ShipmentDocumentResponseDto>>> GetDocumentsByShipmentAsync(Guid shipmentId, string airlineId, int page = 1, int pageSize = 10)
        {
            var shipment = await _repository.FindById<Shipment>(shipmentId);
            if (shipment == null || shipment.AirlineId.ToString() != airlineId)
                return Result.Failure<PagedResultDto<ShipmentDocumentResponseDto>>(new[] { new Error("Shipment.NotFound", "Shipment not found or does not belong to the airline.") });

            var query = _repository.GetAll<ShipmentDocument>()
                .Where(d => d.ShipmentId == shipmentId && !d.IsDeleted);

            var totalCount = await query.CountAsync();
            var documents = await query
                .OrderByDescending(d => d.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResultDto<ShipmentDocumentResponseDto>
            {
                Items = documents.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Result<PagedResultDto<ShipmentDocumentResponseDto>>.Success(result);
        }

        public async Task<Result<ShipmentDocumentResponseDto>> GetDocumentByIdAsync(Guid documentId, string airlineId)
        {
            var document = await _repository.GetAll<ShipmentDocument>()
                .Include(d => d.Shipment)
                .FirstOrDefaultAsync(d => d.Id == documentId && d.Shipment.AirlineId.ToString() == airlineId && !d.IsDeleted);

            if (document == null)
                return Result.Failure<ShipmentDocumentResponseDto>(new[] { new Error("Document.NotFound", "Document not found.") });

            return Result<ShipmentDocumentResponseDto>.Success(MapToDto(document));
        }

        public async Task<Result<PagedResultDto<ShipmentDocumentResponseDto>>> GetDocumentsByAirlineAsync(string airlineId, int page = 1, int pageSize = 10)
        {
            var query = _repository.GetAll<ShipmentDocument>()
                .Include(d => d.Shipment)
                .Where(d => d.Shipment.AirlineId.ToString() == airlineId && !d.IsDeleted);

            var totalCount = await query.CountAsync();
            var documents = await query
                .OrderByDescending(d => d.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResultDto<ShipmentDocumentResponseDto>
            {
                Items = documents.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Result<PagedResultDto<ShipmentDocumentResponseDto>>.Success(result);
        }

        public async Task<Result<ShipmentDocumentResponseDto>> UploadDocumentAsync(Guid shipmentId, string airlineId, CreateShipmentDocumentRequestDto dto, string? userId = null, string? userEmail = null, string? userName = null, string? ipAddress = null)
        {
            var shipment = await _repository.FindById<Shipment>(shipmentId);
            if (shipment == null || shipment.AirlineId.ToString() != airlineId)
                return Result.Failure<ShipmentDocumentResponseDto>(new[] { new Error("Shipment.NotFound", "Shipment not found or does not belong to the airline.") });

            // accept cloudinary path from caller (or do upload in controller), then create record.
            var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/jpg", "image/png" };
            if (!allowedTypes.Contains(dto.ContentType?.ToLowerInvariant()))
                return Result.Failure<ShipmentDocumentResponseDto>(new[] { new Error("Document.UnsupportedType", "Only PDF, JPEG, JPG and PNG are allowed.") });

            var document = new ShipmentDocument
            {
                ShipmentId = shipmentId,
                AirwayBillNumber = shipment.AirwayBillNumber,
                ShipmentDate = shipment.ShipmentDate.DateTime,
                FileName = dto.FileName,
                ContentType = dto.ContentType,
                FileSizeBytes = dto.FileSizeBytes,
                StoragePath = dto.StoragePath,
                UploadedAt = DateTime.UtcNow,
                UploadedByUserId = dto.UploadedByUserId,
                CreatedDate = DateTimeOffset.UtcNow,
                UpdatedDate = DateTimeOffset.UtcNow,
                LastUpdatedBy = string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId)
            };

            await _repository.Add(document);
            await _unitOfWork.SaveChangesAsync();

            // Audit logging (fire and forget - don't break main operation if audit fails)
            if (!string.IsNullOrEmpty(userId))
            {
                _ = _auditService.LogAuditAsync(
                    userId: Guid.Parse(userId),
                    userName: userName ?? "Unknown",
                    userEmail: userEmail ?? "unknown@email.com",
                    action: "Create",
                    entityName: nameof(ShipmentDocument),
                    entityId: document.Id,
                    changes: $"Document uploaded: {document.FileName} ({dto.ContentType})",
                    ipAddress: ipAddress ?? ""
                ).ConfigureAwait(false);
            }

            return Result<ShipmentDocumentResponseDto>.Success(MapToDto(document));
        }

        public async Task<Result<ShipmentDocumentResponseDto>> UpdateDocumentAsync(Guid documentId, string airlineId, UpdateShipmentDocumentRequestDto dto, string? userId = null, string? userEmail = null, string? userName = null, string? ipAddress = null)
        {
            var document = await _repository.GetAll<ShipmentDocument>()
                .Include(d => d.Shipment)
                .FirstOrDefaultAsync(d => d.Id == documentId && d.Shipment.AirlineId.ToString() == airlineId);

            if (document == null)
                return Result.Failure<ShipmentDocumentResponseDto>(new[] { new Error("Document.NotFound", "Document not found.") });

            var oldValues = new Dictionary<string, object?>
            {
                { "FileName", document.FileName },
                { "ContentType", document.ContentType },
                { "FileSizeBytes", document.FileSizeBytes }
            };

            document.FileName = dto.FileName;
            if (!string.IsNullOrEmpty(dto.ContentType)) document.ContentType = dto.ContentType;
            if (dto.FileSizeBytes.HasValue) document.FileSizeBytes = dto.FileSizeBytes;
            if (!string.IsNullOrEmpty(dto.StoragePath)) document.StoragePath = dto.StoragePath;
            document.UpdatedDate = DateTimeOffset.UtcNow;
            document.LastUpdatedBy = string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId);

            _repository.Update(document);
            await _unitOfWork.SaveChangesAsync();

            // Audit logging (fire and forget - don't break main operation if audit fails)
            if (!string.IsNullOrEmpty(userId))
            {
                var newValues = new Dictionary<string, object?>
                {
                    { "FileName", document.FileName },
                    { "ContentType", document.ContentType },
                    { "FileSizeBytes", document.FileSizeBytes }
                };
                var changeSummary = AuditHelper.GetChangeSummary(oldValues, newValues);

                _ = _auditService.LogAuditAsync(
                    userId: Guid.Parse(userId),
                    userName: userName ?? "Unknown",
                    userEmail: userEmail ?? "unknown@email.com",
                    action: "Update",
                    entityName: nameof(ShipmentDocument),
                    entityId: document.Id,
                    changes: changeSummary,
                    oldValues: AuditHelper.SerializeToJson(oldValues),
                    newValues: AuditHelper.SerializeToJson(newValues),
                    ipAddress: ipAddress ?? ""
                ).ConfigureAwait(false);
            }

            return Result<ShipmentDocumentResponseDto>.Success(MapToDto(document));
        }

        public async Task<Result> DeleteDocumentAsync(Guid documentId, string airlineId, string? userId = null, string? userEmail = null, string? userName = null, string? ipAddress = null)
        {
            var document = await _repository.GetAll<ShipmentDocument>()
                .Include(d => d.Shipment)
                .FirstOrDefaultAsync(d => d.Id == documentId && d.Shipment.AirlineId.ToString() == airlineId);

            if (document == null)
                return Result.Failure(new[] { new Error("Document.NotFound", "Document not found.") });

            var documentData = $"File: {document.FileName}, Size: {document.FileSizeBytes} bytes";
            
            // Soft delete: record deleted entity and mark as deleted
            var deletedEntity = SoftDeleteHelper.CreateDeletedEntity(
                document,
                string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId),
                userEmail,
                userName,
                ipAddress
            );
            await _repository.Add(deletedEntity);

            // Mark as deleted instead of removing
            document.IsDeleted = true;
            _repository.Update(document);
            await _unitOfWork.SaveChangesAsync();

            // Audit logging (fire and forget - don't break main operation if audit fails)
            if (!string.IsNullOrEmpty(userId))
            {
                _ = _auditService.LogAuditAsync(
                    userId: Guid.Parse(userId),
                    userName: userName ?? "Unknown",
                    userEmail: userEmail ?? "unknown@email.com",
                    action: "Delete",
                    entityName: nameof(ShipmentDocument),
                    entityId: documentId,
                    changes: $"Document deleted: {documentData}",
                    ipAddress: ipAddress ?? ""
                ).ConfigureAwait(false);
            }

            return Result.Success();
        }

        private ShipmentDocumentResponseDto MapToDto(ShipmentDocument document)
        {
            return new ShipmentDocumentResponseDto
            {
                Id = document.Id,
                ShipmentId = document.ShipmentId,
                AirwayBillNumber = document.AirwayBillNumber,
                ShipmentDate = document.ShipmentDate,
                FileName = document.FileName,
                ContentType = document.ContentType,
                FileSizeBytes = document.FileSizeBytes,
                StoragePath = document.StoragePath,
                UploadedAt = document.UploadedAt,
                UploadedByUserId = document.UploadedByUserId,
                CreatedDate = document.CreatedDate,
                UpdatedDate = document.UpdatedDate,
            };
        }
    }
}