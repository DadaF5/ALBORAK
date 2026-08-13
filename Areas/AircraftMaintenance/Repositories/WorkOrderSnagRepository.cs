// Areas/AircraftMaintenance/Repositories/WorkOrderSnagRepository.cs
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public class WorkOrderSnagRepository : IWorkOrderSnagRepository
    {
        private readonly FRAContext _context;
        public WorkOrderSnagRepository(FRAContext context) => _context = context;

        public async Task<WorkOrderSnag> AddAsync(WorkOrderSnag link)
        {
            await _context.WorkOrderSnags.AddAsync(link);
            return link;
        }

        public async Task<IEnumerable<WorkOrderSnag>> GetByWorkOrderAsync(int workOrderId) =>
            await _context.WorkOrderSnags
                .Where(w => w.WorkOrderId == workOrderId)
                .Include(w => w.Snag)
                .ToListAsync();

        public async Task<IEnumerable<WorkOrderSnag>> GetBySnagAsync(int snagId) =>
            await _context.WorkOrderSnags
                .Where(w => w.SnagId == snagId)
                .Include(w => w.WorkOrder)
                .ToListAsync();
    }
}