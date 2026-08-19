using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public class WorkSectionRepository : GenericRepository<WorkSection>, IWorkSectionRepository
    {
        public WorkSectionRepository(FRAContext context) : base(context)
        {
        }

        public async Task<List<WorkSection>> GetAllWithDetailsAsync()
        {
            return await _context.Set<WorkSection>()
                .Include(x => x.AcMainGroup)
                .OrderBy(x => x.AcMainGroup!.Code)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.Code)
                .ToListAsync();
        }

        public async Task<bool> ExistsByCodeAsync(int acMainGroupId, string code, int? excludeId = null)
        {
            var normalized = code.Trim().ToUpper();

            return await _context.Set<WorkSection>()
                .AnyAsync(x =>
                    x.AcMainGroupId == acMainGroupId &&
                    x.Code.ToUpper() == normalized &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));
        }
    }
}
