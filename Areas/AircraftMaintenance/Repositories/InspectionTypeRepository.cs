using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public class InspectionTypeRepository : GenericRepository<InspectionType>, IInspectionTypeRepository
    {
        public InspectionTypeRepository(FRAContext context) : base(context)
        {
        }

        public async Task<IEnumerable<InspectionType>> GetAllWithDetailsAsync()
        {
            return await _context.Set<InspectionType>()
                .Include(x => x.AcType)
                .Include(x => x.NextInspectionType)
                .OrderBy(x => x.AcType!.Code)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.Code)
                .ToListAsync();
        }

        public async Task<InspectionType?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Set<InspectionType>()
                .Include(x => x.AcType)
                .Include(x => x.NextInspectionType)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> ExistsByCodeAsync(int acTypeId, string code, int? excludeId = null)
        {
            var normalizedCode = code.Trim().ToUpper();

            return await _context.Set<InspectionType>()
                .AnyAsync(x =>
                    x.AcTypeId == acTypeId &&
                    x.Code.ToUpper() == normalizedCode &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));
        }
    }
}