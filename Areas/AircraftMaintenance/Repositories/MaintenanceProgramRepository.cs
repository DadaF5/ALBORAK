using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public class MaintenanceProgramRepository : GenericRepository<MaintenanceProgram>, IMaintenanceProgramRepository
    {
        public MaintenanceProgramRepository(FRAContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MaintenanceProgram>> GetAllWithDetailsAsync()
        {
            return await _context.Set<MaintenanceProgram>()
                .Include(x => x.AcType)
                .OrderBy(x => x.AcType!.Code)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.Code)
                .ToListAsync();
        }

        public async Task<MaintenanceProgram?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Set<MaintenanceProgram>()
                .Include(x => x.AcType)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> ExistsByCodeAsync(int acTypeId, string code, int? excludeId = null)
        {
            var normalizedCode = code.Trim().ToUpper();

            return await _context.Set<MaintenanceProgram>()
                .AnyAsync(x =>
                    x.AcTypeId == acTypeId &&
                    x.Code.ToUpper() == normalizedCode &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));
        }
    }
}