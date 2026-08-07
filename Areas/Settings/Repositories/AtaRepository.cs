using FRAProject.Areas.Settings.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.Settings.Repositories
{
    public class AtaRepository : GenericRepository<Ata>, IAtaRepository
    {
        public AtaRepository(FRAContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Ata>> GetAllWithDetailsAsync()
        {
            return await _context.Set<Ata>()
                .Include(x => x.AtaCategory)
                .ToListAsync();
        }

        public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
        {
            var normalized = code.Trim().ToUpper();

            return await _context.Set<Ata>()
                .AnyAsync(x =>
                    x.Code.ToUpper() == normalized &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));
        }
    }
}