using FRAProject.Areas.Settings.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.Settings.Repositories
{
    public interface IAtaCategoryRepository : IGenericRepository<AtaCategory>
    {
        Task<bool> ExistsByCodeAsync(string code, int? excludeId = null);
    }
}