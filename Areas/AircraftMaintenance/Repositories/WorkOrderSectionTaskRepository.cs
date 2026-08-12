using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public class WorkOrderSectionTaskRepository : GenericRepository<WorkOrderSectionTask>, IWorkOrderSectionTaskRepository
    {
        public WorkOrderSectionTaskRepository(FRAContext context) : base(context)
        {
        }

        public async Task<List<WorkOrderSectionTask>> GetByWorkOrderSectionIdAsync(int workOrderSectionId)
        {
            return await _context.Set<WorkOrderSectionTask>()
                .Where(x => x.WorkOrderSectionId == workOrderSectionId)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .ToListAsync();
        }
    }
}