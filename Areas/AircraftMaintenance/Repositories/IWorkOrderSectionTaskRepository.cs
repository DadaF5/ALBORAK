using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IWorkOrderSectionTaskRepository : IGenericRepository<WorkOrderSectionTask>
    {
        Task<List<WorkOrderSectionTask>> GetByWorkOrderSectionIdAsync(int workOrderSectionId);
    }
}