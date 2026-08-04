using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IJobCardRepository : IGenericRepository<JobCard>
    {
        Task<IEnumerable<JobCard>> GetAllWithDetailsAsync();
        Task<JobCard?> GetByIdWithDetailsAsync(int id);
        Task<bool> ExistsByCodeAsync(int acTypeId, string cardCode, int? excludeId = null);
    }
}