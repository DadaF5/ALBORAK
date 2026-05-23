using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public class UserMaintenanceAssignmentRepository
        : GenericRepository<UserMaintenanceAssignment>, IUserMaintenanceAssignmentRepository
    {
        public UserMaintenanceAssignmentRepository(FRAContext context) : base(context)
        {
        }

        public async Task<UserMaintenanceAssignment?> GetActiveAssignmentAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;

            return await _context.Set<UserMaintenanceAssignment>()
                .Include(a => a.Base)
                .Include(a => a.AcMainGroup)
                .Include(a => a.MaintenanceRole)
                .Include(a => a.AdditionalGroups)
                    .ThenInclude(g => g.AcMainGroup)
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.IsActive &&
                    (a.EffectiveTo == null || a.EffectiveTo.Value.Date >= today));
        }

        public async Task<IEnumerable<UserMaintenanceAssignment>> GetHistoryByUserAsync(string userId)
        {
            return await _context.Set<UserMaintenanceAssignment>()
                .Include(a => a.Base)
                .Include(a => a.AcMainGroup)
                .Include(a => a.MaintenanceRole)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.EffectiveFrom)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserMaintenanceAssignment>> GetActiveByBaseAsync(int baseId)
        {
            var today = DateTime.UtcNow.Date;

            return await _context.Set<UserMaintenanceAssignment>()
                .Include(a => a.User)
                .Include(a => a.AcMainGroup)
                .Include(a => a.MaintenanceRole)
                .Where(a =>
                    a.BaseId == baseId &&
                    a.IsActive &&
                    (a.EffectiveTo == null || a.EffectiveTo.Value.Date >= today))
                .OrderBy(a => a.AcMainGroup.Name)
                .ThenBy(a => a.User.LastName)
                .ToListAsync();
        }

        public async Task<UserMaintenanceScope?> GetScopeAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;

            var assignment = await GetActiveAssignmentAsync(userId);
            if (assignment == null) return null;

            var user = await _context.Users.FindAsync(userId);

            // Collect all active AcMainGroup IDs: primary + additional
            var activeGroups = new List<(int Id, string Name)>
            {
                (assignment.AcMainGroupId, assignment.AcMainGroup.Name)
            };

            foreach (var extra in assignment.AdditionalGroups)
            {
                if (extra.EffectiveTo == null || extra.EffectiveTo.Value.Date >= today)
                {
                    activeGroups.Add((extra.AcMainGroupId, extra.AcMainGroup.Name));
                }
            }

            return new UserMaintenanceScope
            {
                UserId = userId,
                UserDisplayName = user?.DisplayName ?? userId,
                AssignmentId = assignment.Id,
                AssignmentEffectiveFrom = assignment.EffectiveFrom,
                BaseId = assignment.BaseId,
                BaseName = assignment.Base.BaseName,
                PrimaryAcMainGroupId = assignment.AcMainGroupId,
                PrimaryAcMainGroupName = assignment.AcMainGroup.Name,
                ActiveAcMainGroupIds = activeGroups.Select(g => g.Id).ToList().AsReadOnly(),
                ActiveAcMainGroupNames = activeGroups.Select(g => g.Name).ToList().AsReadOnly(),
                MaintenanceRoleId = assignment.MaintenanceRoleId,
                RoleCode = assignment.MaintenanceRole.Code,
                RoleName = assignment.MaintenanceRole.Name
            };
        }
    }
}
