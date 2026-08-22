using Microsoft.EntityFrameworkCore;
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Infrastructure.Repositories;
using FRAProject.Data;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IComponentRepository : IGenericRepository<Component>
    {
        Task<Component?> GetWithDetailsAsync(int id);
        Task<List<Component>> GetByAircraftAsync(int aircraftId);
        Task<List<Component>> GetByStockBaseAsync(int baseId, bool includeUnderRepair = true);
        Task<bool> ExistsSerialAsync(int componentTypeId, string serialNumber, int? excludeId = null);
        /// <summary>All Components currently Installed — base query the scope filter is applied on top of (IsAircraftInScopeAsync per row) plus all InStock/UnderRepair rows filtered by StockBaseId membership. See ComponentService for the actual scope application — this just returns the raw candidate set.</summary>
        Task<List<Component>> GetAllWithCurrentLocationAsync();

        /// <summary>NEW — direct children (one level, not recursive) of a parent Component, eager-loaded for tree display.</summary>
        Task<List<Component>> GetChildrenAsync(int parentComponentId);

        /// <summary>
        /// NEW — walks ParentComponentId up from the given Component and
        /// returns every ancestor, NEAREST PARENT FIRST, ULTIMATE ROOT LAST
        /// (e.g. for a Fuel-Pump-on-DEEC-on-Engine chain, calling this on the
        /// Fuel Pump returns [DEEC, Engine]). Returns self only in the sense
        /// that it never includes the starting Component itself — empty list
        /// if the given Component has no parent (it already is a root).
        /// Callers resolving effective aircraft/location use the LAST entry
        /// (chain[^1]) as the ultimate root.
        /// </summary>
        Task<List<Component>> GetAncestorChainAsync(int componentId);
    }

    public class ComponentRepository : GenericRepository<Component>, IComponentRepository
    {
        public ComponentRepository(FRAContext context) : base(context) { }

        public async Task<Component?> GetWithDetailsAsync(int id)
        {
            return await _context.Set<Component>()
                .Include(c => c.ComponentType).ThenInclude(t => t!.LifeLimitProfiles).ThenInclude(p => p.Stages).ThenInclude(s => s.Dimensions).ThenInclude(d => d.DimensionType) // NEW (Revision 13) — generic per-stage dimensions
                .Include(c => c.CurrentAircraft)
                .Include(c => c.CurrentPosition)
                .Include(c => c.StockBase)
                .Include(c => c.ComponentLifeStatus).ThenInclude(s => s!.MatchedLifeLimitProfile)
                .Include(c => c.ComponentLifeStatus).ThenInclude(s => s!.Dimensions).ThenInclude(d => d.DimensionType) // NEW (Revision 13)
                .Include(c => c.ParentComponent).ThenInclude(p => p!.ComponentType)
                .Include(c => c.ChildComponents).ThenInclude(ch => ch.ComponentType)
                .Include(c => c.ChildComponents).ThenInclude(ch => ch.ComponentLifeStatus).ThenInclude(s => s!.DrivingDimensionType) // fixed — Details.cshtml's no-slot-defined fallback reads child.ComponentLifeStatus directly off this collection; was missing before Revision 13 too, caught during this rewrite's review
                .Include(c => c.InitialReading).ThenInclude(r => r!.Values).ThenInclude(v => v.DimensionType) // NEW (Revision 12/13) — RecomputeAsync needs this loaded to seed opening counters, now per-dimension
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Component>> GetChildrenAsync(int parentComponentId)
        {
            return await _context.Set<Component>()
                .Include(c => c.ComponentType)
                .Include(c => c.ComponentLifeStatus).ThenInclude(s => s!.DrivingDimensionType) // NEW (Revision 13)
                .Where(c => c.ParentComponentId == parentComponentId)
                .OrderBy(c => c.ComponentType!.PartNumber)
                .ToListAsync();
        }

        public async Task<List<Component>> GetAncestorChainAsync(int componentId)
        {
            var chain = new List<Component>();
            var current = await _context.Set<Component>().FirstOrDefaultAsync(c => c.Id == componentId);
            // Guard against a corrupt cycle (should be impossible — AttachToParentAsync
            // rejects any attach that would create one) so this can never infinite-loop.
            var visited = new HashSet<int> { componentId };

            while (current?.ParentComponentId is int parentId)
            {
                if (!visited.Add(parentId)) break;

                var parent = await _context.Set<Component>()
                    .Include(c => c.ComponentType)
                    .Include(c => c.CurrentAircraft)
                    .Include(c => c.CurrentPosition)
                    .Include(c => c.StockBase)
                    .FirstOrDefaultAsync(c => c.Id == parentId);
                if (parent == null) break;

                chain.Add(parent);
                current = parent;
            }

            return chain;
        }

        public async Task<List<Component>> GetByAircraftAsync(int aircraftId)
        {
            return await _context.Set<Component>()
                .Include(c => c.ComponentType)
                .Include(c => c.CurrentPosition)
                .Include(c => c.ComponentLifeStatus).ThenInclude(s => s!.DrivingDimensionType) // NEW (Revision 13) — headline dimension for list display, no full Dimensions join needed here
                .Where(c => c.Status == ComponentStatus.Installed && c.CurrentAircraftId == aircraftId)
                .OrderBy(c => c.CurrentPosition!.SortOrder)
                .ToListAsync();
        }

        public async Task<List<Component>> GetByStockBaseAsync(int baseId, bool includeUnderRepair = true)
        {
            var statuses = includeUnderRepair
                ? new[] { ComponentStatus.InStock, ComponentStatus.UnderRepair }
                : new[] { ComponentStatus.InStock };

            return await _context.Set<Component>()
                .Include(c => c.ComponentType)
                .Where(c => c.StockBaseId == baseId && statuses.Contains(c.Status))
                .OrderBy(c => c.ComponentType!.PartNumber)
                .ToListAsync();
        }

        public async Task<bool> ExistsSerialAsync(int componentTypeId, string serialNumber, int? excludeId = null)
        {
            var query = _context.Set<Component>().Where(c => c.ComponentTypeId == componentTypeId && c.SerialNumber == serialNumber);
            if (excludeId.HasValue) query = query.Where(c => c.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<List<Component>> GetAllWithCurrentLocationAsync()
        {
            return await _context.Set<Component>()
                .Include(c => c.ComponentType)
                .Include(c => c.CurrentAircraft)
                .Include(c => c.CurrentPosition)
                .Include(c => c.StockBase)
                .Include(c => c.ComponentLifeStatus).ThenInclude(s => s!.DrivingDimensionType) // NEW (Revision 13) — headline dimension for list display, no full Dimensions join needed here
                .Include(c => c.ParentComponent).ThenInclude(p => p!.ComponentType) // NEW — hierarchy list-page columns
                .Include(c => c.ChildComponents)
                .Where(c => c.IsActive)
                .ToListAsync();
        }
    }
}
