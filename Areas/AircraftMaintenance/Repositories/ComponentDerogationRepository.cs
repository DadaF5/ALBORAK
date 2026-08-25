using Microsoft.EntityFrameworkCore;
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Infrastructure.Repositories;
using FRAProject.Data;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    /// <summary>
    /// Append-only. Deliberately does NOT expose Update beyond the generic
    /// IGenericRepository&lt;T&gt; base provides (the base method exists for
    /// the future "void" action — see ComponentDerogation.IsActive — but no
    /// service call uses it yet) — same "corrections are a new row, never an
    /// edit of history" discipline as IComponentEventRepository.
    /// </summary>
    public interface IComponentDerogationRepository : IGenericRepository<ComponentDerogation>
    {
        Task<List<ComponentDerogation>> GetByComponentTypeAsync(int componentTypeId);

        /// <summary>
        /// NEW — for ComponentLifeStatusCalculator's derogation-wiring pass.
        /// Deliberately lean: no Includes (the calculator only needs the
        /// scalar fields — DimensionTypeId, TargetStageType, Mode, Direction,
        /// Value, applicability, EffectiveUntil — to compute deltas, it
        /// already has the dimension catalog loaded separately) and IsActive
        /// filtered at the DB rather than in memory since this runs on every
        /// RecomputeAsync call (i.e. after every ComponentEvent).
        /// </summary>
        Task<List<ComponentDerogation>> GetActiveByComponentTypeAsync(int componentTypeId);
    }

    public class ComponentDerogationRepository : GenericRepository<ComponentDerogation>, IComponentDerogationRepository
    {
        public ComponentDerogationRepository(FRAContext context) : base(context) { }

        public async Task<List<ComponentDerogation>> GetByComponentTypeAsync(int componentTypeId)
        {
            return await _context.Set<ComponentDerogation>()
                .Include(d => d.DimensionType)
                .Include(d => d.VoidedByUser) // NEW — Void action display
                .Where(d => d.ComponentTypeId == componentTypeId)
                .OrderByDescending(d => d.IssuedDate).ThenByDescending(d => d.Id)
                .ToListAsync();
        }

        public async Task<List<ComponentDerogation>> GetActiveByComponentTypeAsync(int componentTypeId)
        {
            return await _context.Set<ComponentDerogation>()
                .Where(d => d.ComponentTypeId == componentTypeId && d.IsActive)
                .ToListAsync();
        }
    }
}
