using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.HR.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IAcMainGroupRepository : IGenericRepository<AcMainGroup>
    {
        Task<IEnumerable<AcMainGroup>> GetByBaseIdAsync(int baseId);
        Task<IEnumerable<AcMainGroup>> GetByAcCategoryIdAsync(int acCategoryId);
        Task<IEnumerable<AcCategory>> GetAllCategoriesAsync();
        Task<IEnumerable<Base>> GetAllBasesAsync();


    }
}
