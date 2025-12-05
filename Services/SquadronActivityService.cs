using System;
using System.Linq;
using System.Threading.Tasks;
using FRAProject.Data;
using FRAProject.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Services
{
    /// <summary>
    /// Encapsulates business logic to finalize sorties and generate maintenance artifacts (flight logs / workorders).
    /// </summary>
    public class SquadronActivityService
    {
        private readonly FRAContext _context;

        public SquadronActivityService(FRAContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Finalize a sortie: create FlightLog, update aircraft and components counters and generate work orders for crossed thresholds.
        /// This method is transactional.
        /// </summary>
        public async Task<Result> CompleteSortieAsync(int sortieId, DateTime takeoffUtc, DateTime landingUtc, decimal? hobbsStart, decimal? hobbsEnd, decimal? tachStart, decimal? tachEnd, decimal? fuelUsedKg, string completedBy)
        {
            if (landingUtc <= takeoffUtc)
                return Result.Fail("Landing time must be after takeoff time.");

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var sortie = await _context.Sorties
                    .Include(s => s.Aircraft).ThenInclude(a => a.AcType)
                    .Include(s => s.Aircraft).ThenInclude(a => a.Components)
                    .ThenInclude(c => c.Thresholds)
                    .FirstOrDefaultAsync(s => s.SortieId == sortieId);

                if (sortie == null)
                    return Result.Fail("Sortie not found.");

                if (sortie.IsCompleted)
                    return Result.Fail("Sortie is already completed.");

                if (!sortie.AircraftId.HasValue)
                    return Result.Fail("Sortie must have an Aircraft assigned before completion.");

                var durationMinutes = (int)Math.Round((landingUtc - takeoffUtc).TotalMinutes);
                var cycles = 1;

                // Create FlightLog
                var flightLog = new FlightLog
                {
                    SortieId = sortie.SortieId,
                    AircraftId = sortie.AircraftId!.Value,
                    TakeOffUtc = takeoffUtc,
                    LandingUtc = landingUtc,
                    DurationMinutes = durationMinutes,
                    Cycles = cycles,
                    HobbsStart = hobbsStart,
                    HobbsEnd = hobbsEnd,
                    TachStart = tachStart,
                    TachEnd = tachEnd,
                    FuelUsedKg = fuelUsedKg,
                    Notes = sortie.Notes,
                    CreatedBy = completedBy
                };
                _context.FlightLogs.Add(flightLog);

                // Update sortie completion metadata
                sortie.IsCompleted = true;
                sortie.CompletedAtUtc = DateTime.UtcNow;
                sortie.CompletedBy = completedBy;

                // Update aircraft totals
                var aircraft = sortie.Aircraft!;
                //aircraft.TotalMinutes += durationMinutes;
                //aircraft.TotalCycles += cycles;

                // Update components and evaluate thresholds
                foreach (var comp in aircraft.Components ?? Enumerable.Empty<MaintenanceComponent>())
                {
                    comp.TotalMinutes += durationMinutes;
                    comp.TotalCycles += cycles;
                    comp.LastUpdatedUtc = DateTime.UtcNow;

                    foreach (var thr in comp.Thresholds ?? Enumerable.Empty<MaintenanceThreshold>())
                    {
                        var triggered = false;
                        if (string.Equals(thr.ThresholdType, "Minutes", StringComparison.OrdinalIgnoreCase))
                        {
                            if (comp.TotalMinutes >= thr.Value && (thr.LastTriggeredUtc == null || (comp.TotalMinutes - durationMinutes) < thr.Value))
                                triggered = true;
                        }
                        else if (string.Equals(thr.ThresholdType, "Cycles", StringComparison.OrdinalIgnoreCase))
                        {
                            if (comp.TotalCycles >= thr.Value && (thr.LastTriggeredUtc == null || (comp.TotalCycles - cycles) < thr.Value))
                                triggered = true;
                        }

                        if (triggered)
                        {
                            var wo = new MaintenanceWorkOrder
                            {
                                AircraftId = aircraft.Id,
                                ComponentId = comp.Id,
                                ThresholdId = thr.Id,
                                Title = $"Auto: maintenance for {comp.PartNumber} - threshold {thr.Value} {thr.ThresholdType}",
                                Description = $"Auto-generated after sortie completion. Component total minutes: {comp.TotalMinutes}, cycles: {comp.TotalCycles}",
                                Status = "Open",
                                TriggeredTotalMinutes = comp.TotalMinutes,
                                TriggeredTotalCycles = comp.TotalCycles,
                                CreatedAtUtc = DateTime.UtcNow
                            };
                            _context.MaintenanceWorkOrders.Add(wo);

                            thr.LastTriggeredUtc = DateTime.UtcNow;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return Result.Fail("Error completing sortie: " + ex.Message);
            }
        }
    }

    // simple result helper
    public class Result
    {
        public bool Success { get; private set; }
        public string? Error { get; private set; }

        public static Result Ok() => new Result { Success = true };
        public static Result Fail(string error) => new Result { Success = false, Error = error };
    }
}
