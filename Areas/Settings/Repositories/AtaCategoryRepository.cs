using FRAProject.Areas.Settings.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.Settings.Repositories
{
    public class AtaCategoryRepository : GenericRepository<AtaCategory>, IAtaCategoryRepository
    {
        public AtaCategoryRepository(FRAContext context) : base(context)
        {
        }

        public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
        {
            var normalized = code.Trim().ToUpper();

            return await _context.Set<AtaCategory>()
                .AnyAsync(x =>
                    x.Code.ToUpper() == normalized &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));
        }
    }
}