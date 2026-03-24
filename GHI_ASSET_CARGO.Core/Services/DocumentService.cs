using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Document;
using GHI_ASSET_CARGO.Core.Dtos.Shipment;
using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GHI_ASSET_CARGO.Core.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DocumentService(IRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<PagedResultDto<ShipmentDocumentResponseDto>>> GetDocumentsByShipmentAsync(Guid shipmentId, string airlineId, int page = 1, int pageSize = 10)
        {
            var shipment = await _repository.FindById<Shipment>(shipmentId);
            if (shipment == null || shipment.AirlineId.ToString() != airlineId)
                return Result.Failure<PagedResultDto<ShipmentDocumentResponseDto>>(new[] { new Error("Shipment.NotFound", "Shipment not found or does not belong to the airline.") });

            var query = _repository.GetAll<ShipmentDocument>()
                .Where(d => d.ShipmentId == shipmentId);

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
                .FirstOrDefaultAsync(d => d.Id == documentId && d.Shipment.AirlineId.ToString() == airlineId);

            if (document == null)
                return Result.Failure<ShipmentDocumentResponseDto>(new[] { new Error("Document.NotFound", "Document not found.") });

            return Result<ShipmentDocumentResponseDto>.Success(MapToDto(document));
        }

        public async Task<Result<ShipmentDocumentResponseDto>> UploadDocumentAsync(Guid shipmentId, string airlineId, CreateShipmentDocumentRequestDto dto)
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
                UpdatedDate = DateTimeOffset.UtcNow
            };

            await _repository.Add(document);
            await _unitOfWork.SaveChangesAsync();

            return Result<ShipmentDocumentResponseDto>.Success(MapToDto(document));
        }

        public async Task<Result<ShipmentDocumentResponseDto>> UpdateDocumentAsync(Guid documentId, string airlineId, UpdateShipmentDocumentRequestDto dto)
        {
            var document = await _repository.GetAll<ShipmentDocument>()
                .Include(d => d.Shipment)
                .FirstOrDefaultAsync(d => d.Id == documentId && d.Shipment.AirlineId.ToString() == airlineId);

            if (document == null)
                return Result.Failure<ShipmentDocumentResponseDto>(new[] { new Error("Document.NotFound", "Document not found.") });

            document.FileName = dto.FileName;
            if (!string.IsNullOrEmpty(dto.ContentType)) document.ContentType = dto.ContentType;
            if (dto.FileSizeBytes.HasValue) document.FileSizeBytes = dto.FileSizeBytes;
            if (!string.IsNullOrEmpty(dto.StoragePath)) document.StoragePath = dto.StoragePath;
            document.UpdatedDate = DateTimeOffset.UtcNow;

            _repository.Update(document);
            await _unitOfWork.SaveChangesAsync();

            return Result<ShipmentDocumentResponseDto>.Success(MapToDto(document));
        }

        public async Task<Result> DeleteDocumentAsync(Guid documentId, string airlineId)
        {
            var document = await _repository.GetAll<ShipmentDocument>()
                .Include(d => d.Shipment)
                .FirstOrDefaultAsync(d => d.Id == documentId && d.Shipment.AirlineId.ToString() == airlineId);

            if (document == null)
                return Result.Failure(new[] { new Error("Document.NotFound", "Document not found.") });

            _repository.Remove(document);
            await _unitOfWork.SaveChangesAsync();

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