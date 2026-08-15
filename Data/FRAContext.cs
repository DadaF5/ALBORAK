using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.HR.Models;
using FRAProject.Areas.Medical.Models;
using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Data.Configurations;
using FRAProject.Data.EntityConfigurations;
using FRAProject.Enums;
using FRAProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FRAProject.Data
{
    public partial class FRAContext : IdentityDbContext<ApplicationUser>
    {
        public FRAContext(DbContextOptions<FRAContext> options)
            : base(options)
        {
        }

        // =====================================
        // DOMAIN: HR (Human Resources)
        // =====================================
        public DbSet<Person> Persons { get; set; } = null!;
        public DbSet<Rank> Ranks { get; set; } = null!;
        public DbSet<RankType> RankTypes { get; set; } = null!;
        public DbSet<Base> Bases { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<SubDepartment> SubDepartments { get; set; } = null!;
        public DbSet<Wing> Wings { get; set; } = null!;
        public DbSet<Squadron> Squadrons { get; set; } = null!;

        // =====================================
        // DOMAIN: Squadron Operations
        // =====================================
        public DbSet<Odv> Odvs { get; set; } = null!;
        public DbSet<Sortie> Sorties { get; set; } = null!;
        public DbSet<SortieCrew> SortieCrews { get; set; } = null!;
        public DbSet<Mission> Missions { get; set; } = null!;
        public DbSet<Phase> Phases { get; set; } = null!;
        public DbSet<CallSign> CallSigns { get; set; } = null!;
        public DbSet<CrewMember> CrewMembers { get; set; } = null!;
        public DbSet<Qualification> Qualifications { get; set; } = null!;
        public DbSet<CrewMemberQualification> CrewMemberQualifications { get; set; } = null!;

        // =====================================
        // DOMAIN: Medical Care Center
        // =====================================
        public DbSet<MedicalCheck> MedicalChecks { get; set; } = null!;
        public DbSet<MedicalBilan> MedicalBilans { get; set; } = null!;

        // =====================================
        // DOMAIN: User Management (Cross-cutting)
        // =====================================
        public DbSet<UserDocument> UserDocuments { get; set; } = null!;
        public DbSet<UserQualification> UserQualifications { get; set; } = null!;
        public DbSet<MenuItem> MenuItems { get; set; } = null!;

        // =====================================
        // DOMAIN: Platform Access Control (NEW)
        // =====================================
        public DbSet<UserProfile> UserProfiles { get; set; } = null!;
        public DbSet<Module> Modules { get; set; } = null!;
        public DbSet<ModuleRole> ModuleRoles { get; set; } = null!;
        //public DbSet<UserAssignment> UserAssignments { get; set; } = null!;
        // FRAContext.cs — add DbSet
        public DbSet<UserAssignment> UserAssignments { get; set; } = null!;

        // =====================================
        // DOMAIN: Aircraft Maintenance
        // =====================================
        public DbSet<AcCategory> AcCategories { get; set; } = null!;
        public DbSet<AcMainGroup> AcMainGroups { get; set; } = null!;
        public DbSet<AcType> AcTypes { get; set; } = null!;
        public DbSet<AcStatusType> AcStatusTypes { get; set; } = null!;
        public DbSet<Aircraft> Aircrafts { get; set; } = null!;
        public DbSet<FlightLog> FlightLogs { get; set; } = null!;
        public DbSet<MaintenanceComponent> MaintenanceComponents { get; set; } = null!;
        public DbSet<MaintenanceThreshold> MaintenanceThresholds { get; set; } = null!;
        public DbSet<MaintenanceWorkOrder> MaintenanceWorkOrders { get; set; } = null!;
        public DbSet<AircraftDocumentType> AircraftDocumentTypes { get; set; } = null!;
        public DbSet<AircraftDocument> AircraftDocuments { get; set; } = null!;
        public DbSet<AtaCategory> AtaCategories { get; set; } = null!;
        public DbSet<Ata> Ata { get; set; } = null!;
        public DbSet<WorkOrderInspectionType> WorkOrderInspectionTypes { get; set; } = null!;
        // =====================================
        // Restrictions & Certificates
        // =====================================
        public DbSet<AircraftRestriction> AircraftRestrictions { get; set; } = null!;
        public DbSet<AircraftCertificate> AircraftCertificates { get; set; } = null!;

        // =====================================
        // DOMAIN: Aircraft Maintenance — Inspection Process
        // =====================================
        public DbSet<InspectionType> InspectionTypes { get; set; } = null!;
        public DbSet<MaintenanceProgram> MaintenancePrograms { get; set; } = null!;
        public DbSet<InspectionTypeProgram> InspectionTypePrograms { get; set; } = null!;
        public DbSet<JobCard> JobCards { get; set; } = null!;
        public DbSet<ProgramJobCard> ProgramJobCards { get; set; } = null!;
        public DbSet<JobCardPlanningRule> JobCardPlanningRules { get; set; } = null!;
        public DbSet<JobCardAttachment> JobCardAttachments { get; set; } = null!;
        public DbSet<InspectionState> InspectionStates { get; set; } = null!;
        public DbSet<AircraftJobCardState> AircraftJobCardStates { get; set; } = null!;
        public DbSet<WorkOrder> WorkOrders { get; set; } = null!;
        public DbSet<WorkOrderJobCard> WorkOrderJobCards { get; set; } = null!;
        public DbSet<WorkOrderJobCardSignOff> WorkOrderJobCardSignOffs { get; set; } = null!;
        public DbSet<WorkOrderSection> WorkOrderSections { get; set; } = null!;
        public DbSet<WorkOrderSectionPart> WorkOrderSectionParts { get; set; } = null!;
        public DbSet<WorkOrderSectionTask> WorkOrderSectionTasks { get; set; } = null!;
        public DbSet<WorkOrderSectionSignOff> WorkOrderSectionSignOffs { get; set; } = null!;

        // Aircraft snags and malfunctions
        public DbSet<Snag> Snags { get; set; } = null!;
        public DbSet<WorkOrderSnag> WorkOrderSnags { get; set; } = null!;

        // =====================================
        // Settings & Lookups
        // =====================================
        public DbSet<AircraftVersion> AircraftVersions { get; set; } = null!;
        public DbSet<AircraftManufacturer> AircraftManufacturers { get; set; } = null!;

        // ── Lookup tables — Form 5a / ImmatriculationDossier ─────────────
        public DbSet<Country> Countries { get; set; } = null!;
        public DbSet<EmployingAuthority> EmployingAuthorities { get; set; } = null!;
        public DbSet<CdnDocType> CdnDocTypes { get; set; } = null!;
        public DbSet<MissionRole> MissionRoles { get; set; } = null!;
        public DbSet<ImmatriculationDocType> ImmatriculationDocTypes { get; set; } = null!;

        // ── Immatriculation dossier ───────────────────────────────────────
        // FIX: One DbSet per entity — duplicate ImmatriculationDossiers removed.
        // "Dossiers" is the correct property name used throughout controllers.
        public DbSet<ImmatriculationDossier> Dossiers { get; set; } = null!;
        public DbSet<DossierAuthority> DossierAuthorities { get; set; } = null!;
        public DbSet<DossierAircraft> DossierAircrafts { get; set; } = null!;
        public DbSet<DossierAirworthiness> DossierAirworthiness { get; set; } = null!;
        public DbSet<ImmatriculationDocument> ImmatriculationDocuments { get; set; } = null!;

        // --===========================
        // DOMAIN: Support
        // --===========================
        public DbSet<BugReport> BugReports { get; set; } = null!;

        // ════════════════════════════════════════════════════════════════
        //  OnModelCreating
        // ════════════════════════════════════════════════════════════════
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── IEntityTypeConfiguration classes ─────────────────────────
            modelBuilder.ApplyConfiguration(new OdvConfiguration());
            modelBuilder.ApplyConfiguration(new SortieConfiguration());
            modelBuilder.ApplyConfiguration(new MissionConfiguration());
            modelBuilder.ApplyConfiguration(new PhaseConfiguration());
            modelBuilder.ApplyConfiguration(new SortieCrewConfiguration());

            // ── Lookup tables — Fluent API + seed data ────────────────────
            modelBuilder.ApplyConfiguration(new CountryConfiguration());
            modelBuilder.ApplyConfiguration(new EmployingAuthorityConfiguration());
            modelBuilder.ApplyConfiguration(new AcCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new CdnDocTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MissionRoleConfiguration());
            modelBuilder.ApplyConfiguration(new ImmatriculationDocTypeConfiguration());

            // Aircraft configurations
            modelBuilder.ApplyConfiguration(new AircraftConfiguration());

            // ── Immatriculation dossier — 5 configurations ───────────────
            // All class names prefixed with "ImmatriculationDossier" to avoid
            // conflicts with existing configuration classes in the project.
            modelBuilder.ApplyConfiguration(new ImmatriculationDossierConfiguration());
            modelBuilder.ApplyConfiguration(new ImmatriculationDossierAuthorityConfiguration());
            modelBuilder.ApplyConfiguration(new ImmatriculationDossierAircraftConfiguration());
            modelBuilder.ApplyConfiguration(new ImmatriculationDossierAirworthinessConfiguration());
            modelBuilder.ApplyConfiguration(new ImmatriculationDossierDocumentConfiguration());


            // Maintenance configurations
            modelBuilder.ApplyConfiguration(new AircraftCertificateConfiguration());
            modelBuilder.ApplyConfiguration(new AircraftRestrictionConfiguration());

            // Maintenance Inspection configuration
            // Inspection Process configurations
            modelBuilder.ApplyConfiguration(new InspectionTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MaintenanceProgramConfiguration());
            modelBuilder.ApplyConfiguration(new InspectionTypeProgramConfiguration());
            modelBuilder.ApplyConfiguration(new JobCardConfiguration());
            modelBuilder.ApplyConfiguration(new ProgramJobCardConfiguration());
            modelBuilder.ApplyConfiguration(new JobCardPlanningRuleConfiguration());
            modelBuilder.ApplyConfiguration(new JobCardAttachmentConfiguration());
            modelBuilder.ApplyConfiguration(new InspectionStateConfiguration());
            modelBuilder.ApplyConfiguration(new AircraftJobCardStateConfiguration());
            modelBuilder.ApplyConfiguration(new WorkOrderConfiguration());
            modelBuilder.ApplyConfiguration(new WorkOrderJobCardConfiguration());
            modelBuilder.ApplyConfiguration(new WorkOrderJobCardSignOffConfiguration());

            // ── Platform access control ───────────────────────────────────
            modelBuilder.ApplyConfiguration(new UserProfileConfiguration());
            modelBuilder.ApplyConfiguration(new ModuleConfiguration());
            modelBuilder.ApplyConfiguration(new ModuleRoleConfiguration());
            //modelBuilder.ApplyConfiguration(new UserAssignmentConfiguration());
            // FRAContext.cs — OnModelCreating, add this block.
            // No double-FK-into-same-parent risk here (every FK points to a
            // DIFFERENT parent type), so plain WithMany() with no navigation
            // collection needed on any of the five parent classes — unlike the
            // Snag/WorkOrder situation.
            modelBuilder.Entity<UserAssignment>(entity =>
            {
                entity.HasOne(ua => ua.User)
                    .WithMany()
                    .HasForeignKey(ua => ua.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ua => ua.ModuleRole)
                    .WithMany()
                    .HasForeignKey(ua => ua.ModuleRoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ua => ua.Base)
                    .WithMany()
                    .HasForeignKey(ua => ua.BaseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ua => ua.AcMainGroup)
                    .WithMany()
                    .HasForeignKey(ua => ua.AcMainGroupId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ua => ua.Wing)
                    .WithMany()
                    .HasForeignKey(ua => ua.WingId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            // Support for snags and bug reports
            modelBuilder.Entity<BugReport>()
                .HasOne(b => b.ReportedBy)
                .WithMany()
                .HasForeignKey(b => b.ReportedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BugReport>()
                .HasOne(b => b.ResolvedBy)
                .WithMany()
                .HasForeignKey(b => b.ResolvedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Decimal precision ─────────────────────────────────────────
            ConfigureDecimalPrecision(modelBuilder);

            // ── Maintenance & FlightLog ────────────────────────────────────
            ConfigureMaintenance(modelBuilder);
            ConfigureFlightLog(modelBuilder);

            // ── Partials ──────────────────────────────────────────────────
            ConfigureCallSign(modelBuilder);
            ConfigureMenus(modelBuilder);

            // ── CrewMember ────────────────────────────────────────────────
            modelBuilder.Entity<CrewMember>(b =>
            {
                b.HasKey(cm => cm.Id);
                b.HasIndex(cm => cm.PersonId).IsUnique();

                b.HasOne(cm => cm.Person)
                    .WithOne(p => p.CrewMember)
                    .HasForeignKey<CrewMember>(cm => cm.PersonId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Qualification ─────────────────────────────────────────────
            modelBuilder.Entity<Qualification>(b =>
            {
                b.HasKey(q => q.Id);
                b.Property(q => q.Name).HasMaxLength(100).IsRequired();
            });

            // ── CrewMemberQualification ────────────────────────────────────
            modelBuilder.Entity<CrewMemberQualification>(b =>
            {
                b.HasKey(cmq => cmq.Id);

                b.HasOne(cmq => cmq.CrewMember)
                    .WithMany(cm => cm.CrewMemberQualifications)
                    .HasForeignKey(cmq => cmq.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(cmq => cmq.Qualification)
                    .WithMany(q => q.CrewMemberQualifications)
                    .HasForeignKey(cmq => cmq.QualificationId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(cmq => new { cmq.CrewMemberId, cmq.QualificationId });
            });

            // ── MedicalCheck ──────────────────────────────────────────────
            modelBuilder.Entity<MedicalCheck>()
                .HasMany(mc => mc.Bilans)
                .WithOne(mb => mb.MedicalCheck)
                .HasForeignKey(mb => mb.MedicalCheckId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Wing → Squadron ────────────────────────────────────────────
            modelBuilder.Entity<Wing>()
                .HasMany(w => w.Squadrons)
                .WithOne(s => s.Wing)
                .HasForeignKey(s => s.WingId)
                .OnDelete(DeleteBehavior.Restrict);

            // AtaCategory → Ata (ATA Chapters) relationship
            modelBuilder.Entity<AtaCategory>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<Ata>()
                .HasOne(a => a.AtaCategory)
                .WithMany(c => c.AtaChapters)
                .HasForeignKey(a => a.AtaCategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<JobCard>()
               .HasOne(j => j.Ata)
               .WithMany()
               .HasForeignKey(j => j.AtaId)
               .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<WorkOrderInspectionType>()
                .HasOne(x => x.WorkOrder)
                .WithMany(w => w.WorkOrderInspectionTypes)
                .HasForeignKey(x => x.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);   // deleting a WO deletes its links

            modelBuilder.Entity<WorkOrderInspectionType>()
                .HasOne(x => x.InspectionType)
                .WithMany()
                .HasForeignKey(x => x.InspectionTypeId)
                .OnDelete(DeleteBehavior.Restrict);  // don't let this delete cascade into InspectionType

            modelBuilder.Entity<WorkOrderInspectionType>()
               .HasIndex(x => new { x.WorkOrderId, x.InspectionTypeId })
               .IsUnique();


            // ── AircraftDocumentType ───────────────────────────────────────
            modelBuilder.Entity<AircraftDocumentType>()
                .HasIndex(x => x.Code)
                .IsUnique();

            // ── AircraftManufacturer ───────────────────────────────────────
            modelBuilder.Entity<AircraftManufacturer>()
                .HasIndex(x => x.Code)
                .IsUnique();

            // ── AircraftVersion ────────────────────────────────────────────
            modelBuilder.Entity<AircraftVersion>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<AircraftVersion>(entity =>
            {
                entity.HasOne(av => av.AcType)
                    .WithMany(a => a.AircraftVersions)
                    .HasForeignKey(av => av.AcTypeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_AircraftVersions_AcType");

                entity.HasIndex(e => new { e.Code, e.AcTypeId })
                    .IsUnique()
                    .HasDatabaseName("UQ_AircraftVersions_Code_AcType");

                entity.HasIndex(e => new { e.Name, e.AcTypeId })
                    .IsUnique()
                    .HasDatabaseName("UQ_AircraftVersions_Name_AcType");
            });

            // ── Enum → string converters ───────────────────────────────────
            var zoneConverter = new EnumToStringConverter<Zone>();
            var missionTypeConverter = new EnumToStringConverter<MissionType>();
            var odvStatusConverter = new EnumToStringConverter<OdvStatus>();
        }

        // ════════════════════════════════════════════════════════════════
        //  PRIVATE CONFIGURE METHODS
        // ════════════════════════════════════════════════════════════════

        private void ConfigureFlightLog(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FlightLog>(e =>
            {
                e.HasKey(f => f.Id);

                e.HasOne(f => f.Aircraft)
                 .WithMany(a => a.FlightLogs)
                 .HasForeignKey(f => f.AircraftId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(f => f.Sortie)
                 .WithOne()
                 .HasForeignKey<FlightLog>(f => f.SortieId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.Property(f => f.HobbsStart).HasPrecision(8, 2);
                e.Property(f => f.HobbsEnd).HasPrecision(8, 2);
                e.Property(f => f.TachStart).HasPrecision(8, 2);
                e.Property(f => f.TachEnd).HasPrecision(8, 2);
                e.Property(f => f.FuelUsedKg).HasPrecision(10, 2);

                e.HasIndex(f => f.SortieId).IsUnique(false);
                e.HasIndex(f => f.AircraftId);
            });
        }

        private void ConfigureMaintenance(ModelBuilder modelBuilder)
        {
           
            modelBuilder.Entity<MaintenanceThreshold>(e =>
            {
                e.HasKey(t => t.Id);
                e.HasOne(t => t.Component)
                 .WithMany(c => c.Thresholds)
                 .HasForeignKey(t => t.ComponentId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MaintenanceWorkOrder>(e =>
            {
                e.HasKey(w => w.Id);
                e.HasOne(w => w.Aircraft)
                 .WithMany()
                 .HasForeignKey(w => w.AircraftId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(w => w.Component)
                 .WithMany()
                 .HasForeignKey(w => w.ComponentId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<WorkOrderSection>()
                .HasOne(x => x.WorkOrder)
                .WithMany()
                .HasForeignKey(x => x.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkOrderSection>()
                .HasOne(x => x.WorkSection)
                .WithMany()
                .HasForeignKey(x => x.WorkSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkOrderSectionPart>()
                .HasOne(x => x.WorkOrderSection)
                .WithMany()
                .HasForeignKey(x => x.WorkOrderSectionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkOrderSectionTask>()
                .HasOne(x => x.WorkOrderSection)
                .WithMany()
                .HasForeignKey(x => x.WorkOrderSectionId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WorkOrderSectionSignOff>()
                .HasOne(x => x.WorkOrderSection)
                .WithMany()
                .HasForeignKey(x => x.WorkOrderSectionId)
                .OnDelete(DeleteBehavior.Cascade);
            // FRAContext.cs — OnModelCreating, add this block
            modelBuilder.Entity<Snag>(entity =>
            {
                entity.HasOne(s => s.Aircraft)
                    .WithMany(a => a.Snags)
                    .HasForeignKey(s => s.AircraftId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Ata)
                    .WithMany(a => a.Snags)
                    .HasForeignKey(s => s.AtaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.DiscoveryBase)
                    .WithMany(b => b.Snags)
                    .HasForeignKey(s => s.DiscoveryBaseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.DiscoveredDuringWorkOrder)
                    .WithMany(w => w.DiscoveredSnags)
                    .HasForeignKey(s => s.DiscoveredDuringWorkOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.LinkedWorkOrder)
                    .WithMany(w => w.LinkedSnags)
                    .HasForeignKey(s => s.LinkedWorkOrderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<WorkOrderSnag>(entity =>
            {
                entity.HasOne(ws => ws.WorkOrder)
                    .WithMany(w => w.WorkOrderSnags)
                    .HasForeignKey(ws => ws.WorkOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ws => ws.Snag)
                    .WithMany(s => s.WorkOrderSnags)
                    .HasForeignKey(ws => ws.SnagId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }


        private void ConfigureDecimalPrecision(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sortie>(e =>
            {
                e.Property(s => s.FuelQuantity).HasPrecision(10, 2);
            });

            modelBuilder.Entity<FlightLog>(e =>
            {
                e.Property(f => f.HobbsStart).HasPrecision(8, 2);
                e.Property(f => f.HobbsEnd).HasPrecision(8, 2);
                e.Property(f => f.TachStart).HasPrecision(8, 2);
                e.Property(f => f.TachEnd).HasPrecision(8, 2);
                e.Property(f => f.FuelUsedKg).HasPrecision(10, 2);
            });

            modelBuilder.Entity<MaintenanceComponent>(e =>
            {
                // Add decimal fields here as needed
            });
        }
    }
}