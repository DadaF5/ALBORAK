using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public class JobCardRepository : GenericRepository<JobCard>, IJobCardRepository
    {
        public JobCardRepository(FRAContext context) : base(context)
        {
        }

        public async Task<IEnumerable<JobCard>> GetAllWithDetailsAsync()
        {
            return await _context.Set<JobCard>()
                .Include(x => x.AcType)
                .OrderBy(x => x.AcType!.Code)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.CardCode)
                .ToListAsync();
        }

        public async Task<JobCard?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Set<JobCard>()
                .Include(x => x.AcType)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> ExistsByCodeAsync(int acTypeId, string cardCode, int? excludeId = null)
        {
            var normalizedCode = cardCode.Trim().ToUpper();

            return await _context.Set<JobCard>()
                .AnyAsync(x =>
                    x.AcTypeId == acTypeId &&
                    x.CardCode.ToUpper() == normalizedCode &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));
        }
    }
}