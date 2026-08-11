using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IWorkOrderRepository : IGenericRepository<WorkOrder>
    {
        Task<List<WorkOrder>> GetAllWithDetailsAsync();
        Task<WorkOrder?> GetByIdWithDetailsAsync(int id);
        Task<string> GenerateNextWONumberAsync(int year);
        Task<HashSet<int>> GetActiveInspectionTypeIdsForAircraftAsync(int aircraftId);

    }
}