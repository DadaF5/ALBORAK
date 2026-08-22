using Microsoft.EntityFrameworkCore;
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Infrastructure.Repositories;
using FRAProject.Data;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IComponentTypeRepository : IGenericRepository<ComponentType>
    {
        Task<ComponentType?> GetWithLifeLimitAsync(int id);
        /// <summary>Same as GetAllAsync but eager-loads LifeLimitProfiles so list-page profile counts are accurate — the generic GetAllAsync() has no Include, so ComponentType.LifeLimitProfiles would silently read as empty otherwise.</summary>
        Task<List<ComponentType>> GetAllWithLifeLimitProfilesAsync();
        Task<List<ComponentType>> GetEligibleForPositionAsync(int componentPositionId);
        Task<bool> ExistsByPartNumberAsync(string partNumber, int? excludeId = null);
        Task<List<int>> GetPositionIdsAsync(int componentTypeId);
        /// <summary>Stages the full eligible-positions replacement (RemoveRange + Add) — does NOT commit. Caller must call IUnitOfWork.CompleteAsync() afterward.</summary>
        Task SetPositionsAsync(int componentTypeId, IEnumerable<int> componentPositionIds);

        /// <summary>NEW — every ComponentType this one is eligible to be a CHILD of (reverse lookup, via EligibleAsChildIn) — used to offer "attach as sub-assembly to..." choices when receiving/editing a Component.</summary>
        Task<List<ComponentType>> GetEligibleParentTypesAsync(int childComponentTypeId);
    }

    public class ComponentTypeRepository : GenericRepository<ComponentType>, IComponentTypeRepository
    {
        public ComponentTypeRepository(FRAContext context) : base(context) { }

        public async Task<ComponentType?> GetWithLifeLimitAsync(int id)
        {
            return await _context.Set<ComponentType>()
                .Include(t => t.LifeLimitProfiles).ThenInclude(p => p.Stages)
                .Include(t => t.Ata)
                .Include(t => t.AircraftManufacturer)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<ComponentType>> GetAllWithLifeLimitProfilesAsync()
        {
            return await _context.Set<ComponentType>()
                .Include(t => t.LifeLimitProfiles)
                .Include(t => t.Ata)
                .ToListAsync();
        }

        public async Task<List<ComponentType>> GetEligibleForPositionAsync(int componentPositionId)
        {
            return await _context.Set<ComponentType>()
                .Where(t => t.IsActive && t.ComponentTypePositions.Any(cp => cp.ComponentPositionId == componentPositionId))
                .OrderBy(t => t.PartNumber)
                .ToListAsync();
        }

        public async Task<bool> ExistsByPartNumberAsync(string partNumber, int? excludeId = null)
        {
            var query = _context.Set<ComponentType>().Where(t => t.PartNumber == partNumber);
            if (excludeId.HasValue) query = query.Where(t => t.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<List<int>> GetPositionIdsAsync(int componentTypeId)
        {
            return await _context.Set<ComponentTypePosition>()
                .Where(x => x.ComponentTypeId == componentTypeId)
                .Select(x => x.ComponentPositionId)
                .ToListAsync();
        }

        /// <summary>Replaces the full eligible-positions set for a ComponentType — same "Manage" pattern as InspectionType.ManagePrograms/ProgramJobCards bulk-assign. Staged only (no SaveChanges here) — matches the rest of this codebase's UnitOfWork.CompleteAsync() commit convention.</summary>
        public async Task SetPositionsAsync(int componentTypeId, IEnumerable<int> componentPositionIds)
        {
            var set = _context.Set<ComponentTypePosition>();
            var existing = await set.Where(x => x.ComponentTypeId == componentTypeId).ToListAsync();
            set.RemoveRange(existing);

            foreach (var positionId in componentPositionIds.Distinct())
            {
                set.Add(new ComponentTypePosition { ComponentTypeId = componentTypeId, ComponentPositionId = positionId });
            }
        }

        public async Task<List<ComponentType>> GetEligibleParentTypesAsync(int childComponentTypeId)
        {
            // Eligibility rows no longer carry ParentComponentTypeId directly
            // (moved to ComponentTypeSlot this revision) — join through Slot.
            return await _context.Set<ComponentTypeSubAssemblySlot>()
                .Include(x => x.Slot)
                .Where(x => x.ChildComponentTypeId == childComponentTypeId && x.IsActive && x.Slot!.IsActive)
                .Select(x => x.Slot!.ParentComponentType!)
                .Distinct()
                .OrderBy(t => t.PartNumber)
                .ToListAsync();
        }
    }
}
