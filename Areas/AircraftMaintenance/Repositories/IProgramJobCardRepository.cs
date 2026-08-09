using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IProgramJobCardRepository : IGenericRepository<ProgramJobCard>
    {
        Task<List<ProgramJobCard>> GetByProgramIdWithDetailsAsync(int maintenanceProgramId);
        Task<bool> ExistsAsync(int maintenanceProgramId, int jobCardId);
    }
}