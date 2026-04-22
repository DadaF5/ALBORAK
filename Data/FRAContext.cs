using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.HR.Models;
using FRAProject.Areas.Medical.Models;
using FRAProject.Areas.Settings;
using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.SquadronOps.Models;
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
        // Manages employee records, organizational structure, and personnel hierarchy
        // =====================================
        public DbSet<Person> Persons { get; set; } = null!;                      // Employee records
        public DbSet<Rank> Ranks { get; set; } = null!;                          // Military/organizational ranks
        public DbSet<RankType> RankTypes { get; set; } = null!;                  // Rank categories
        public DbSet<Base> Bases { get; set; } = null!;                          // Military bases/locations
        public DbSet<Department> Departments { get; set; } = null!;              // Organizational departments
        public DbSet<SubDepartment> SubDepartments { get; set; } = null!;        // Sub-organizational units
        public DbSet<Wing> Wings { get; set; } = null!;                          // Wing-level organization
        public DbSet<Squadron> Squadrons { get; set; } = null!;                  // Squadron-level organization

        // =====================================
        // DOMAIN: Squadron Operations
        // Manages flight scheduling, missions, and operational planning
        // Links: Personnel (CrewMembers) → Sorties → Aircraft
        // =====================================
        public DbSet<Odv> Odvs { get; set; } = null!;                           // Operational Daily Flight schedule
        public DbSet<Sortie> Sorties { get; set; } = null!;                     // Flight missions/sorties
        public DbSet<SortieCrew> SortieCrews { get; set; } = null!;             // Crew assignments to sorties
        public DbSet<Mission> Missions { get; set; } = null!;                   // Mission types (training, combat, etc.)
        public DbSet<Phase> Phases { get; set; } = null!;                       // Mission phases
        public DbSet<CallSign> CallSigns { get; set; } = null!;                 // Radio call signs
        public DbSet<CrewMember> CrewMembers { get; set; } = null!;             // Flight crew personnel
        public DbSet<Qualification> Qualifications { get; set; } = null!;       // Crew qualifications/certifications
        public DbSet<CrewMemberQualification> CrewMemberQualifications { get; set; } = null!; // Links crew to qualifications

        // =====================================
        // DOMAIN: Medical Care Center
        // Manages crew member medical fitness, examinations, and health records
        // Links: CrewMembers → MedicalChecks → Medical fitness decisions
        // =====================================
        public DbSet<MedicalCheck> MedicalChecks { get; set; } = null!;         // Medical examination records
        public DbSet<MedicalBilan> MedicalBilans { get; set; } = null!;         // Detailed medical assessment results

        // =====================================
        // DOMAIN: User Management (Cross-cutting)
        // User-specific documents and qualifications for Identity users
        // =====================================
        public DbSet<UserDocument> UserDocuments { get; set; } = null!;         // User uploaded documents
        public DbSet<UserQualification> UserQualifications { get; set; } = null!; // User qualifications
        public DbSet<MenuItem> MenuItems { get; set; } = null!;                 // Application menu structure

        // =====================================
        // DOMAIN: Aircraft Maintenance
        // Manages aircraft inventory, maintenance tracking, and serviceability
        // Links: Aircraft → MaintenanceWorkOrders → Components → Flight availability
        // =====================================
        public DbSet<AcCategory> AcCategories { get; set; } = null!;            // Aircraft categories
        public DbSet<AcMainGroup> AcMainGroups { get; set; } = null!;           // Aircraft main groups
        public DbSet<AcType> AcTypes { get; set; } = null!;                     // Aircraft types/models
        public DbSet<AcStatusType> AcStatusTypes { get; set; } = null!;         // Aircraft status types
        public DbSet<Aircraft> Aircrafts { get; set; } = null!;                 // Aircraft inventory
        public DbSet<FlightLog> FlightLogs { get; set; } = null!;               // Flight hour/cycle tracking
        public DbSet<MaintenanceComponent> MaintenanceComponents { get; set; } = null!;     // Aircraft components
        public DbSet<MaintenanceThreshold> MaintenanceThresholds { get; set; } = null!;     // Maintenance intervals
        public DbSet<MaintenanceWorkOrder> MaintenanceWorkOrders { get; set; } = null!;     // Maintenance work orders
        public DbSet<AircraftDocumentType> AircraftDocumentTypes { get; set; } = null!;
        public DbSet<AircraftDocument> AircraftDocuments { get; set; } = null!; // if you have this entity too


        // =====================================
        // Settings & Lookups (dbo)
        public DbSet<AircraftVersion> AircraftVersions { get; set; } = null!;     // Lookup for aircraft versions
        public DbSet<AircraftManufacturer> AircraftManufacturers { get; set; } = null!;

        
        //public DbSet<MaintenanceType> MaintenanceTypes { get; set; } = null!;
        //public DbSet<EngineType> EngineTypes { get; set; } = null!;
        //public DbSet<SgsEventType> SgsEventTypes { get; set; } = null!;
        //public DbSet<RiskLevel> RiskLevels { get; set; } = null!;
        //public DbSet<FluidType> FluidTypes { get; set; } = null!;

        private void ConfigureFlightLog(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FlightLog>(e =>
            {
                e.HasKey(f => f.Id);

                // Make sure EF uses the declared AircraftId property as the FK and the Aircraft.FlightLogs inverse nav
                e.HasOne(f => f.Aircraft)
                 .WithMany(a => a.FlightLogs)     // requires Aircraft.FlightLogs nav (you have it)
                 .HasForeignKey(f => f.AircraftId)
                 .OnDelete(DeleteBehavior.Restrict);

                // Map Sortie relation explicitly to avoid ambiguity if Sortie has navs
                e.HasOne(f => f.Sortie)
                 .WithOne() // or .WithOne(s => s.FlightLog) if Sortie has a FlightLog nav
                 .HasForeignKey<FlightLog>(f => f.SortieId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Decimal precision (avoid warnings)
                e.Property(f => f.HobbsStart).HasPrecision(8, 2);
                e.Property(f => f.HobbsEnd).HasPrecision(8, 2);
                e.Property(f => f.TachStart).HasPrecision(8, 2);
                e.Property(f => f.TachEnd).HasPrecision(8, 2);
                e.Property(f => f.FuelUsedKg).HasPrecision(10, 2);

                // Optional: index on SortieId or AircraftId
                e.HasIndex(f => f.SortieId).IsUnique(false);
                e.HasIndex(f => f.AircraftId);
            });
        }

        private void ConfigureMaintenance(ModelBuilder modelBuilder)
        {
            // Configure maintenance related entities (do not duplicate FlightLog mapping here)
            modelBuilder.Entity<MaintenanceComponent>(e =>
            {
                e.HasKey(c => c.Id);
                e.HasOne(c => c.Aircraft).WithMany(a => a.Components).HasForeignKey(c => c.AircraftId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MaintenanceThreshold>(e =>
            {
                e.HasKey(t => t.Id);
                e.HasOne(t => t.Component).WithMany(c => c.Thresholds).HasForeignKey(t => t.ComponentId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MaintenanceWorkOrder>(e =>
            {
                e.HasKey(w => w.Id);
                e.HasOne(w => w.Aircraft).WithMany().HasForeignKey(w => w.AircraftId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(w => w.Component).WithMany().HasForeignKey(w => w.ComponentId).OnDelete(DeleteBehavior.Restrict);
            });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Use the dedicated configuration classes / partials for entity configuration.
            // Do not duplicate relationship configuration in multiple places.

            modelBuilder.ApplyConfiguration(new OdvConfiguration());
            modelBuilder.ApplyConfiguration(new SortieConfiguration());
            modelBuilder.ApplyConfiguration(new MissionConfiguration());
            modelBuilder.ApplyConfiguration(new PhaseConfiguration());
            modelBuilder.ApplyConfiguration(new SortieCrewConfiguration());
            // Decimal precision configuration
            ConfigureDecimalPrecision(modelBuilder);

            // Maintenance & FlightLog configuration (single canonical methods)
            ConfigureMaintenance(modelBuilder);
            ConfigureFlightLog(modelBuilder);

            // CallSign configuration in partial class
            ConfigureCallSign(modelBuilder);

            // Menu configuration in partial class
            ConfigureMenus(modelBuilder);

            // Enforce 1:1 relationship between Person and CrewMember by making PersonId unique
            modelBuilder.Entity<CrewMember>(b =>
            {
                b.HasKey(cm => cm.Id);

                b.HasIndex(cm => cm.PersonId).IsUnique();    // enforces 1:1

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


            modelBuilder.Entity<Qualification>(b =>
            {
                b.HasKey(q => q.Id);
                b.Property(q => q.Name).HasMaxLength(100).IsRequired();
            });

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

            modelBuilder.Entity<MedicalCheck>()
                .HasMany(mc => mc.Bilans)
                .WithOne(mb => mb.MedicalCheck)
                .HasForeignKey(mb => mb.MedicalCheckId)
                .OnDelete(DeleteBehavior.Cascade);



            // Wing -> Squadron: prevent cascade delete so deleting a Wing won't delete Squadrons
            modelBuilder.Entity<Wing>()
                .HasMany(w => w.Squadrons)
                .WithOne(s => s.Wing)
                .HasForeignKey(s => s.WingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Aircraft>()
                .HasOne(a => a.AcType)
                .WithMany(t => t.Aircrafts)
                .HasForeignKey(a => a.AcTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AircraftDocumentType>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<AircraftManufacturer>()
                .HasIndex(x => x.Code)
                .IsUnique();
            modelBuilder.Entity<AircraftVersion>()
                .HasIndex(x => x.Code)
                .IsUnique();
            modelBuilder.Entity<AircraftVersion>(entity =>
            {
                // LookupBase already handles: Id, Code, Name, Description, IsActive, SortOrder

                // FK relationship - MUST match collection name in AcType
                entity.HasOne(av => av.AcType)
                    .WithMany(a => a.AircraftVersions)  // ← Must match property name
                    .HasForeignKey(av => av.AcTypeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_AircraftVersions_AcType");

                // Unique constraint on Code per AcType
                entity.HasIndex(e => new { e.Code, e.AcTypeId })
                    .IsUnique()
                    .HasDatabaseName("UQ_AircraftVersions_Code_AcType");

                // Unique constraint on Name per AcType
                entity.HasIndex(e => new { e.Name, e.AcTypeId })
                    .IsUnique()
                    .HasDatabaseName("UQ_AircraftVersions_Name_AcType");
            });
            // --- Enum-to-string converters for Odv enum-backed fields ---
            var zoneConverter = new EnumToStringConverter<Zone>();
            var missionTypeConverter = new EnumToStringConverter<MissionType>();
            var odvStatusConverter = new EnumToStringConverter<OdvStatus>();
        }

        private void ConfigureDecimalPrecision(ModelBuilder modelBuilder)
        {
            // Sortie fuel
            modelBuilder.Entity<Sortie>(e =>
            {
                e.Property(s => s.FuelQuantity).HasPrecision(10, 2);
            });

            // FlightLog hobbs/tach/fuel
            modelBuilder.Entity<FlightLog>(e =>
            {
                e.Property(f => f.HobbsStart).HasPrecision(8, 2);
                e.Property(f => f.HobbsEnd).HasPrecision(8, 2);
                e.Property(f => f.TachStart).HasPrecision(8, 2);
                e.Property(f => f.TachEnd).HasPrecision(8, 2);
                e.Property(f => f.FuelUsedKg).HasPrecision(10, 2);
            });

            // Maintenance component numeric values if present
            modelBuilder.Entity<MaintenanceComponent>(e =>
            {
                // if you have any decimal fields here, configure them too:
                // e.Property(c => c.SomeDecimalField).HasPrecision(9, 2);
            });

            // Add other decimal properties here as needed
        }

        
    }
}