using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.HR.Models;
using FRAProject.Areas.Medical.Models;
using FRAProject.Areas.Settings;
using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Data.Configurations;
using FRAProject.Data.EntityConfigurations;
using FRAProject.Enums;
using FRAProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

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

        // =====================================
        // Settings & Lookups
        // =====================================
        public DbSet<AircraftVersion> AircraftVersions { get; set; } = null!;
        public DbSet<AircraftManufacturer> AircraftManufacturers { get; set; } = null!;

        // ── Lookup tables — Form 5a / ImmatriculationDossier feature ─────
        public DbSet<Country> Countries { get; set; } = null!;
        public DbSet<EmployingAuthority> EmployingAuthorities { get; set; } = null!;
        //public DbSet<CdnDocType> CdnDocTypes { get; set; } = null!;
        //public DbSet<MissionRole> MissionRoles { get; set; } = null!;
        //public DbSet<ImmatriculationDocType> ImmatriculationDocTypes { get; set; } = null!;

        //// ── Immatriculation dossier ───────────────────────────────────────
        //public DbSet<ImmatriculationDossier> Dossiers { get; set; } = null!;
        //public DbSet<DossierAuthority> DossierAuthorities { get; set; } = null!;
        //public DbSet<DossierAircraft> DossierAircrafts { get; set; } = null!;
        //public DbSet<DossierAirworthiness> DossierAirworthiness { get; set; } = null!;
        //public DbSet<ImmatriculationDocument> ImmatriculationDocuments { get; set; } = null!;

        // Uncomment when ImmatriculationDossier is built:
        // public DbSet<ImmatriculationDossier>  ImmatriculationDossiers  { get; set; } = null!;
        // public DbSet<ImmatriculationDocument> ImmatriculationDocuments { get; set; } = null!;

        //public DbSet<MaintenanceType> MaintenanceTypes { get; set; } = null!;
        //public DbSet<EngineType> EngineTypes { get; set; } = null!;
        //public DbSet<SgsEventType> SgsEventTypes { get; set; } = null!;
        //public DbSet<RiskLevel> RiskLevels { get; set; } = null!;
        //public DbSet<FluidType> FluidTypes { get; set; } = null!;

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

            // ── NEW: Country — Fluent API + seed data ─────────────────────
            modelBuilder.ApplyConfiguration(new CountryConfiguration());

            // ── NEW: EmployingAuthority — Fluent API + seed data ──────────
            modelBuilder.ApplyConfiguration(new EmployingAuthorityConfiguration());

            // ── NEW: AcCategory — Fluent API + seed data ──────────────────
            modelBuilder.ApplyConfiguration(new AcCategoryConfiguration());

            //// ── NEW: CdnDocType — Fluent API + seed data ──────────────────
            //modelBuilder.ApplyConfiguration(new CdnDocTypeConfiguration());

            //// ── NEW: MissionRole — Fluent API + seed data ─────────────────
            //modelBuilder.ApplyConfiguration(new MissionRoleConfiguration());

            //// ── NEW: ImmatriculationDocType — Fluent API + seed data ──────
            //// All 6 lookup tables now complete.
            //modelBuilder.ApplyConfiguration(new ImmatriculationDocTypeConfiguration());

            //// ── Immatriculation dossier — 4 configurations ────────────────
            //// DossierConfiguration handles the shared PK (1:1) pattern
            //// and all FK relationships across the 4 tables.
            //modelBuilder.ApplyConfiguration(new DossierConfiguration());
            //modelBuilder.ApplyConfiguration(new DossierAuthorityConfiguration());
            //modelBuilder.ApplyConfiguration(new DossierAircraftConfiguration());
            //modelBuilder.ApplyConfiguration(new DossierAirworthinessConfiguration());
            //modelBuilder.ApplyConfiguration(new ImmatriculationDocumentConfiguration());

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

                b.HasOne(cm => cm.Squadron)
                    .WithMany(s => s.CrewMembers)
                    .HasForeignKey(cm => cm.SquadronId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(cm => cm.PrimaryQualification)
                    .WithMany()
                    .HasForeignKey(cm => cm.PrimaryQualificationId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<CrewMember>()
                .HasMany(cm => cm.MedicalChecks)
                .WithOne(mc => mc.CrewMember)
                .HasForeignKey(mc => mc.CrewMemberId)
                .OnDelete(DeleteBehavior.Cascade);

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

            // ── Aircraft ───────────────────────────────────────────────────
            modelBuilder.Entity<Aircraft>()
                .HasOne(a => a.AcType)
                .WithMany(t => t.Aircrafts)
                .HasForeignKey(a => a.AcTypeId)
                .OnDelete(DeleteBehavior.Restrict);

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
            modelBuilder.Entity<MaintenanceComponent>(e =>
            {
                e.HasKey(c => c.Id);
                e.HasOne(c => c.Aircraft)
                 .WithMany(a => a.Components)
                 .HasForeignKey(c => c.AircraftId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

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