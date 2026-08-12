using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public class WorkOrderSectionSignOffRepository : GenericRepository<WorkOrderSectionSignOff>, IWorkOrderSectionSignOffRepository
    {
        public WorkOrderSectionSignOffRepository(FRAContext context) : base(context)
        {
        }

        public async Task<List<WorkOrderSectionSignOff>> GetOrCreateCanonicalAsync(int workOrderSectionId)
        {
            var existing = await _context.Set<WorkOrderSectionSignOff>()
                .Where(x => x.WorkOrderSectionId == workOrderSectionId)
                .ToListAsync();

            var existingLevels = existing.Select(x => x.Level).ToHashSet();

            var missing = WorkOrderSectionSignOff.CanonicalLevels
                .Where(l => !existingLevels.Contains(l.Level))
                .Select(l => new WorkOrderSectionSignOff
                {
                    WorkOrderSectionId = workOrderSectionId,
                    Level = l.Level,
                    SortOrder = l.SortOrder,
                    CreatedAtUtc = DateTime.UtcNow
                })
                .ToList();

            if (missing.Any())
            {
                await _context.Set<WorkOrderSectionSignOff>().AddRangeAsync(missing);
                await _context.SaveChangesAsync();
                existing.AddRange(missing);
            }

            return existing.OrderBy(x => x.SortOrder).ToList();
        }
    }
}