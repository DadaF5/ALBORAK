using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public class WorkOrderRepository : GenericRepository<WorkOrder>, IWorkOrderRepository
    {
        public WorkOrderRepository(FRAContext context) : base(context)
        {
        }

        public async Task<List<WorkOrder>> GetAllWithDetailsAsync()
        {
            return await _context.Set<WorkOrder>()
                .Include(w => w.Aircraft)
                .OrderByDescending(w => w.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task<WorkOrder?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Set<WorkOrder>()
            .Include(w => w.Aircraft).ThenInclude(a => a!.AcType)!.ThenInclude(t => t!.AircraftManufacturer)
            .Include(w => w.WorkOrderInspectionTypes).ThenInclude(wit => wit.InspectionType)
            .Include(w => w.WorkOrderJobCards).ThenInclude(wjc => wjc.JobCard)
            .Include(w => w.WorkOrderJobCards).ThenInclude(wjc => wjc.MaintenanceProgram)
            .FirstOrDefaultAsync(w => w.Id == id);
        }

        // NOTE: best-effort sequential numbering, NOT safe under concurrent
        // creation (two simultaneous Create calls could get the same
        // number). Fine for current single-admin dev usage; if multiple
        // planners create WOs concurrently in production, replace with a
        // dedicated counter/sequence table.
        public async Task<string> GenerateNextWONumberAsync(int year)
        {
            var prefix = $"OT-{year}-";
            var count = await _context.Set<WorkOrder>()
                .CountAsync(w => w.WONumber.StartsWith(prefix));

            return $"{prefix}{(count + 1):D4}";
        }
        public async Task<HashSet<int>> GetActiveInspectionTypeIdsForAircraftAsync(int aircraftId)
        {
            var ids = await _context.Set<WorkOrder>()
                .Where(w => w.AircraftId == aircraftId && w.Status != "CLOSED")
                .SelectMany(w => w.WorkOrderInspectionTypes.Select(wit => wit.InspectionTypeId))
                .ToListAsync();

            return ids.ToHashSet();
        }

    }
}