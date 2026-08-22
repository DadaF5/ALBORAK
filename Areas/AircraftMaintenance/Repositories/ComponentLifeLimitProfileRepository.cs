using Microsoft.EntityFrameworkCore;
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Infrastructure.Repositories;
using FRAProject.Data;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IComponentLifeLimitProfileRepository : IGenericRepository<ComponentLifeLimitProfile>
    {
        Task<List<ComponentLifeLimitProfile>> GetByComponentTypeAsync(int componentTypeId);
        Task<ComponentLifeLimitProfile?> GetWithStagesAsync(int id);
        /// <summary>True if an active PN_BASED profile already exists for this ComponentType (excluding excludeId) — belt-and-suspenders check alongside the DB filtered unique index.</summary>
        Task<bool> HasActivePnBasedProfileAsync(int componentTypeId, int? excludeId = null);
        /// <summary>Stages the full stage-list replacement (RemoveRange + Add) — does NOT commit. Caller must call IUnitOfWork.CompleteAsync() afterward.</summary>
        Task ReplaceStagesAsync(int profileId, IEnumerable<ComponentLifeLimitStage> stages);
    }

    public class ComponentLifeLimitProfileRepository : GenericRepository<ComponentLifeLimitProfile>, IComponentLifeLimitProfileRepository
    {
        public ComponentLifeLimitProfileRepository(FRAContext context) : base(context) { }

        public async Task<List<ComponentLifeLimitProfile>> GetByComponentTypeAsync(int componentTypeId)
        {
            return await _context.Set<ComponentLifeLimitProfile>()
                .Include(p => p.Stages).ThenInclude(s => s.Dimensions).ThenInclude(d => d.DimensionType) // NEW (Revision 13)
                .Where(p => p.ComponentTypeId == componentTypeId)
                .OrderByDescending(p => p.ApplicabilityRuleType == Models.ApplicabilityRuleType.Specific)
                .ThenBy(p => p.SerialNumber)
                .ToListAsync();
        }

        public async Task<ComponentLifeLimitProfile?> GetWithStagesAsync(int id)
        {
            return await _context.Set<ComponentLifeLimitProfile>()
                .Include(p => p.Stages.OrderBy(s => s.SequenceOrder)).ThenInclude(s => s.Dimensions).ThenInclude(d => d.DimensionType) // NEW (Revision 13)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> HasActivePnBasedProfileAsync(int componentTypeId, int? excludeId = null)
        {
            var query = _context.Set<ComponentLifeLimitProfile>().Where(p =>
                p.ComponentTypeId == componentTypeId &&
                p.ApplicabilityRuleType == Models.ApplicabilityRuleType.PnBased &&
                p.IsActive);
            if (excludeId.HasValue) query = query.Where(p => p.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        /// <summary>Replaces a profile's full stage list — same "Manage" bulk-replace pattern used elsewhere (InspectionType.ManagePrograms, ComponentType's position eligibility). Staged only (no SaveChanges here) — matches the rest of this codebase's UnitOfWork.CompleteAsync() commit convention.</summary>
        public async Task ReplaceStagesAsync(int profileId, IEnumerable<ComponentLifeLimitStage> stages)
        {
            var set = _context.Set<ComponentLifeLimitStage>();
            var existing = await set.Where(s => s.ComponentLifeLimitProfileId == profileId).ToListAsync();
            set.RemoveRange(existing);

            var sequence = 1;
            foreach (var stage in stages)
            {
                stage.ComponentLifeLimitProfileId = profileId;
                stage.SequenceOrder = sequence++;
                set.Add(stage);
            }
        }
    }
}
