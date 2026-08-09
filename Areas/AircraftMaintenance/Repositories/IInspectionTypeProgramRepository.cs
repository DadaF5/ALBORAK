using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IInspectionTypeProgramRepository : IGenericRepository<InspectionTypeProgram>
    {
        Task<List<InspectionTypeProgram>> GetByInspectionTypeIdsAsync(List<int> inspectionTypeIds);
    }
}