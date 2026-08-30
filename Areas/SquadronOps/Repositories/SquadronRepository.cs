// Areas/SquadronOps/Repositories/SquadronRepository.cs
using FRAProject.Areas.HR.Models;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.SquadronOps.Repositories
{
    public class SquadronRepository : GenericRepository<Squadron>, ISquadronRepository
    {
        public SquadronRepository(FRAContext context) : base(context)
        {
        }

        public async Task<(int WingId, int BaseId)?> GetScopeInfoAsync(int squadronId)
        {
            var info = await (from s in _context.Set<Squadron>()
                               join w in _context.Set<Wing>() on s.WingId equals w.Id
                               join d in _context.Set<Department>() on w.DepartmentId equals d.Id
                               where s.Id == squadronId
                               select new { WingId = w.Id, d.BaseId })
                              .FirstOrDefaultAsync();

            return info == null ? null : (info.WingId, info.BaseId);
        }

        public async Task<HashSet<int>> GetInScopeIdsAsync(IReadOnlyCollection<int> allowedBaseIds, IReadOnlyCollection<int> allowedWingIds)
        {
            var query = from s in _context.Set<Squadron>()
                        join w in _context.Set<Wing>() on s.WingId equals w.Id
                        join d in _context.Set<Department>() on w.DepartmentId equals d.Id
                        where allowedBaseIds.Contains(d.BaseId)
                        where allowedWingIds.Count == 0 || allowedWingIds.Contains(w.Id)
                        select s.Id;

            return (await query.ToListAsync()).ToHashSet();
        }
    }
}
