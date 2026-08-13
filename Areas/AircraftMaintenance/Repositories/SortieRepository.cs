// Areas/AircraftMaintenance/Repositories/SortieRepository.cs
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Data;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public class SortieRepository : ISortieRepository
    {
        private readonly FRAContext _context;
        public SortieRepository(FRAContext context) => _context = context;

        public async Task<Dictionary<int, int>> GetAccumulatedFHByAcTypeAsync(DateOnly from, DateOnly to)
        {
            var fromDt = from.ToDateTime(TimeOnly.MinValue);
            var toDt = to.ToDateTime(TimeOnly.MaxValue);

            var result = await _context.Sorties
                .Where(s => s.IsFinalized == true && s.DurationMinutes.HasValue)
                .Where(s => (s.RealTOFF ?? s.StartTime) >= fromDt
                         && (s.RealTOFF ?? s.StartTime) <= toDt)
                .GroupBy(s => s.AcTypeId)
                .Select(g => new { AcTypeId = g.Key, TotalMinutes = g.Sum(s => s.DurationMinutes!.Value) })
                .ToDictionaryAsync(x => x.AcTypeId, x => x.TotalMinutes);

            return result;
        }
    }
}