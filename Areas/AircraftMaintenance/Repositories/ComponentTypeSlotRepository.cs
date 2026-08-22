using Microsoft.EntityFrameworkCore;
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Infrastructure.Repositories;
using FRAProject.Data;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    /// <summary>NEW — hierarchy slot DEFINITIONS (design doc §2). See ComponentTypeSlot's doc comment for why capacity was normalized out of the per-PN eligibility table into here.</summary>
    public interface IComponentTypeSlotRepository : IGenericRepository<ComponentTypeSlot>
    {
        /// <summary>All slots defined on a parent ComponentType, eager-loaded with their eligible child PNs — used by both the admin "Manage slots" page and AttachToParentAsync/GetSlotStatusAsync.</summary>
        Task<List<ComponentTypeSlot>> GetByParentComponentTypeAsync(int parentComponentTypeId, bool includeInactive = false);

        /// <summary>Single active slot by its code on a given parent ComponentType — the lookup AttachToParentAsync uses to resolve capacity + eligibility together in one call.</summary>
        Task<ComponentTypeSlot?> GetBySlotCodeAsync(int parentComponentTypeId, string slotCode);

        Task<ComponentTypeSlot?> GetWithEligibilityAsync(int id);

        Task<bool> ExistsAsync(int parentComponentTypeId, string slotCode, int? excludeId = null);
    }

    public class ComponentTypeSlotRepository : GenericRepository<ComponentTypeSlot>, IComponentTypeSlotRepository
    {
        public ComponentTypeSlotRepository(FRAContext context) : base(context) { }

        public async Task<List<ComponentTypeSlot>> GetByParentComponentTypeAsync(int parentComponentTypeId, bool includeInactive = false)
        {
            var query = _context.Set<ComponentTypeSlot>()
                .Include(s => s.EligibleChildren).ThenInclude(e => e.ChildComponentType)
                .Where(s => s.ParentComponentTypeId == parentComponentTypeId);

            if (!includeInactive) query = query.Where(s => s.IsActive);

            return await query.OrderBy(s => s.SortOrder).ThenBy(s => s.SlotCode).ToListAsync();
        }

        public async Task<ComponentTypeSlot?> GetBySlotCodeAsync(int parentComponentTypeId, string slotCode)
        {
            return await _context.Set<ComponentTypeSlot>()
                .Include(s => s.EligibleChildren).ThenInclude(e => e.ChildComponentType)
                .FirstOrDefaultAsync(s => s.ParentComponentTypeId == parentComponentTypeId && s.SlotCode == slotCode && s.IsActive);
        }

        public async Task<ComponentTypeSlot?> GetWithEligibilityAsync(int id)
        {
            return await _context.Set<ComponentTypeSlot>()
                .Include(s => s.EligibleChildren).ThenInclude(e => e.ChildComponentType)
                .Include(s => s.ParentComponentType)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> ExistsAsync(int parentComponentTypeId, string slotCode, int? excludeId = null)
        {
            var query = _context.Set<ComponentTypeSlot>()
                .Where(s => s.ParentComponentTypeId == parentComponentTypeId && s.SlotCode == slotCode);
            if (excludeId.HasValue) query = query.Where(s => s.Id != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}
