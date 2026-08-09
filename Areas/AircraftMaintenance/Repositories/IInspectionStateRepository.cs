using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IInspectionStateRepository : IGenericRepository<InspectionState>
    {
        Task<InspectionState?> GetByAircraftAndTypeAsync(int aircraftId, int inspectionTypeId);
        Task<List<InspectionState>> GetAllWithDetailsAsync();
    }
}