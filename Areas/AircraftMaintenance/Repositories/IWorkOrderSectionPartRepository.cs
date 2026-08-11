using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IWorkOrderSectionPartRepository : IGenericRepository<WorkOrderSectionPart>
    {
        Task<List<WorkOrderSectionPart>> GetByWorkOrderSectionIdAsync(int workOrderSectionId);
    }
}