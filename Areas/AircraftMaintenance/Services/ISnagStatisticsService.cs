// Areas/AircraftMaintenance/Services/ISnagStatisticsService.cs
using FRAProject.Areas.AircraftMaintenance.Models;

namespace FRAProject.Areas.AircraftMaintenance.Services
{
    public class AtaMtbfDto
    {
        public int AtaId { get; set; }
        public string AtaLabel { get; set; } = null!;
        public int AcTypeId { get; set; }
        public string AcTypeLabel { get; set; } = null!;
        public int SnagCount { get; set; }
        public int AccumulatedFH { get; set; }        // minutes
        public double? MtbfHours { get; set; }         // null if SnagCount == 0 (undefined, not zero)
    }

    public interface ISnagStatisticsService
    {
        // MTBF per ATA chapter per AcType, over a period
        Task<List<AtaMtbfDto>> GetMtbfByAtaAsync(DateOnly from, DateOnly to);

        // Top-N ATA chapters by snag count — "top offenders" view, standard reliability-board output
        Task<List<AtaMtbfDto>> GetTopOffendersAsync(DateOnly from, DateOnly to, int topN = 10);

        // Repeat-defect detection: same Aircraft + same Ata within a rolling window
        // (standard reliability-program flag — recurring defect on the same system/airframe)
        Task<List<Snag>> GetRepeatDefectsAsync(int aircraftId, int ataId, int windowDays = 90);
    }
}