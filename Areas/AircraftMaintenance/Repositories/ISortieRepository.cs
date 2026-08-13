// Areas/AircraftMaintenance/Repositories/ISortieRepository.cs
using FRAProject.Areas.SquadronOps.Models;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface ISortieRepository
    {
        // Sum of DurationMinutes for FINALIZED sorties whose RealTOFF (or
        // StartTime fallback) falls in [from, to], grouped by AcTypeId.
        // Unfinalized sorties are excluded — DurationMinutes is only
        // trustworthy post-Squadron-finalization per the model's own comment.
        Task<Dictionary<int, int>> GetAccumulatedFHByAcTypeAsync(DateOnly from, DateOnly to);
    }
}