using FRAProject.Areas.AircraftMaintenance.Repositories;

namespace FRAProject.Infrastructure.Interfaces
{
    public interface IUnitOfWork
    {
        IAcMainGroupRepository AcMainGroups { get; }
        IInspectionTypeRepository InspectionTypes { get; }
        IUserMaintenanceAssignmentRepository UserMaintenanceAssignments { get; }

        Task<int> CompleteAsync(); // Save changes across all repositories
    }
}
