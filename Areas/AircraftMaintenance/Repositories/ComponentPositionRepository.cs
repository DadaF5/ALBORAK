using Microsoft.EntityFrameworkCore;
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Infrastructure.Repositories;
using FRAProject.Data;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IComponentPositionRepository : IGenericRepository<ComponentPosition>
    {
        Task<List<ComponentPosition>> GetByAcTypeAsync(int acTypeId, bool includeInactive = false);
        Task<bool> ExistsByCodeAsync(int acTypeId, string code, int? excludeId = null);
        /// <summary>NEW — every active position across every AcType, with AcType eagerly loaded (base GetAllAsync doesn't Include navigation properties), for the ComponentType "Positions éligibles" picker.</summary>
        Task<List<ComponentPosition>> GetAllActiveWithAcTypeAsync();
        /// <summary>
        /// NEW — fixes a real bug: Index.cshtml and Tree.cshtml both read
        /// p.AcType?.Name / p.Ata?.Name, but both actions were fetching via
        /// the base GetAllAsync() (no Include), and this app doesn't use
        /// lazy-loading proxies (confirmed against the real AcType.cs:
        /// "No virtual — EF Core uses explicit Include(), not lazy
        /// loading."), so both columns rendered blank — confirmed live via
        /// Dadda's screenshot of Index showing an empty "Type d'aéronef"
        /// column. Includes BOTH active and inactive rows (unlike
        /// GetAllActiveWithAcTypeAsync above) — Index/Tree do their own
        /// includeInactive filtering in C#, same as before.
        /// </summary>
        Task<List<ComponentPosition>> GetAllWithAcTypeAndAtaAsync();
    }

    public class ComponentPositionRepository : GenericRepository<ComponentPosition>, IComponentPositionRepository
    {
        public ComponentPositionRepository(FRAContext context) : base(context) { }

        public async Task<List<ComponentPosition>> GetByAcTypeAsync(int acTypeId, bool includeInactive = false)
        {
            var query = _context.Set<ComponentPosition>().Where(p => p.AcTypeId == acTypeId);
            if (!includeInactive) query = query.Where(p => p.IsActive);
            return await query.OrderBy(p => p.SortOrder).ThenBy(p => p.Name).ToListAsync();
        }

        public async Task<List<ComponentPosition>> GetAllActiveWithAcTypeAsync()
        {
            return await _context.Set<ComponentPosition>()
                .Include(p => p.AcType)
                .Where(p => p.IsActive)
                .OrderBy(p => p.AcType!.Code).ThenBy(p => p.SortOrder).ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<List<ComponentPosition>> GetAllWithAcTypeAndAtaAsync()
        {
            return await _context.Set<ComponentPosition>()
                .Include(p => p.AcType)
                .Include(p => p.Ata)
                .OrderBy(p => p.AcType!.Code).ThenBy(p => p.SortOrder).ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsByCodeAsync(int acTypeId, string code, int? excludeId = null)
        {
            var query = _context.Set<ComponentPosition>().Where(p => p.AcTypeId == acTypeId && p.Code == code);
            if (excludeId.HasValue) query = query.Where(p => p.Id != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}
