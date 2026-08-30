// Areas/SquadronOps/Repositories/SortiePlanningRepository.cs
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.SquadronOps.Repositories
{
    public class SortiePlanningRepository : GenericRepository<Sortie>, ISortiePlanningRepository
    {
        public SortiePlanningRepository(FRAContext context) : base(context)
        {
        }

        public async Task<Sortie?> GetByIdWithOdvAsync(int id)
        {
            return await _context.Set<Sortie>()
                .Include(s => s.Odv)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
