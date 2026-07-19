using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Financial;
using GHI_ASSET_CARGO.Core.Utilities;
using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GHI_ASSET_CARGO.Core.Services
{
    public class FinancialService : IFinancialService
    {
        private readonly IRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public FinancialService(IRepository repository, IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<Result<FinancialResponseDto>> GetFinancialByShipmentIdAsync(Guid shipmentId, string airlineId)
        {
            var financial = await _repository.GetAll<Financial>()
                .Include(f => f.Shipment)
                .FirstOrDefaultAsync(f => f.ShipmentId == shipmentId && f.AirlineId.ToString() == airlineId && !f.IsDeleted);

            if (financial == null)
                return Result.Failure<FinancialResponseDto>(new[] { new Error("Financial.NotFound", "Financial record not found for this shipment.") });

            return Result<FinancialResponseDto>.Success(MapToResponse(financial));
        }

        public async Task<Result<FinancialResponseDto>> GetFinancialByIdAsync(Guid financialId, string airlineId)
        {
            var financial = await _repository.GetAll<Financial>()
                .Include(f => f.Shipment)
                .FirstOrDefaultAsync(f => f.Id == financialId && f.AirlineId.ToString() == airlineId && !f.IsDeleted);

            if (financial == null)
                return Result.Failure<FinancialResponseDto>(new[] { new Error("Financial.NotFound", "Financial record not found.") });

            return Result<FinancialResponseDto>.Success(MapToResponse(financial));
        }

        public async Task<Result<List<FinancialResponseDto>>> GetFinancialsByAirlineAsync(string airlineId)
        {
            var financials = await _repository.GetAll<Financial>()
                .Include(f => f.Shipment)
                .Where(f => f.AirlineId.ToString() == airlineId && !f.IsDeleted)
                .ToListAsync();

            var dtos = financials.Select(MapToResponse).ToList();

            return Result<List<FinancialResponseDto>>.Success(dtos);
        }

        public async Task<Result<FinancialResponseDto>> CreateFinancialAsync(Guid shipmentId, string airlineId, CreateFinancialRequestDto dto, string? userId = null, string? userEmail = null, string? userName = null, string? ipAddress = null)
        {
            // Check if shipment exists and belongs to airline
            var shipment = await _repository.FindById<Shipment>(shipmentId);
            if (shipment == null || shipment.AirlineId.ToString() != airlineId)
                return Result.Failure<FinancialResponseDto>(new[] { new Error("Shipment.NotFound", "Shipment not found or does not belong to the airline.") });

            // Check if financial already exists for this shipment
            var existing = await _repository.GetAll<Financial>().AnyAsync(f => f.ShipmentId == shipmentId && !f.IsDeleted);
            if (existing)
                return Result.Failure<FinancialResponseDto>(new[] { new Error("Financial.AlreadyExists", "Financial record already exists for this shipment.") });

            var financial = new Financial
            {
                AirlineId = shipment.AirlineId,
                ShipmentId = shipmentId,
                MAWB = dto.MAWB,
                DateOfIssue = dto.DateOfIssue,
                AgentsOrClients = dto.AgentsOrClients,
                Product = dto.Product,
                Routing = dto.Routing,
                FlightNo = dto.FlightNo,
                Pieces = dto.Pieces,
                ChargeableWeightKg = dto.ChargeableWeightKg,
                GrossWeightKg = dto.GrossWeightKg,
                SpotRate = dto.SpotRate,
                PublishedRates = dto.PublishedRates,
                ROE = dto.ROE,
                FreightAmountNGN = dto.FreightAmountNGN,
                NCAACharges5Percent = dto.NCAACharges5Percent,
                TotalChargeNGN = dto.TotalChargeNGN,
                ChargesCollect = dto.ChargesCollect,
                FuelSurcharge = dto.FuelSurcharge,
                SECSurcharge = dto.SECSurcharge,
                HandlingSurcharge = dto.HandlingSurcharge,
                SurchargeDueAgent = dto.SurchargeDueAgent,
                AWBFee = dto.AWBFee,
                GSACommissionNGN = dto.GSACommissionNGN,
                VATOnCommission = dto.VATOnCommission,
                AmtDueAirline = dto.AmtDueAirline,
                DueAPGInc = dto.DueAPGInc,
                DueSLC = dto.DueSLC,
                LastUpdatedBy = string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId)
            };

            await _repository.Add(financial);
            shipment.HasFinancial = true;
            _repository.Update(shipment);
            await _unitOfWork.SaveChangesAsync();

            // Audit logging (fire and forget - don't break main operation if audit fails)
            if (!string.IsNullOrEmpty(userId))
            {
                _ = _auditService.LogAuditAsync(
                    userId: Guid.Parse(userId),
                    userName: userName ?? "Unknown",
                    userEmail: userEmail ?? "unknown@email.com",
                    action: "Create",
                    entityName: nameof(Financial),
                    entityId: financial.Id,
                    changes: $"Financial record created: MAWB {financial.MAWB}, Amount NGN {financial.TotalChargeNGN}",
                    ipAddress: ipAddress ?? ""
                ).ConfigureAwait(false);
            }

            return Result<FinancialResponseDto>.Success(MapToResponse(financial));
        }

        public async Task<Result<FinancialResponseDto>> UpdateFinancialAsync(Guid financialId, string airlineId, UpdateFinancialRequestDto dto, string? userId = null, string? userEmail = null, string? userName = null, string? ipAddress = null)
        {
            var financial = await _repository.GetAll<Financial>()
                .Include(f => f.Shipment)
                .FirstOrDefaultAsync(f => f.Id == financialId && f.AirlineId.ToString() == airlineId);

            if (financial == null)
                return Result.Failure<FinancialResponseDto>(new[] { new Error("Financial.NotFound", "Financial record not found.") });

            var oldValues = new Dictionary<string, object?>
            {
                { "MAWB", financial.MAWB },
                { "DateOfIssue", financial.DateOfIssue },
                { "TotalChargeNGN", financial.TotalChargeNGN },
                { "AmtDueAirline", financial.AmtDueAirline }
            };

            financial.MAWB = dto.MAWB;
            financial.DateOfIssue = dto.DateOfIssue;
            financial.AgentsOrClients = dto.AgentsOrClients;
            financial.Product = dto.Product;
            financial.Routing = dto.Routing;
            financial.FlightNo = dto.FlightNo;
            financial.Pieces = dto.Pieces;
            financial.ChargeableWeightKg = dto.ChargeableWeightKg;
            financial.GrossWeightKg = dto.GrossWeightKg;
            financial.SpotRate = dto.SpotRate;
            financial.PublishedRates = dto.PublishedRates;
            financial.ROE = dto.ROE;
            financial.FreightAmountNGN = dto.FreightAmountNGN;
            financial.NCAACharges5Percent = dto.NCAACharges5Percent;
            financial.TotalChargeNGN = dto.TotalChargeNGN;
            financial.ChargesCollect = dto.ChargesCollect;
            financial.FuelSurcharge = dto.FuelSurcharge;
            financial.SECSurcharge = dto.SECSurcharge;
            financial.HandlingSurcharge = dto.HandlingSurcharge;
            financial.SurchargeDueAgent = dto.SurchargeDueAgent;
            financial.AWBFee = dto.AWBFee;
            financial.GSACommissionNGN = dto.GSACommissionNGN;
            financial.VATOnCommission = dto.VATOnCommission;
            financial.AmtDueAirline = dto.AmtDueAirline;
            financial.DueAPGInc = dto.DueAPGInc;
            financial.DueSLC = dto.DueSLC;
            financial.UpdatedDate = DateTime.UtcNow;
            financial.LastUpdatedBy = string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId);

            _repository.Update(financial);
            await _unitOfWork.SaveChangesAsync();

            // Audit logging
            if (!string.IsNullOrEmpty(userId))
            {
                var newValues = new Dictionary<string, object?>
                {
                    { "MAWB", financial.MAWB },
                    { "DateOfIssue", financial.DateOfIssue },
                    { "TotalChargeNGN", financial.TotalChargeNGN },
                    { "AmtDueAirline", financial.AmtDueAirline }
                };
                var changeSummary = AuditHelper.GetChangeSummary(oldValues, newValues);

                _ = _auditService.LogAuditAsync(
                    userId: Guid.Parse(userId),
                    userName: userName ?? "Unknown",
                    userEmail: userEmail ?? "unknown@email.com",
                    action: "Update",
                    entityName: nameof(Financial),
                    entityId: financial.Id,
                    changes: changeSummary,
                    oldValues: AuditHelper.SerializeToJson(oldValues),
                    newValues: AuditHelper.SerializeToJson(newValues),
                    ipAddress: ipAddress ?? ""
                ).ConfigureAwait(false);
            }

            return Result<FinancialResponseDto>.Success(MapToResponse(financial));
        }

        public async Task<Result> DeleteFinancialAsync(Guid financialId, string airlineId, string? userId = null, string? userEmail = null, string? userName = null, string? ipAddress = null)
        {
            var financial = await _repository.GetAll<Financial>()
                .Include(f => f.Shipment)
                .FirstOrDefaultAsync(f => f.Id == financialId && f.AirlineId.ToString() == airlineId);

            if (financial == null)
                return Result.Failure(new[] { new Error("Financial.NotFound", "Financial record not found.") });

            var financialData = $"MAWB: {financial.MAWB}, Amount: NGN {financial.TotalChargeNGN}";
            
            if (financial.Shipment != null)
            {
                financial.Shipment.HasFinancial = false;
                _repository.Update(financial.Shipment);
            }

            // Soft delete: record deleted entity and mark as deleted
            var deletedEntity = SoftDeleteHelper.CreateDeletedEntity(
                financial,
                string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId),
                userEmail,
                userName,
                ipAddress
            );
            await _repository.Add(deletedEntity);

            // Mark as deleted instead of removing
            financial.IsDeleted = true;
            _repository.Update(financial);
            await _unitOfWork.SaveChangesAsync();

            // Audit logging (fire and forget - don't break main operation if audit fails)
            if (!string.IsNullOrEmpty(userId))
            {
                _ = _auditService.LogAuditAsync(
                    userId: Guid.Parse(userId),
                    userName: userName ?? "Unknown",
                    userEmail: userEmail ?? "unknown@email.com",
                    action: "Delete",
                    entityName: nameof(Financial),
                    entityId: financialId,
                    changes: $"Financial record deleted: {financialData}",
                    ipAddress: ipAddress ?? ""
                ).ConfigureAwait(false);
            }

            return Result.Success();
        }

        private FinancialResponseDto MapToResponse(Financial financial)
        {
            return new FinancialResponseDto
            {
                Id = financial.Id,
                ShipmentId = financial.ShipmentId,
                MAWB = financial.MAWB,
                DateOfIssue = financial.DateOfIssue,
                AgentsOrClients = financial.AgentsOrClients,
                Product = financial.Product,
                Routing = financial.Routing,
                FlightNo = financial.FlightNo,
                Pieces = financial.Pieces,
                ChargeableWeightKg = financial.ChargeableWeightKg,
                GrossWeightKg = financial.GrossWeightKg,
                SpotRate = financial.SpotRate,
                PublishedRates = financial.PublishedRates,
                ROE = financial.ROE,
                FreightAmountNGN = financial.FreightAmountNGN,
                NCAACharges5Percent = financial.NCAACharges5Percent,
                TotalChargeNGN = financial.TotalChargeNGN,
                ChargesCollect = financial.ChargesCollect,
                FuelSurcharge = financial.FuelSurcharge,
                SECSurcharge = financial.SECSurcharge,
                HandlingSurcharge = financial.HandlingSurcharge,
                SurchargeDueAgent = financial.SurchargeDueAgent,
                AWBFee = financial.AWBFee,
                GSACommissionNGN = financial.GSACommissionNGN,
                VATOnCommission = financial.VATOnCommission,
                AmtDueAirline = financial.AmtDueAirline,
                DueAPGInc = financial.DueAPGInc,
                DueSLC = financial.DueSLC,
                CreatedDate = financial.CreatedDate,
                UpdatedDate = financial.UpdatedDate
            };
        }
    }
}