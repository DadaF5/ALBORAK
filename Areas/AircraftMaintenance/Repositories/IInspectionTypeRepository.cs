using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Repositories
{
    public interface IInspectionTypeRepository : IGenericRepository<InspectionType>
    {
        /// <summary>Returns all inspection types with AcType and NextInspectionType loaded.</summary>
        Task<IEnumerable<InspectionType>> GetAllWithDetailsAsync();

        /// <summary>Returns a single inspection type with related data loaded.</summary>
        Task<InspectionType?> GetByIdWithDetailsAsync(int id);

        /// <summary>Returns all active inspection types for the given AcType.</summary>
        Task<IEnumerable<InspectionType>> GetByAcTypeIdAsync(int acTypeId);

        /// <summary>
        /// Checks whether a Code already exists for the given AcType.
        /// Pass excludeId to skip the current entity during an edit check.
        /// </summary>
        Task<bool> ExistsByCodeAsync(int acTypeId, string code, int? excludeId = null);
    }
}
