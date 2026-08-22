using Microsoft.EntityFrameworkCore;
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Infrastructure.Repositories;
using FRAProject.Data;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    /// <summary>
    /// NEW — hierarchy per-PN eligibility rows (design doc §2). RESTRUCTURED
    /// this revision: capacity (MaxCount) moved out to ComponentTypeSlot —
    /// this repository is now purely about "which child PN(s) fit which slot".
    /// </summary>
    public interface IComponentTypeSubAssemblySlotRepository : IGenericRepository<ComponentTypeSubAssemblySlot>
    {
        Task<ComponentTypeSubAssemblySlot?> FindEligibilityAsync(int slotId, int childComponentTypeId);
        Task<bool> ExistsAsync(int slotId, int childComponentTypeId, int? excludeId = null);
    }

    public class ComponentTypeSubAssemblySlotRepository : GenericRepository<ComponentTypeSubAssemblySlot>, IComponentTypeSubAssemblySlotRepository
    {
        public ComponentTypeSubAssemblySlotRepository(FRAContext context) : base(context) { }

        public async Task<ComponentTypeSubAssemblySlot?> FindEligibilityAsync(int slotId, int childComponentTypeId)
        {
            return await _context.Set<ComponentTypeSubAssemblySlot>()
                .FirstOrDefaultAsync(x => x.SlotId == slotId && x.ChildComponentTypeId == childComponentTypeId && x.IsActive);
        }

        public async Task<bool> ExistsAsync(int slotId, int childComponentTypeId, int? excludeId = null)
        {
            var query = _context.Set<ComponentTypeSubAssemblySlot>()
                .Where(x => x.SlotId == slotId && x.ChildComponentTypeId == childComponentTypeId);
            if (excludeId.HasValue) query = query.Where(x => x.Id != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}
