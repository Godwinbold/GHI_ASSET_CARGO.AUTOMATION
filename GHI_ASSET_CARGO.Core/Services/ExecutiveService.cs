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

        public async Task<Result<AnalyticalResponseDto>> GetAnalyticalAsync(ExecutiveDashboardRequestDto request)
        {
            // Define date ranges for current and previous periods
            DateTimeOffset currentStartDate, currentEndDate, previousStartDate, previousEndDate;

            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                currentStartDate = request.StartDate.Value;
                currentEndDate = request.EndDate.Value;
                var dateDifference = currentEndDate - currentStartDate;
                previousStartDate = currentStartDate.AddDays(-(int)dateDifference.TotalDays);
                previousEndDate = currentStartDate.AddSeconds(-1);
            }
            else
            {
                // Default to last 12 months for current and previous 12 months before that
                currentEndDate = DateTimeOffset.UtcNow;
                currentStartDate = currentEndDate.AddMonths(-12);
                previousEndDate = currentStartDate.AddSeconds(-1);
                previousStartDate = previousEndDate.AddMonths(-12);
            }

            // Get current period metrics
            var currentShipmentsQuery = _repository.GetAll<Shipment>()
                .Where(s => s.ShipmentDate >= currentStartDate && s.ShipmentDate <= currentEndDate);

            if (request.AirlineId.HasValue)
                currentShipmentsQuery = currentShipmentsQuery.Where(s => s.AirlineId == request.AirlineId.Value);

            var currentTotalShipments = await currentShipmentsQuery.CountAsync();

            var currentFinancialsQuery = _repository.GetAll<Financial>()
                .Include(f => f.Shipment)
                .Where(f => f.Shipment.ShipmentDate >= currentStartDate && f.Shipment.ShipmentDate <= currentEndDate);

            if (request.AirlineId.HasValue)
                currentFinancialsQuery = currentFinancialsQuery.Where(f => f.AirlineId == request.AirlineId.Value);

            var currentFinancials = await currentFinancialsQuery.ToListAsync();
            var currentTotalRevenue = currentFinancials.Sum(f => f.FreightAmountNGN ?? 0m);
            var currentTotalWeight = currentFinancials.Sum(f => f.GrossWeightKg ?? 0m);
            var currentAvgWeight = currentTotalShipments > 0 ? currentTotalWeight / (decimal)currentTotalShipments : 0m;

            // Get previous period metrics
            var previousShipmentsQuery = _repository.GetAll<Shipment>()
                .Where(s => s.ShipmentDate >= previousStartDate && s.ShipmentDate <= previousEndDate);

            if (request.AirlineId.HasValue)
                previousShipmentsQuery = previousShipmentsQuery.Where(s => s.AirlineId == request.AirlineId.Value);

            var previousTotalShipments = await previousShipmentsQuery.CountAsync();

            var previousFinancialsQuery = _repository.GetAll<Financial>()
                .Include(f => f.Shipment)
                .Where(f => f.Shipment.ShipmentDate >= previousStartDate && f.Shipment.ShipmentDate <= previousEndDate);

            if (request.AirlineId.HasValue)
                previousFinancialsQuery = previousFinancialsQuery.Where(f => f.AirlineId == request.AirlineId.Value);

            var previousFinancials = await previousFinancialsQuery.ToListAsync();
            var previousTotalRevenue = previousFinancials.Sum(f => f.FreightAmountNGN ?? 0m);
            var previousTotalWeight = previousFinancials.Sum(f => f.GrossWeightKg ?? 0m);
            var previousAvgWeight = previousTotalShipments > 0 ? previousTotalWeight / (decimal)previousTotalShipments : 0m;

            // Calculate growth metrics
            var shipmentsGrowth = currentTotalShipments - previousTotalShipments;
            var shipmentsGrowthPercent = previousTotalShipments > 0 ? (shipmentsGrowth / (decimal)previousTotalShipments) * 100 : 0;

            var revenueGrowth = currentTotalRevenue - previousTotalRevenue;
            var revenueGrowthPercent = previousTotalRevenue > 0 ? (revenueGrowth / previousTotalRevenue) * 100 : 0;

            var weightGrowth = currentAvgWeight - previousAvgWeight;
            var weightGrowthPercent = previousAvgWeight > 0 ? (weightGrowth / previousAvgWeight) * 100 : 0;

            // Get monthly data for charts
            var monthlyData = await currentShipmentsQuery
                .GroupBy(s => new { s.ShipmentDate.Year, s.ShipmentDate.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                .ToListAsync();

            var monthlyFinancials = currentFinancials
                .GroupBy(f => new { f.Shipment.ShipmentDate.Year, f.Shipment.ShipmentDate.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Revenue = g.Sum(x => x.FreightAmountNGN ?? 0m) })
                .ToList();

            var monthlyChartData = monthlyData
                .Select(m => new MonthlyDataDto
                {
                    Month = new DateTime(m.Year, m.Month, 1),
                    ShipmentCount = m.Count,
                    Revenue = monthlyFinancials.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.Revenue ?? 0m
                })
                .OrderBy(x => x.Month)
                .ToList();

            // Get cargo unit summary per airline
            var airlineIds = await _repository.GetAll<Airline>().Select(a => new { a.Id, a.AirlineName }).ToListAsync();

            var shipmentsByAirline = await currentShipmentsQuery
                .GroupBy(s => s.AirlineId)
                .Select(g => new { AirlineId = g.Key, Total = g.Count() })
                .ToListAsync();

            var financialsByAirline = currentFinancials
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

            var response = new AnalyticalResponseDto
            {
                TotalShipments = new MetricWithComparisonDto
                {
                    CurrentValue = currentTotalShipments,
                    PreviousValue = previousTotalShipments,
                    GrowthValue = shipmentsGrowth,
                    GrowthPercentage = (decimal)shipmentsGrowthPercent
                },
                TotalRevenue = new MetricWithComparisonDto
                {
                    CurrentValue = currentTotalRevenue,
                    PreviousValue = previousTotalRevenue,
                    GrowthValue = revenueGrowth,
                    GrowthPercentage = (decimal)revenueGrowthPercent
                },
                AverageWeight = new MetricWithComparisonDto
                {
                    CurrentValue = currentAvgWeight,
                    PreviousValue = previousAvgWeight,
                    GrowthValue = weightGrowth,
                    GrowthPercentage = (decimal)weightGrowthPercent
                },
                MonthlyData = monthlyChartData,
                CargoUnits = cargoUnits
            };

            return Result<AnalyticalResponseDto>.Success(response);
        }
    }
}
