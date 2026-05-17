using FRAProject.Areas.Settings.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.Settings.Interfaces
{
    public interface IAcMainGroupRepository : IGenericRepository<AcMainGroup>
    {
        Task<IEnumerable<AcMainGroup>> GetByBaseIdAsync(int baseId);
        Task<IEnumerable<AcMainGroup>> GetByAcCategoryIdAsync(int acCategoryId);
        Task<IEnumerable<AcCategory>> GetAllCategoriesAsync();
        Task<IEnumerable<Base>> GetAllBasesAsync();



    }
}
