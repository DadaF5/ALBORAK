// Areas/AircraftMaintenance/Repositories/SnagRepository.cs
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public class SnagRepository : ISnagRepository
    {
        private readonly FRAContext _context;
        public SnagRepository(FRAContext context) => _context = context;

        public async Task<Snag?> GetByIdAsync(int id) =>
            await _context.Snags.FindAsync(id);

        public async Task<Snag?> GetWithDetailsAsync(int id) =>
            await _context.Snags
                .Include(s => s.Aircraft).ThenInclude(a => a!.AcType)
                .Include(s => s.Ata)
                .Include(s => s.DiscoveryBase)
                .Include(s => s.DiscoveredDuringWorkOrder)
                .Include(s => s.LinkedWorkOrder)
                .Include(s => s.WorkOrderSnags)
                .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<IEnumerable<Snag>> GetOpenByAircraftAsync(int aircraftId) =>
            await _context.Snags
                .Where(s => s.AircraftId == aircraftId && s.Status != SnagStatus.CLOSED)
                .Include(s => s.Ata)
                .OrderByDescending(s => s.DiscoveryDate)
                .ToListAsync();

        public async Task<IEnumerable<Snag>> GetAllAsync(bool includeClosed = false)
        {
            var query = _context.Snags
                .Include(s => s.Aircraft)
                .Include(s => s.Ata)
                .AsQueryable();

            if (!includeClosed) query = query.Where(s => s.Status != SnagStatus.CLOSED);

            return await query.ToListAsync();
        }

        public async Task<Snag> AddAsync(Snag snag)
        {
            await _context.Snags.AddAsync(snag);
            return snag;
        }

        public void Update(Snag snag) => _context.Snags.Update(snag);

        // Atomic per-year sequence, same lock pattern as WorkOrder.WONumber
        public async Task<string> GetNextSnagNumberAsync(int year)
        {
            var prefix = $"AVA-{year}-";

            var lastNumber = await _context.Snags
                .Where(s => s.SnagNumber.StartsWith(prefix))
                .OrderByDescending(s => s.SnagNumber)
                .Select(s => s.SnagNumber)
                .FirstOrDefaultAsync();

            var next = 1;
            if (lastNumber != null && int.TryParse(lastNumber[prefix.Length..], out var lastSeq))
                next = lastSeq + 1;

            return $"{prefix}{next:D4}";
        }

        public async Task<Dictionary<(int AtaId, int AcTypeId), int>> GetSnagCountByAtaAndAcTypeAsync(
            DateOnly from, DateOnly to)
        {
            var raw = await _context.Snags
                .Where(s => s.DiscoveryDate >= from && s.DiscoveryDate <= to)
                .Include(s => s.Aircraft)
                .GroupBy(s => new { s.AtaId, AcTypeId = s.Aircraft!.AcTypeId })
                .Select(g => new { g.Key.AtaId, g.Key.AcTypeId, Count = g.Count() })
                .ToListAsync();

            return raw.ToDictionary(x => (x.AtaId, x.AcTypeId), x => x.Count);
        }
    }
}