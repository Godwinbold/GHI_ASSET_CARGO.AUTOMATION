using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Executive;
using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GHI_ASSET_CARGO.Core.Services
{
    public class ExecutiveService : IExecutiveService
    {
        private readonly IRepository _repository;
        public ExecutiveService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<ExecutiveDashboardResponseDto>> GetExecutiveDashboardAsync(ExecutiveDashboardRequestDto request)
        {
            // Shipments query with optional filters
            var shipmentsQuery = _repository.GetAll<Shipment>().AsQueryable();

            if (request.StartDate.HasValue)
                shipmentsQuery = shipmentsQuery.Where(s => s.ShipmentDate >= request.StartDate.Value);
            if (request.EndDate.HasValue)
                shipmentsQuery = shipmentsQuery.Where(s => s.ShipmentDate <= request.EndDate.Value);
            if (request.AirlineId.HasValue)
                shipmentsQuery = shipmentsQuery.Where(s => s.AirlineId == request.AirlineId.Value);

            var totalShipments = await shipmentsQuery.CountAsync();

            // Financials query to compute weights and amounts
            var financialsQuery = _repository.GetAll<Financial>()
                .Include(f => f.Shipment)
                .AsQueryable();

            if (request.StartDate.HasValue)
                financialsQuery = financialsQuery.Where(f => f.Shipment.ShipmentDate >= request.StartDate.Value);
            if (request.EndDate.HasValue)
                financialsQuery = financialsQuery.Where(f => f.Shipment.ShipmentDate <= request.EndDate.Value);
            if (request.AirlineId.HasValue)
                financialsQuery = financialsQuery.Where(f => f.AirlineId == request.AirlineId.Value);

            var financials = await financialsQuery.ToListAsync();

            var totalRevenue = financials.Sum(f => f.FreightAmountNGN ?? 0m);
            var totalWeight = financials.Sum(f => f.GrossWeightKg ?? 0m);
            var avgWeight = totalShipments > 0 ? (decimal)totalWeight / Math.Max(1, totalShipments) : 0m;

            // Cargo unit summary per airline
            var airlineIds = await _repository.GetAll<Airline>().Select(a => new { a.Id, a.AirlineName }).ToListAsync();

            var shipmentsByAirline = await shipmentsQuery
                .GroupBy(s => s.AirlineId)
                .Select(g => new { AirlineId = g.Key, Total = g.Count() })
                .ToListAsync();

            var financialsByAirline = financials
                .GroupBy(f => f.AirlineId)
                .Select(g => new
                {
                    AirlineId = g.Key,
                    TotalWeight = g.Sum(x => x.GrossWeightKg ?? 0m),
                    TotalAmount = g.Sum(x => x.FreightAmountNGN ?? 0m)
                })
                .ToList();

            var cargoUnits = airlineIds
                .Where(a => !request.AirlineId.HasValue || a.Id == request.AirlineId.Value)
                .Select(a =>
                {
                    var ship = shipmentsByAirline.FirstOrDefault(x => x.AirlineId == a.Id);
                    var fin = financialsByAirline.FirstOrDefault(x => x.AirlineId == a.Id);
                    return new CargoUnitSummaryDto
                    {
                        AirlineId = a.Id,
                        AirlineName = a.AirlineName,
                        TotalShipments = ship?.Total ?? 0,
                        TotalWeightKg = fin?.TotalWeight ?? 0m,
                        TotalAmount = fin?.TotalAmount ?? 0m
                    };
                })
                .OrderByDescending(x => x.TotalShipments)
                .ToList();

            var response = new ExecutiveDashboardResponseDto
            {
                CargoUnits = cargoUnits,
                FilteredResult = new ExecutiveMetricsDto
                {
                    TotalShipments = totalShipments,
                    TotalRevenue = totalRevenue,
                    AverageWeightKg = avgWeight
                }
            };

            return Result<ExecutiveDashboardResponseDto>.Success(response);
        }
    }
}
