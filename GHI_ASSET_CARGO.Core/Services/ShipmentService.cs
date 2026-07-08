using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Shipment;
using GHI_ASSET_CARGO.Domain.Entities;
using GHI_ASSET_CARGO.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GHI_ASSET_CARGO.Core.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly IRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ShipmentService(IRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<PagedResultDto<ShipmentResponseDto>>> GetShipmentsForAirlineAsync(string airlineId, int page, int pageSize, string? awbSearch = null)
        {
            var query = _repository.GetAll<Shipment>().Where(s => s.AirlineId.ToString() == airlineId);

            if (!string.IsNullOrWhiteSpace(awbSearch))
                query = query.Where(s => s.AirwayBillNumber.Contains(awbSearch));

            var totalCount = await query.CountAsync();

            var shipments = await query
                .OrderByDescending(s => s.ShipmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = new List<ShipmentResponseDto>();
            foreach (var s in shipments)
            {
                var notes = await _repository.GetAll<ShipmentNote>()
                    .Where(n => n.ShipmentId == s.Id)
                    .OrderByDescending(n => n.CreatedDate)
                    .ToListAsync();
                items.Add(MapToResponse(s, notes));
            }

            var paged = new PagedResultDto<ShipmentResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Result<PagedResultDto<ShipmentResponseDto>>.Success(paged);
        }

        public async Task<Result<PagedResultDto<ShipmentResponseDto>>> GetShipmentsByStatusAsync(string airlineId, ShipmentStatus status, int page = 1, int pageSize = 10)
        {
            var query = _repository.GetAll<Shipment>()
                .Where(s => s.AirlineId.ToString() == airlineId && s.Status == status);

            var totalCount = await query.CountAsync();

            var shipments = await query
                .OrderByDescending(s => s.ShipmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = new List<ShipmentResponseDto>();
            foreach (var s in shipments)
            {
                var notes = await _repository.GetAll<ShipmentNote>()
                    .Where(n => n.ShipmentId == s.Id)
                    .OrderByDescending(n => n.CreatedDate)
                    .ToListAsync();
                items.Add(MapToResponse(s, notes));
            }

            var paged = new PagedResultDto<ShipmentResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Result<PagedResultDto<ShipmentResponseDto>>.Success(paged);
        }

        public async Task<Result<PagedResultDto<ShipmentResponseDto>>> GetShipmentsForAirlineFilteredAsync(string airlineId, Guid? userId = null, string? awb = null, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, ShipmentStatus? status = null, int page = 1, int pageSize = 10)
        {
            // default date range: last month to now if not supplied
            var start = startDate ?? DateTimeOffset.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTimeOffset.UtcNow;

            var query = _repository.GetAll<Shipment>().Where(s => s.AirlineId.ToString() == airlineId);

            if (!string.IsNullOrWhiteSpace(awb))
                query = query.Where(s => s.AirwayBillNumber.Contains(awb));

            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);

            // apply date range
            query = query.Where(s => s.ShipmentDate >= start && s.ShipmentDate <= end);

            // if filtering by user, find shipments that have documents uploaded by that user
            if (userId.HasValue)
            {
                var docShipments = _repository.GetAll<Domain.Entities.ShipmentDocument>()
                    .Where(d => d.UploadedByUserId == userId.Value)
                    .Select(d => d.ShipmentId);

                query = query.Where(s => docShipments.Contains(s.Id));
            }

            var totalCount = await query.CountAsync();

            var shipments = await query
                .OrderByDescending(s => s.ShipmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = new List<ShipmentResponseDto>();
            foreach (var s in shipments)
            {
                var notes = await _repository.GetAll<ShipmentNote>()
                    .Where(n => n.ShipmentId == s.Id)
                    .OrderByDescending(n => n.CreatedDate)
                    .ToListAsync();
                items.Add(MapToResponse(s, notes));
            }

            var paged = new PagedResultDto<ShipmentResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Result<PagedResultDto<ShipmentResponseDto>>.Success(paged);
        }

        public async Task<Result<ShipmentResponseDto>> GetShipmentByIdAsync(string shipmentId, string airlineId)
        {
            var shipment = await _repository.FindById<Shipment>(Guid.Parse(shipmentId));
            if (shipment == null)
                return new Error[] { new("Shipment.NotFound", "Shipment not found.") };

            if (!string.Equals(shipment.AirlineId.ToString(), airlineId, StringComparison.OrdinalIgnoreCase))
                return new Error[] { new("Shipment.Forbidden", "You do not have access to this shipment.") };

            var notes = await _repository.GetAll<ShipmentNote>()
                .Where(n => n.ShipmentId.ToString() == shipmentId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            return Result<ShipmentResponseDto>.Success(MapToResponse(shipment, notes));
        }

        public async Task<Result<ShipmentResponseDto>> GetShipmentByAwbAsync(string airwayBillNumber, string airlineId)
        {
            var shipment = await _repository.GetAll<Shipment>()
                .Where(s => s.AirlineId.ToString() == airlineId && s.AirwayBillNumber == airwayBillNumber)
                .FirstOrDefaultAsync();

            if (shipment == null)
                return new Error[] { new("Shipment.NotFound", "No tracking information available for the provided ID. Please ensure the tracking ID is correct or contact support for assistance.") };

            var notes = await _repository.GetAll<ShipmentNote>()
                .Where(n => n.ShipmentId == shipment.Id)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            return Result<ShipmentResponseDto>.Success(MapToResponse(shipment, notes));
        }

        public async Task<Result<ShipmentResponseDto>> CreateShipmentAsync(string airlineId, CreateShipmentRequestDto dto)
        {
            var exists = await _repository.GetAll<Shipment>()
                .AnyAsync(s => s.AirlineId.ToString() == airlineId && s.AirwayBillNumber == dto.AirwayBillNumber);
            if (exists)
                return new Error[] { new("Shipment.DuplicateAwb", "A shipment with this Airway Bill Number already exists for your airline.") };

            var shipment = new Shipment
            {
                AirlineId =Guid.Parse( airlineId),
                AirwayBillNumber = dto.AirwayBillNumber,
                Status = dto.Status,
                ShipmentDate = dto.ShipmentDate,
                CreatedDate = DateTimeOffset.UtcNow,
                UpdatedDate = DateTimeOffset.UtcNow
            };

            await _repository.Add(shipment);
            await _unitOfWork.SaveChangesAsync();

            return Result<ShipmentResponseDto>.Success(MapToResponse(shipment, new List<ShipmentNote>()));
        }

        public async Task<Result> AddNoteAsync(string shipmentId, string airlineId, AddNoteRequestDto dto)
        {
            var shipment = await _repository.FindById<Shipment>(Guid.Parse(shipmentId));
            if (shipment == null)
                return new Error[] { new("Shipment.NotFound", "Shipment not found.") };

            if (!string.Equals(shipment.AirlineId.ToString(), airlineId, StringComparison.OrdinalIgnoreCase))
                return new Error[] { new("Shipment.Forbidden", "You do not have access to this shipment.") };

            var note = new ShipmentNote
            {
                ShipmentId = Guid.Parse(shipmentId),
                Content = dto.Content,
                CreatedDate = DateTimeOffset.UtcNow,
                UpdatedDate = DateTimeOffset.UtcNow
            };

            await _repository.Add(note);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> DeleteShipmentAsync(string shipmentId, string airlineId)
        {
            var shipment = await _repository.FindById<Shipment>(Guid.Parse(shipmentId));
            if (shipment == null)
                return new Error[] { new("Shipment.NotFound", "Shipment not found.") };

            if (!string.Equals(shipment.AirlineId.ToString(), airlineId, StringComparison.OrdinalIgnoreCase))
                return new Error[] { new("Shipment.Forbidden", "You do not have access to this shipment.") };

            _repository.Remove(shipment);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }

        private static ShipmentResponseDto MapToResponse(Shipment s, List<ShipmentNote> notes)
        {
            return new ShipmentResponseDto
            {
                Id = s.Id.ToString(),
                AirlineId = s.AirlineId.ToString(),
                AirwayBillNumber = s.AirwayBillNumber,
                Status = s.Status,
                StatusDisplay = s.Status.ToString(),
                ShipmentDate = s.ShipmentDate,
                HasFinancial = s.HasFinancial,
                CreatedDate = s.CreatedDate,
                UpdatedDate = s.UpdatedDate,
                Notes = notes.Select(n => new ShipmentNoteDto
                {
                    Id = n.Id.ToString(),
                    Content = n.Content,
                    CreatedDate = n.CreatedDate
                }).ToList()
            };
        }
    }
}
