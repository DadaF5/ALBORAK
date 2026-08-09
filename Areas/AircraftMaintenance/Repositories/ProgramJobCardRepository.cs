using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public class ProgramJobCardRepository : GenericRepository<ProgramJobCard>, IProgramJobCardRepository
    {
        public ProgramJobCardRepository(FRAContext context) : base(context)
        {
        }

        public async Task<List<ProgramJobCard>> GetByProgramIdWithDetailsAsync(int maintenanceProgramId)
        {
            return await _context.Set<ProgramJobCard>()
                .Include(x => x.JobCard)
                .Where(x => x.MaintenanceProgramId == maintenanceProgramId)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.JobCard!.CardCode)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int maintenanceProgramId, int jobCardId)
        {
            return await _context.Set<ProgramJobCard>()
                .AnyAsync(x => x.MaintenanceProgramId == maintenanceProgramId && x.JobCardId == jobCardId);
        }
    }
}