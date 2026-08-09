using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public class InspectionTypeProgramRepository : GenericRepository<InspectionTypeProgram>, IInspectionTypeProgramRepository
    {
        public InspectionTypeProgramRepository(FRAContext context) : base(context)
        {
        }

        public async Task<List<InspectionTypeProgram>> GetByInspectionTypeIdsAsync(List<int> inspectionTypeIds)
        {
            return await _context.Set<InspectionTypeProgram>()
                .Include(x => x.MaintenanceProgram)
                .Where(x => inspectionTypeIds.Contains(x.InspectionTypeId))
                .ToListAsync();
        }
    }
}