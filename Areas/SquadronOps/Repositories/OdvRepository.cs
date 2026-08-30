// Areas/SquadronOps/Repositories/OdvRepository.cs
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.SquadronOps.Repositories
{
    public class OdvRepository : GenericRepository<Odv>, IOdvRepository
    {
        public OdvRepository(FRAContext context) : base(context)
        {
        }

        public async Task<List<Odv>> GetBoardForDateAsync(DateTime date, HashSet<int>? allowedSquadronIds, HashSet<int>? allowedAcMainGroupIds)
        {
            var query = _context.Set<Odv>()
                .Include(o => o.Mission)
                .Include(o => o.AcMainGroup)
                .Include(o => o.CallSign)
                .Include(o => o.Sorties!)
                    .ThenInclude(s => s.AcType)
                .Include(o => o.Sorties!)
                    .ThenInclude(s => s.SortieCrews)
                        .ThenInclude(sc => sc.CrewMember)
                .Where(o => o.OdvDate == date)
                .AsNoTracking()
                .AsQueryable();

            if (allowedSquadronIds != null)
                query = query.Where(o => allowedSquadronIds.Contains(o.SquadronId));

            if (allowedAcMainGroupIds != null && allowedAcMainGroupIds.Count > 0)
                query = query.Where(o => allowedAcMainGroupIds.Contains(o.AcMainGroupId));

            return await query.OrderBy(o => o.TOFF).ToListAsync();
        }

        public async Task<Odv?> GetByIdWithSortiesAsync(int id)
        {
            return await _context.Set<Odv>()
                .Include(o => o.Sorties)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
