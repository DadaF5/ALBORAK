using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IInspectionTypeRepository : IGenericRepository<InspectionType>
    {
        Task<IEnumerable<InspectionType>> GetAllWithDetailsAsync();
        Task<InspectionType?> GetByIdWithDetailsAsync(int id);
        Task<bool> ExistsByCodeAsync(int acTypeId, string code, int? excludeId = null);
    }
}