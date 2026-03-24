using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Financial;
using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHI_ASSET_CARGO.Core.Services
{
    public class FinancialService : IFinancialService
    {
        private readonly IRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public FinancialService(IRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<FinancialResponseDto>> GetFinancialByShipmentIdAsync(Guid shipmentId, string airlineId)
        {
            var financial = await _repository.GetAll<Financial>()
                .Include(f => f.Shipment)
                .FirstOrDefaultAsync(f => f.ShipmentId == shipmentId && f.AirlineId.ToString() == airlineId);

            if (financial == null)
                return Result.Failure<FinancialResponseDto>(new[] { new Error("Financial.NotFound", "Financial record not found for this shipment.") });

            return Result<FinancialResponseDto>.Success(MapToResponse(financial));
        }

        public async Task<Result<FinancialResponseDto>> GetFinancialByIdAsync(Guid financialId, string airlineId)
        {
            var financial = await _repository.GetAll<Financial>()
                .Include(f => f.Shipment)
                .FirstOrDefaultAsync(f => f.Id == financialId && f.AirlineId.ToString() == airlineId);

            if (financial == null)
                return Result.Failure<FinancialResponseDto>(new[] { new Error("Financial.NotFound", "Financial record not found.") });

            return Result<FinancialResponseDto>.Success(MapToResponse(financial));
        }

        public async Task<Result<FinancialResponseDto>> CreateFinancialAsync(Guid shipmentId, string airlineId, CreateFinancialRequestDto dto)
        {
            // Check if shipment exists and belongs to airline
            var shipment = await _repository.FindById<Shipment>(shipmentId);
            if (shipment == null || shipment.AirlineId.ToString() != airlineId)
                return Result.Failure<FinancialResponseDto>(new[] { new Error("Shipment.NotFound", "Shipment not found or does not belong to the airline.") });

            // Check if financial already exists for this shipment
            var existing = await _repository.GetAll<Financial>().AnyAsync(f => f.ShipmentId == shipmentId);
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
                DueSLC = dto.DueSLC
            };

            await _repository.Add(financial);
            await _unitOfWork.SaveChangesAsync();

            return Result<FinancialResponseDto>.Success(MapToResponse(financial));
        }

        public async Task<Result<FinancialResponseDto>> UpdateFinancialAsync(Guid financialId, string airlineId, UpdateFinancialRequestDto dto)
        {
            var financial = await _repository.GetAll<Financial>()
                .Include(f => f.Shipment)
                .FirstOrDefaultAsync(f => f.Id == financialId && f.AirlineId.ToString() == airlineId);

            if (financial == null)
                return Result.Failure<FinancialResponseDto>(new[] { new Error("Financial.NotFound", "Financial record not found.") });

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

            _repository.Update(financial);
            await _unitOfWork.SaveChangesAsync();

            return Result<FinancialResponseDto>.Success(MapToResponse(financial));
        }

        public async Task<Result> DeleteFinancialAsync(Guid financialId, string airlineId)
        {
            var financial = await _repository.GetAll<Financial>()
                .Include(f => f.Shipment)
                .FirstOrDefaultAsync(f => f.Id == financialId && f.AirlineId.ToString() == airlineId);

            if (financial == null)
                return Result.Failure(new[] { new Error("Financial.NotFound", "Financial record not found.") });

            _repository.Remove(financial);
            await _unitOfWork.SaveChangesAsync();

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