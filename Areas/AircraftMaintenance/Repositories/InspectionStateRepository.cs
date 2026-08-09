using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public class InspectionStateRepository : GenericRepository<InspectionState>, IInspectionStateRepository
    {
        public InspectionStateRepository(FRAContext context) : base(context)
        {
        }

        public async Task<InspectionState?> GetByAircraftAndTypeAsync(int aircraftId, int inspectionTypeId)
        {
            return await _context.Set<InspectionState>()
                .FirstOrDefaultAsync(s => s.AircraftId == aircraftId && s.InspectionTypeId == inspectionTypeId);
        }

        public async Task<List<InspectionState>> GetAllWithDetailsAsync()
        {
            return await _context.Set<InspectionState>()
                .Include(s => s.Aircraft)
                .Include(s => s.InspectionType)
                .ToListAsync();
        }
    }
}