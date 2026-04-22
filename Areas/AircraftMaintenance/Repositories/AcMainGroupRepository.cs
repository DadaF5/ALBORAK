using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.HR.Models;
using FRAProject.Areas.Settings.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public class AcMainGroupRepository : GenericRepository<AcMainGroup>, IAcMainGroupRepository
    {
        public AcMainGroupRepository(FRAContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AcMainGroup>> GetByBaseIdAsync(int baseId)
        {
            return await _context.Set<AcMainGroup>()
                                 .Include(g => g.AcCategory)
                                 .Include(g => g.Base)
                                 .Where(g => g.BaseId == baseId)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<AcMainGroup>> GetByAcCategoryIdAsync(int categoryId)
        {
            return await _context.Set<AcMainGroup>()
                                 .Include(g => g.AcCategory)
                                 .Include(g => g.Base)
                                 .Where(g => g.AcCategoryId == categoryId)
                                 .ToListAsync();
        }

        //public async Task<IEnumerable<AcCategory>> GetAcCategoriesAsync()
        //{
        //    return await _context.AcCategories.OrderBy(c => c.Name).ToListAsync();
        //}

        public async Task<IEnumerable<Base>> GetAllBasesAsync()
        {
            return await _context.Bases.OrderBy(b => b.BaseName).ToListAsync(); // Populates Bases
        }

        public async Task<IEnumerable<AcCategory>> GetAllCategoriesAsync()
        {
            return await _context.AcCategories.OrderBy(c => c.Name).ToListAsync(); // Populates Categories
        }
    }
}
