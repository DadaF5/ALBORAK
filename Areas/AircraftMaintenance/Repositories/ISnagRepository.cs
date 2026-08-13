// Areas/AircraftMaintenance/Repositories/ISnagRepository.cs (widened per last session's note)
using FRAProject.Areas.AircraftMaintenance.Models;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface ISnagRepository
    {
        Task<Snag?> GetByIdAsync(int id);
        Task<Snag?> GetWithDetailsAsync(int id);
        Task<IEnumerable<Snag>> GetOpenByAircraftAsync(int aircraftId);
        Task<IEnumerable<Snag>> GetAllAsync(bool includeClosed = false);
        Task<Snag> AddAsync(Snag snag);
        void Update(Snag snag);

        Task<string> GetNextSnagNumberAsync(int year);

        // Widened: groups by (AtaId, AcTypeId) via Snag -> Aircraft -> AcTypeId,
        // to match Sortie's AcTypeId-keyed FH denominator.
        Task<Dictionary<(int AtaId, int AcTypeId), int>> GetSnagCountByAtaAndAcTypeAsync(
            DateOnly from, DateOnly to);
    }
}