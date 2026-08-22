using Microsoft.EntityFrameworkCore;
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Infrastructure.Repositories;
using FRAProject.Data;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    /// <summary>
    /// Append-only. Deliberately does NOT expose Update/Delete beyond what the
    /// generic IGenericRepository<T> base provides — the service layer must never
    /// call those for ComponentEvent. Corrections happen by adding a new event,
    /// never editing history (same rule as UserAssignment).
    /// </summary>
    public interface IComponentEventRepository : IGenericRepository<ComponentEvent>
    {
        Task<List<ComponentEvent>> GetHistoryAsync(int componentId);
        Task<ComponentEvent?> GetLastOverhaulAsync(int componentId);
    }

    public class ComponentEventRepository : GenericRepository<ComponentEvent>, IComponentEventRepository
    {
        public ComponentEventRepository(FRAContext context) : base(context) { }

        public async Task<List<ComponentEvent>> GetHistoryAsync(int componentId)
        {
            return await _context.Set<ComponentEvent>()
                .Include(e => e.Aircraft)
                .Include(e => e.Position)
                .Include(e => e.LinkedWorkOrder)
                .Include(e => e.PerformedByUser)
                .Include(e => e.RelatedParentComponent).ThenInclude(p => p!.ComponentType) // NEW — AttachToParent/DetachFromParent display
                .Include(e => e.Readings).ThenInclude(r => r.DimensionType) // NEW (Revision 13) — generic per-dimension snapshot, ComponentLifeStatusCalculator and history display both need this
                .Where(e => e.ComponentId == componentId)
                .OrderBy(e => e.EventDate).ThenBy(e => e.Id)
                .ToListAsync();
        }

        public async Task<ComponentEvent?> GetLastOverhaulAsync(int componentId)
        {
            return await _context.Set<ComponentEvent>()
                .Where(e => e.ComponentId == componentId && e.EventType == ComponentEventType.Overhaul)
                .OrderByDescending(e => e.EventDate).ThenByDescending(e => e.Id)
                .FirstOrDefaultAsync();
        }
    }
}
