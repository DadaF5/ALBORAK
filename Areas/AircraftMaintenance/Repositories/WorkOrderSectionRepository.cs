using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public class WorkOrderSectionRepository : GenericRepository<WorkOrderSection>, IWorkOrderSectionRepository
    {
        public WorkOrderSectionRepository(FRAContext context) : base(context)
        {
        }

        public async Task<List<WorkOrderSection>> GetByWorkOrderIdWithDetailsAsync(int workOrderId)
        {
            return await _context.Set<WorkOrderSection>()
                .Include(x => x.WorkSection)
                .Where(x => x.WorkOrderId == workOrderId)
                .OrderBy(x => x.WorkSection!.Code)
                .ToListAsync();
        }

        public async Task<WorkOrderSection?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Set<WorkOrderSection>()
                .Include(x => x.WorkSection)
                .Include(x => x.WorkOrder).ThenInclude(w => w!.Aircraft)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}