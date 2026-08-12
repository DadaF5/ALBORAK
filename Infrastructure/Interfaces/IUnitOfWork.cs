
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.AircraftMaintenance.Repositories;
using FRAProject.Areas.Settings.Interfaces;
using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.Settings.Repositories;
using FRAProject.Data.Configurations;
using FRAProject.Models;

namespace FRAProject.Infrastructure.Interfaces
{
    /// <summary>
    /// Unit of Work — owns all repositories for the request.
    /// One DbContext is shared across all repositories so that
    /// a single CompleteAsync() commits everything atomically.
    ///
    /// The controller only ever talks to IUnitOfWork, never to
    /// DbContext or any repository directly.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // ── Specialist repository (AcMainGroup has custom methods) ────────
        // IAcMainGroupRepository extends IGenericRepository<AcMainGroup>
        // so it has all generic methods PLUS any custom ones you added.
        IAcMainGroupRepository AcMainGroups { get; }
        IInspectionTypeRepository InspectionTypes { get; }

        // ── Lookup tables ─────────────────────────────────────────────────
        IGenericRepository<Country> Countries { get; }
        IGenericRepository<EmployingAuthority> EmployingAuthorities { get; }
        IGenericRepository<AcCategory> AcCategories { get; }
        IGenericRepository<AcStatusType> AcStatusTypes { get; }

        IGenericRepository<CdnDocType> CdnDocTypes { get; }
        IGenericRepository<MissionRole> MissionRoles { get; }
        IGenericRepository<ImmatriculationDocType> ImmatriculationDocTypes { get; }

        // ── Settings ──────────────────────────────────────────────────────
        IGenericRepository<AircraftVersion> AircraftVersions { get; }
        IGenericRepository<AircraftManufacturer> AircraftManufacturers { get; }
        IGenericRepository<Base> Bases { get; }

        // ── Immatriculation dossier ───────────────────────────────────────
        IGenericRepository<ImmatriculationDossier> Dossiers { get; }
        IGenericRepository<DossierAuthority> DossierAuthorities { get; }
        IGenericRepository<DossierAircraft> DossierAircrafts { get; }
        IGenericRepository<DossierAirworthiness> DossierAirworthiness { get; }
        IGenericRepository<ImmatriculationDocument> ImmatriculationDocuments { get; }

        // Add more as you build:
        IGenericRepository<AcType> AcTypes { get; }
        IGenericRepository<Aircraft> Aircraft { get; }
        IGenericRepository<AircraftCertificate> AircraftCertificates { get; }
        IGenericRepository<AircraftRestriction> AircraftRestrictions { get; }

        // Maintenance Inspection        
        IMaintenanceProgramRepository MaintenancePrograms { get; }
        IAtaCategoryRepository AtaCategories { get; }
        IAtaRepository Ata { get; }
        IJobCardRepository JobCards { get; }              // ← ADD THIS 
        IProgramJobCardRepository ProgramJobCards { get; }
        IWorkOrderRepository WorkOrders { get; }
        IGenericRepository<WorkOrderJobCard> WorkOrderJobCards { get; }
        IInspectionStateRepository InspectionStates { get; }
        IInspectionTypeProgramRepository InspectionTypePrograms { get; }
        IWorkSectionRepository WorkSections { get; }
        IWorkOrderSectionRepository WorkOrderSections { get; }
        IWorkOrderSectionPartRepository WorkOrderSectionParts { get; }
        IWorkOrderSectionTaskRepository WorkOrderSectionTasks { get; }
        IWorkOrderSectionSignOffRepository WorkOrderSectionSignOffs { get; }
        // ── Commit ────────────────────────────────────────────────────────
        /// <summary>
        /// Flush all staged changes to the database in one transaction.
        /// Single method — use CompleteAsync() everywhere.
        /// </summary>
        Task<int> CompleteAsync();
    }
}