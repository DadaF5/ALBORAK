using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IMaintenanceProgramRepository : IGenericRepository<MaintenanceProgram>
    {
        Task<IEnumerable<MaintenanceProgram>> GetAllWithDetailsAsync();
        Task<MaintenanceProgram?> GetByIdWithDetailsAsync(int id);
        Task<bool> ExistsByCodeAsync(int acTypeId, string code, int? excludeId = null);
    }
}