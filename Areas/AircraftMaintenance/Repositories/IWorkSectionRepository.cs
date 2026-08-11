using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IWorkSectionRepository : IGenericRepository<WorkSection>
    {
        Task<List<WorkSection>> GetAllWithDetailsAsync();
        Task<bool> ExistsByCodeAsync(int acTypeId, string code, int? excludeId = null);
    }
}