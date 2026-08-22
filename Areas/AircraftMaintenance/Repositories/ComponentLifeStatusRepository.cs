using Microsoft.EntityFrameworkCore;
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Infrastructure.Repositories;
using FRAProject.Data;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IComponentLifeStatusRepository : IGenericRepository<ComponentLifeStatus>
    {
        Task<ComponentLifeStatus?> GetByComponentIdAsync(int componentId);
        /// <summary>Insert-or-update by ComponentId — stages the change only (Add, or field-copy onto the tracked existing row). Does NOT commit. Caller (ComponentLifeStatusCalculator) must call IUnitOfWork.CompleteAsync() afterward.</summary>
        Task UpsertAsync(ComponentLifeStatus status);
        /// <summary>Feeds the unified DueList/DamDashboard view — see integration guide.</summary>
        Task<List<ComponentLifeStatus>> GetDueOrOverdueAsync();
    }

    public class ComponentLifeStatusRepository : GenericRepository<ComponentLifeStatus>, IComponentLifeStatusRepository
    {
        public ComponentLifeStatusRepository(FRAContext context) : base(context) { }

        public async Task<ComponentLifeStatus?> GetByComponentIdAsync(int componentId)
        {
            return await _context.Set<ComponentLifeStatus>()
                .Include(s => s.Dimensions).ThenInclude(d => d.DimensionType) // NEW (Revision 13) — full per-dimension breakdown for Details
                .Include(s => s.DrivingDimensionType) // NEW (Revision 13)
                .FirstOrDefaultAsync(s => s.ComponentId == componentId);
        }

        /// <summary>
        /// Revision 13: the 15 fixed Cumulative/SinceOverhaul/Remaining fields
        /// are gone — the per-dimension breakdown now lives in the
        /// ComponentLifeStatusDimension child collection. Because UpsertAsync
        /// does a field-by-field copy onto a TRACKED existing entity (not a
        /// delete+reinsert of the whole row, unlike
        /// ComponentLifeLimitProfileRepository.ReplaceStagesAsync), the
        /// Dimensions collection needs its own explicit "clear existing +
        /// re-add from the incoming status" step — EF won't do that for a
        /// nav collection automatically just because the parent's scalar
        /// fields were copied.
        /// </summary>
        public async Task UpsertAsync(ComponentLifeStatus status)
        {
            var set = _context.Set<ComponentLifeStatus>();
            var existing = await set
                .Include(s => s.Dimensions)
                .FirstOrDefaultAsync(s => s.ComponentId == status.ComponentId);

            if (existing == null)
            {
                set.Add(status);
            }
            else
            {
                existing.LastOverhaulDate = status.LastOverhaulDate;
                existing.MatchedLifeLimitProfileId = status.MatchedLifeLimitProfileId;
                existing.CurrentStageSequence = status.CurrentStageSequence;
                existing.MissedOverhaulCount = status.MissedOverhaulCount;
                existing.LifeLimitExceeded = status.LifeLimitExceeded;
                existing.Status = status.Status;
                existing.DrivingDimensionTypeId = status.DrivingDimensionTypeId;
                existing.DrivingDimensionRemaining = status.DrivingDimensionRemaining;
                existing.DrivingDimensionTolerance = status.DrivingDimensionTolerance;
                existing.LastComputedAtUtc = DateTime.UtcNow;

                // Clear + re-add: existing.Dimensions is tracked, status.Dimensions
                // is a freshly built (untracked) list from this recompute — the
                // simplest correct way to replace a child collection on an
                // already-tracked parent without hand-matching rows by Id.
                _context.Set<ComponentLifeStatusDimension>().RemoveRange(existing.Dimensions);
                existing.Dimensions.Clear();
                foreach (var dim in status.Dimensions)
                {
                    existing.Dimensions.Add(new ComponentLifeStatusDimension
                    {
                        DimensionTypeId = dim.DimensionTypeId,
                        Cumulative = dim.Cumulative,
                        SinceOverhaul = dim.SinceOverhaul,
                        Remaining = dim.Remaining,
                    });
                }
            }
        }

        public async Task<List<ComponentLifeStatus>> GetDueOrOverdueAsync()
        {
            return await _context.Set<ComponentLifeStatus>()
                .Include(s => s.Component).ThenInclude(c => c!.ComponentType)
                .Include(s => s.Component).ThenInclude(c => c!.CurrentAircraft)
                .Include(s => s.Component).ThenInclude(c => c!.CurrentPosition)
                .Include(s => s.DrivingDimensionType) // NEW (Revision 13) — headline dimension for the due list
                .Where(s => s.Status == ComponentLifeStatusValue.Alert || s.Status == ComponentLifeStatusValue.Overdue)
                .ToListAsync();
        }
    }
}
