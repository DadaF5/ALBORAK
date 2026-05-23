using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IUserMaintenanceAssignmentRepository : IGenericRepository<UserMaintenanceAssignment>
    {
        /// <summary>Returns the single currently-active assignment for the given user, or null.</summary>
        Task<UserMaintenanceAssignment?> GetActiveAssignmentAsync(string userId);

        /// <summary>Returns all assignments (history) for the given user, newest first.</summary>
        Task<IEnumerable<UserMaintenanceAssignment>> GetHistoryByUserAsync(string userId);

        /// <summary>Returns all active assignments for a base.</summary>
        Task<IEnumerable<UserMaintenanceAssignment>> GetActiveByBaseAsync(int baseId);

        /// <summary>
        /// Builds the runtime UserMaintenanceScope for the given user from their active assignment
        /// and any currently-active additional group rows. Returns null if no active assignment found.
        /// </summary>
        Task<UserMaintenanceScope?> GetScopeAsync(string userId);
    }
}
