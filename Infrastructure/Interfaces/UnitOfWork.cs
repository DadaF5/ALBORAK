
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.AircraftMaintenance.Repositories;
using FRAProject.Areas.Settings.Interfaces;
using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.Settings.Repositories;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Infrastructure.Repositories;
using FRAProject.Models;
using FRAProject.Support.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FRAContext _context;

        public UnitOfWork(FRAContext context)
        {
            _context = context;

            // AcMainGroups uses a specialist repository (custom methods)
            // so it is initialized directly — not lazy like the others.
            AcMainGroups = new AcMainGroupRepository(_context);
            InspectionTypes = new InspectionTypeRepository(_context);
            MaintenancePrograms = new MaintenanceProgramRepository(_context);
            JobCards = new JobCardRepository(_context);
            AtaCategories = new AtaCategoryRepository(_context);
            Ata = new AtaRepository(_context);
            ProgramJobCards = new ProgramJobCardRepository(_context);
            WorkOrders = new WorkOrderRepository(_context);
            WorkOrderJobCards = new GenericRepository<WorkOrderJobCard>(_context);
            InspectionStates = new InspectionStateRepository(_context);
            InspectionTypePrograms = new InspectionTypeProgramRepository(_context);
            WorkSections = new WorkSectionRepository(_context);
            WorkOrderSections = new WorkOrderSectionRepository(_context);
            WorkOrderSectionParts = new WorkOrderSectionPartRepository(_context);
            WorkOrderSectionTasks = new WorkOrderSectionTaskRepository(_context);
            WorkOrderSectionSignOffs = new WorkOrderSectionSignOffRepository(_context);

            // Aircraft snags and malfunctions
            Snags = new SnagRepository(context);
            WorkOrderSnags = new WorkOrderSnagRepository(context);
            Sorties = new SortieRepository(context);


            // Application Support snags, errors, bugs, issues
            BugReports = new BugReportRepository(_context);

            UserAssignments = new UserAssignmentRepository(context);

            // Component / Life-Limited Parts tracking (NEW)
            ComponentPositions = new ComponentPositionRepository(_context);
            ComponentTypes = new ComponentTypeRepository(_context);
            ComponentLifeLimitProfiles = new ComponentLifeLimitProfileRepository(_context);
            Components = new ComponentRepository(_context);
            ComponentEvents = new ComponentEventRepository(_context);
            ComponentLifeStatuses = new ComponentLifeStatusRepository(_context);
            ComponentTypeSlots = new ComponentTypeSlotRepository(_context);
            ComponentTypeSubAssemblySlots = new ComponentTypeSubAssemblySlotRepository(_context);
            ComponentInitialReadings = new GenericRepository<ComponentInitialReading>(_context); // NEW (Revision 12)
            ComponentLifeLimitDimensionTypes = new GenericRepository<ComponentLifeLimitDimensionType>(_context); // NEW (Revision 13)
            ComponentReferenceBases = new GenericRepository<ComponentReferenceBasis>(_context); // NEW
            ComponentDerogations = new ComponentDerogationRepository(_context); // NEW (Derogation implementation pass)

        }

        // ── Specialist repository ─────────────────────────────────────────
        public IAcMainGroupRepository AcMainGroups { get; private set; }
        public IInspectionTypeRepository InspectionTypes { get; private set; }
        public IMaintenanceProgramRepository MaintenancePrograms { get; private set; }
        public IJobCardRepository JobCards { get; private set; }           // ← ADD THIS LINE
        public IProgramJobCardRepository ProgramJobCards { get; private set; }

        // ── Backing fields — null until first access ──────────────────────

        // Ata
        public IAtaCategoryRepository AtaCategories { get; private set; }
        public IAtaRepository Ata { get; private set; }

        // Lookup tables
        private IGenericRepository<Country>? _countries;
        private IGenericRepository<AcCategory>? _acCategories;
        private IGenericRepository<AcType>? _acTypes;
        private IGenericRepository<Aircraft>? _aircraft;

        // Maintenance ------ START
        private IGenericRepository<AircraftCertificate>? _aircraftCertificates;
        private IGenericRepository<AircraftRestriction>? _aircraftRestrictions;
        // Maintenance ------ END

        private IGenericRepository<EmployingAuthority>? _employingAuthorities;
        private IGenericRepository<AcStatusType>? _acStatusTypes;

        private IGenericRepository<CdnDocType>? _cdnDocTypes;
        private IGenericRepository<MissionRole>? _missionRoles;
        private IGenericRepository<ImmatriculationDocType>? _immatriculationDocTypes;

        // Settings
        private IGenericRepository<AircraftManufacturer>? _aircraftManufacturers;
        private IGenericRepository<AircraftVersion>? _aircraftVersions;

        private IGenericRepository<Base>? _bases;
        private IGenericRepository<Wing>? _wings;

        // Immatriculation dossier
        private IGenericRepository<ImmatriculationDossier>? _dossiers;
        private IGenericRepository<DossierAuthority>? _dossierAuthorities;
        private IGenericRepository<DossierAircraft>? _dossierAircrafts;
        private IGenericRepository<DossierAirworthiness>? _dossierAirworthiness;
        private IGenericRepository<ImmatriculationDocument>? _immatriculationDocuments;


        // UnitOfWork.cs — add a backing field near the other lookup private fields
        private IGenericRepository<ModuleRole>? _moduleRoles;
        private IGenericRepository<Module>? _modules;
        // ── Repository accessors (lazy init) ──────────────────────────────
        // UnitOfWork.cs — add the lazy accessor near the other lookup accessors
        public IGenericRepository<ModuleRole> ModuleRoles =>
            _moduleRoles ??= new GenericRepository<ModuleRole>(_context);
        public IGenericRepository<Module> Modules =>
            _modules ??= new GenericRepository<Module>(_context);
        // Lookup tables
        public IGenericRepository<Country> Countries =>
            _countries ??= new GenericRepository<Country>(_context);
        public IGenericRepository<AcCategory> AcCategories =>
            _acCategories ??= new GenericRepository<AcCategory>(_context);
        public IGenericRepository<AcType> AcTypes =>
            _acTypes ??= new GenericRepository<AcType>(_context);
        public IGenericRepository<Aircraft> Aircraft =>
            _aircraft ??= new GenericRepository<Aircraft>(_context);

        public IGenericRepository<EmployingAuthority> EmployingAuthorities =>
            _employingAuthorities ??= new GenericRepository<EmployingAuthority>(_context);
        public IGenericRepository<AcStatusType> AcStatusTypes =>
            _acStatusTypes ??= new GenericRepository<AcStatusType>(_context);
        public IGenericRepository<CdnDocType> CdnDocTypes =>
            _cdnDocTypes ??= new GenericRepository<CdnDocType>(_context);

        public IGenericRepository<MissionRole> MissionRoles =>
            _missionRoles ??= new GenericRepository<MissionRole>(_context);

        public IGenericRepository<ImmatriculationDocType> ImmatriculationDocTypes =>
            _immatriculationDocTypes ??= new GenericRepository<ImmatriculationDocType>(_context);

        // Settings
        public IGenericRepository<AircraftVersion> AircraftVersions =>
            _aircraftVersions ??= new GenericRepository<AircraftVersion>(_context);

        public IGenericRepository<AircraftManufacturer> AircraftManufacturers =>
            _aircraftManufacturers ??= new GenericRepository<AircraftManufacturer>(_context);

        public IGenericRepository<Base> Bases =>
            _bases ??= new GenericRepository<Base>(_context);
        public IGenericRepository<Wing> Wings =>
            _wings ??= new GenericRepository<Wing>(_context);
        // Immatriculation dossier
        public IGenericRepository<ImmatriculationDossier> Dossiers =>
            _dossiers ??= new GenericRepository<ImmatriculationDossier>(_context);

        public IGenericRepository<DossierAuthority> DossierAuthorities =>
            _dossierAuthorities ??= new GenericRepository<DossierAuthority>(_context);

        public IGenericRepository<DossierAircraft> DossierAircrafts =>
            _dossierAircrafts ??= new GenericRepository<DossierAircraft>(_context);

        public IGenericRepository<DossierAirworthiness> DossierAirworthiness =>
            _dossierAirworthiness ??= new GenericRepository<DossierAirworthiness>(_context);

        public IGenericRepository<ImmatriculationDocument> ImmatriculationDocuments =>
            _immatriculationDocuments ??= new GenericRepository<ImmatriculationDocument>(_context);

        // Maintenance
        public IGenericRepository<AircraftCertificate> AircraftCertificates =>
            _aircraftCertificates ??= new GenericRepository<AircraftCertificate>(_context);
        public IGenericRepository<AircraftRestriction> AircraftRestrictions =>
            _aircraftRestrictions ??= new GenericRepository<AircraftRestriction>(_context);

        public IWorkOrderRepository WorkOrders { get; private set; }
        public IGenericRepository<WorkOrderJobCard> WorkOrderJobCards { get; private set; }
        public IInspectionStateRepository InspectionStates { get; private set; }
        public IInspectionTypeProgramRepository InspectionTypePrograms { get; private set; }
        public IWorkSectionRepository WorkSections { get; private set; }
        public IWorkOrderSectionRepository WorkOrderSections { get; private set; }
        public IWorkOrderSectionPartRepository WorkOrderSectionParts { get; private set; }
        public IWorkOrderSectionTaskRepository WorkOrderSectionTasks { get; private set; }
        public IWorkOrderSectionSignOffRepository WorkOrderSectionSignOffs { get; private set; }

        // Aircraft snags management
        public ISnagRepository Snags { get; }
        public IWorkOrderSnagRepository WorkOrderSnags { get; }
        public ISortieRepository Sorties { get; }

        //  Application Support :Snags, Errors , Bugs, Issues
        public IBugReportRepository BugReports { get; }

        // User assignments and access control
        public IUserAssignmentRepository UserAssignments { get; private set; }

        // ── Component / Life-Limited Parts tracking (NEW) ──────────────────
        public IComponentPositionRepository ComponentPositions { get; private set; }
        public IComponentTypeRepository ComponentTypes { get; private set; }
        public IComponentLifeLimitProfileRepository ComponentLifeLimitProfiles { get; private set; }
        public IComponentRepository Components { get; private set; }
        public IComponentEventRepository ComponentEvents { get; private set; }
        public IComponentLifeStatusRepository ComponentLifeStatuses { get; private set; }
        public IComponentTypeSlotRepository ComponentTypeSlots { get; private set; }
        public IComponentTypeSubAssemblySlotRepository ComponentTypeSubAssemblySlots { get; private set; }
        public IGenericRepository<ComponentInitialReading> ComponentInitialReadings { get; private set; }
        public IGenericRepository<ComponentLifeLimitDimensionType> ComponentLifeLimitDimensionTypes { get; private set; }
        public IGenericRepository<ComponentReferenceBasis> ComponentReferenceBases { get; private set; }
        public IComponentDerogationRepository ComponentDerogations { get; private set; }

        // ── Commit ────────────────────────────────────────────────────────
        // Single method — CompleteAsync() — matches IUnitOfWork contract.
        public async Task<int> CompleteAsync() =>
            await _context.SaveChangesAsync();

        // ── Dispose ───────────────────────────────────────────────────────
        public void Dispose() => _context.Dispose();
    }
}
