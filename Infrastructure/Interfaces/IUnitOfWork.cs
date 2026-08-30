
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.AircraftMaintenance.Repositories;
using FRAProject.Areas.Settings.Interfaces;
using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.Settings.Repositories;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Areas.SquadronOps.Repositories;
using FRAProject.Data.Configurations;
using FRAProject.Infrastructure.Repositories;
using FRAProject.Models;
using FRAProject.Support.Repositories;

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
        IGenericRepository<Wing> Wings { get; }
        // IUnitOfWork.cs — add alongside the other lookup entries
        IGenericRepository<ModuleRole> ModuleRoles { get; }
        IGenericRepository<Module> Modules { get; }

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

        // Aircraft Snags
        // IUnitOfWork.cs — add alongside existing Maintenance Phase 2 entries
        ISnagRepository Snags { get; }
        IWorkOrderSnagRepository WorkOrderSnags { get; }

        // Maintenance-owned, read-only FH-aggregation specialist. Narrow on
        // purpose (one method, no CRUD) — see ISortieRepository.cs. Do NOT
        // repurpose this for SquadronOps' own Sortie CRUD — see
        // SortiePlanning below instead.
        ISortieRepository Sorties { get; }

        // Support
        IBugReportRepository BugReports { get; }

        IUserAssignmentRepository UserAssignments { get; }

        // ── Component / Life-Limited Parts tracking (NEW) ──────────────────
        // All specialist (custom methods beyond generic CRUD) — same pattern
        // as AcMainGroups/InspectionTypes above.
        IComponentPositionRepository ComponentPositions { get; }
        IComponentTypeRepository ComponentTypes { get; }
        IComponentLifeLimitProfileRepository ComponentLifeLimitProfiles { get; }
        IComponentRepository Components { get; }
        IComponentEventRepository ComponentEvents { get; }
        IComponentLifeStatusRepository ComponentLifeStatuses { get; }
        /// <summary>NEW — hierarchy slot definitions (code/name/capacity per parent ComponentType).</summary>
        IComponentTypeSlotRepository ComponentTypeSlots { get; }
        /// <summary>NEW — hierarchy per-PN eligibility rows (which child PN(s) fit which ComponentTypeSlot).</summary>
        IComponentTypeSubAssemblySlotRepository ComponentTypeSubAssemblySlots { get; }
        /// <summary>NEW (Revision 12) — opening FH/Cycles/Landings/prior-overhaul baseline for a component received with pre-existing usage. Plain generic repo — no custom queries needed, always reached via Component.InitialReading for reads.</summary>
        IGenericRepository<ComponentInitialReading> ComponentInitialReadings { get; }
        /// <summary>NEW (Revision 13) — lookup of every life-limit dimension the system knows about (FH/Cycles/CalendarDays/TgoLandings/FullStopLandings, plus any future aircraft-specific counter added later as a new row — no schema change needed). Plain generic repo; code should switch on Code, never Id.</summary>
        IGenericRepository<ComponentLifeLimitDimensionType> ComponentLifeLimitDimensionTypes { get; }
        /// <summary>NEW — lookup of every "computation reference" a dimension can be measured from (SINCE_NEW/SINCE_OVERHAUL/SINCE_INSTALL/SINCE_FIRST_INSTALL — see ComponentReferenceBasis.cs). Plain generic repo; code should switch on Code, never Id, same convention as ComponentLifeLimitDimensionTypes.</summary>
        IGenericRepository<ComponentReferenceBasis> ComponentReferenceBases { get; }
        /// <summary>NEW (Derogation implementation pass) — append-only history of life-limit extensions/exceptions (see ComponentDerogation.cs). Specialist repo — GetByComponentTypeAsync only; no Update exposed at the service layer (corrections are a new row, never an edit).</summary>
        IComponentDerogationRepository ComponentDerogations { get; }

        // ════════════════════════════════════════════════════════════════
        // ── SquadronOps (Odv/Sortie planning) — REDESIGNED 2026-08-29 ──
        // Supersedes Batch 1's plain IGenericRepository<Odv>/<Squadron>
        // entries. Follows the same convention confirmed from the real
        // IWorkOrderRepository/WorkOrderRepository pair: specialist
        // interfaces extend IGenericRepository<T> (full CRUD) plus
        // hand-written Include-aware methods, registered directly
        // (non-lazy) in UnitOfWork's constructor. See each interface's own
        // comments for why it's shaped the way it is.
        // ════════════════════════════════════════════════════════════════
        IOdvRepository Odvs { get; }
        ISquadronRepository Squadrons { get; }
        ISortiePlanningRepository SortiePlanning { get; }

        // Plain generic — no custom queries needed for these yet (out of
        // scope for this redesign pass; revisit if/when their controllers
        // get the same treatment).
        IGenericRepository<Mission> Missions { get; }
        IGenericRepository<Phase> Phases { get; }
        IGenericRepository<CallSign> CallSigns { get; }
        IGenericRepository<CrewMember> CrewMembers { get; }
        IGenericRepository<SortieCrew> SortieCrews { get; }

        // NEW (2026-08-29, Batch 8) — "the authoritative record created once
        // a Sortie is completed" (FlightLog.cs's own doc comment). No custom
        // queries needed yet (Finalize only ever adds one row per Sortie) —
        // plain generic, same convention as Missions/Phases/SortieCrews
        // above rather than a specialist repository.
        IGenericRepository<FlightLog> FlightLogs { get; }

        // ── Commit ────────────────────────────────────────────────────────
        /// <summary>
        /// Flush all staged changes to the database in one transaction.
        /// Single method — use CompleteAsync() everywhere.
        /// </summary>
        Task<int> CompleteAsync();
    }
}
