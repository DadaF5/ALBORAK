// Areas/AircraftMaintenance/Services/SnagStatisticsService.cs
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Services
{
    public class SnagStatisticsService : ISnagStatisticsService
    {
        private readonly IUnitOfWork _uow;
        public SnagStatisticsService(IUnitOfWork uow) => _uow = uow;

        public async Task<List<AtaMtbfDto>> GetMtbfByAtaAsync(DateOnly from, DateOnly to)
        {
            var snagCounts = await _uow.Snags.GetSnagCountByAtaAndAcTypeAsync(from, to);
            var fhByAcType = await _uow.Sorties.GetAccumulatedFHByAcTypeAsync(from, to);

            // SnagStatisticsService.cs — GetMtbfByAtaAsync(), corrected line
            var atas = (await _uow.Ata.GetAllAsync()).ToDictionary(a => a.Id);
            var acTypes = (await _uow.AcTypes.GetAllAsync()).ToDictionary(t => t.Id);

            var result = new List<AtaMtbfDto>();

            foreach (var kv in snagCounts)
            {
                var (ataId, acTypeId) = kv.Key;
                var snagCount = kv.Value;
                var accumulatedFHMinutes = fhByAcType.GetValueOrDefault(acTypeId, 0);

                result.Add(new AtaMtbfDto
                {
                    AtaId = ataId,
                    AtaLabel = atas.TryGetValue(ataId, out var ata) ? $"{ata.Code} — {ata.Name}" : "?",
                    AcTypeId = acTypeId,
                    AcTypeLabel = acTypes.TryGetValue(acTypeId, out var t) ? t.Name : "?",
                    SnagCount = snagCount,
                    AccumulatedFH = accumulatedFHMinutes,
                    //MtbfHours = snagCount == 0 ? null : (double?)(accumulatedFHMinutes / 60.0 / snagCount)
                    // SnagStatisticsService.cs — GetMtbfByAtaAsync(), one-line fix
                    MtbfHours = (snagCount == 0 || accumulatedFHMinutes == 0)
                        ? null
                        : (double?)(accumulatedFHMinutes / 60.0 / snagCount)
                });
            }

            return result.OrderBy(r => r.AtaLabel).ThenBy(r => r.AcTypeLabel).ToList();
        }

        public async Task<List<AtaMtbfDto>> GetTopOffendersAsync(DateOnly from, DateOnly to, int topN = 10)
        {
            var all = await GetMtbfByAtaAsync(from, to);
            return all.OrderByDescending(r => r.SnagCount).Take(topN).ToList();
        }

        public async Task<List<Snag>> GetRepeatDefectsAsync(int aircraftId, int ataId, int windowDays = 90)
        {
            var snags = await _uow.Snags.GetOpenByAircraftAsync(aircraftId); // includes non-closed; widen if you also want closed history here
            var cutoff = DateOnly.FromDateTime(DateTime.Today.AddDays(-windowDays));

            return snags
                .Where(s => s.AtaId == ataId && s.DiscoveryDate >= cutoff)
                .OrderByDescending(s => s.DiscoveryDate)
                .ToList();
        }
    }
}