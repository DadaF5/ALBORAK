using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IWorkOrderSectionRepository : IGenericRepository<WorkOrderSection>
    {
        Task<List<WorkOrderSection>> GetByWorkOrderIdWithDetailsAsync(int workOrderId);
        Task<WorkOrderSection?> GetByIdWithDetailsAsync(int id);
    }
}