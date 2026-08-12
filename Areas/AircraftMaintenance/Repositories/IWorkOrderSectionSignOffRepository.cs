using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IWorkOrderSectionSignOffRepository : IGenericRepository<WorkOrderSectionSignOff>
    {
        // Returns the 4 canonical sign-off rows for a section, creating
        // any that don't exist yet (idempotent — safe to call every time
        // the sign-off screen is opened).
        Task<List<WorkOrderSectionSignOff>> GetOrCreateCanonicalAsync(int workOrderSectionId);
    }
}