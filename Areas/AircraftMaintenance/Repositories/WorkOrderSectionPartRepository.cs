using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public class WorkOrderSectionPartRepository : GenericRepository<WorkOrderSectionPart>, IWorkOrderSectionPartRepository
    {
        public WorkOrderSectionPartRepository(FRAContext context) : base(context)
        {
        }

        public async Task<List<WorkOrderSectionPart>> GetByWorkOrderSectionIdAsync(int workOrderSectionId)
        {
            return await _context.Set<WorkOrderSectionPart>()
                .Where(x => x.WorkOrderSectionId == workOrderSectionId)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .ToListAsync();
        }
    }
}