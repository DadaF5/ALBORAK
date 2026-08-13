// Areas/AircraftMaintenance/Repositories/IWorkOrderSnagRepository.cs
using FRAProject.Areas.AircraftMaintenance.Models;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IWorkOrderSnagRepository
    {
        Task<WorkOrderSnag> AddAsync(WorkOrderSnag link);
        Task<IEnumerable<WorkOrderSnag>> GetByWorkOrderAsync(int workOrderId);
        Task<IEnumerable<WorkOrderSnag>> GetBySnagAsync(int snagId);
    }
}