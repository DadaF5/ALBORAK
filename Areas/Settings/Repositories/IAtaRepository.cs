using FRAProject.Areas.Settings.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.Settings.Repositories
{
    public interface IAtaRepository : IGenericRepository<Ata>
    {
        Task<IEnumerable<Ata>> GetAllWithDetailsAsync();
        Task<bool> ExistsByCodeAsync(string code, int? excludeId = null);
    }
}