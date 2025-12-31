using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using FRAProject.Models;
using FRAProject.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using FRAProject.Data.EntityConfigurations;

namespace FRAProject.Data
{
    public partial class FRAContext : IdentityDbContext<ApplicationUser>
    {
        public FRAContext(DbContextOptions<FRAContext> options)
            : base(options)
        {
        }

        // =====================================
        // DbSets (Base, Department and Person related DbSets)
        // =====================================
        public DbSet<Person> Persons { get; set; } = null!;
        public DbSet<Rank> Ranks { get; set; } = null!;
        public DbSet<RankType> RankTypes { get; set; } = null!;
        public DbSet<Base> Bases { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<SubDepartment> SubDepartments { get; set; } = null!;
        public DbSet<Wing> Wings { get; set; } = null!;
        public DbSet<Squadron> Squadrons { get; set; } = null!;

        // Scheduling / assignments table (ODV)
        public DbSet<Odv> Odvs { get; set; } = null!;
        public DbSet<Sortie> Sorties { get; set; } = null!;
        public DbSet<SortieCrew> SortieCrews { get; set; } = null!;

        // =============================
        // Air activity Related DbSets
        // =============================
        public DbSet<Mission> Missions { get; set; } = null!;
        public DbSet<Phase> Phases { get; set; } = null!;
        public DbSet<CallSign> CallSigns { get; set; } = null!;
        public DbSet<MenuItem> MenuItems { get; set; } = null!;

        // =============================
        // Crew Member Related DbSets
        public DbSet<CrewMember> CrewMembers { get; set; } = null!;
        public DbSet<Qualification> Qualifications { get; set; } = null!;
        public DbSet<CrewMemberQualification> CrewMemberQualifications { get; set; } = null!;

        public DbSet<MedicalCheck> MedicalChecks { get; set; } = null!;
        public DbSet<MedicalBilan> MedicalBilans { get; set; } = null!;

        //===============================
        // User Related DbSets
        public DbSet<UserDocument> UserDocuments { get; set; } = null!;
        public DbSet<UserQualification> UserQualifications { get; set; } = null!;

        //===============================
        // Aircraft Related DbSets
        //===============================
        public DbSet<AcCategory> AcCategories { get; set; } = null!;
        public DbSet<AcMainGroup> AcMainGroups { get; set; } = null!;
        public DbSet<AcType> AcTypes { get; set; } = null!;
        public DbSet<AcStatusType> AcStatusTypes { get; set; } = null!;
        public DbSet<Aircraft> Aircrafts { get; set; } = null!;
        public DbSet<FlightLog> FlightLogs { get; set; } = null!;
        public DbSet<MaintenanceComponent> MaintenanceComponents { get; set; } = null!;
        public DbSet<MaintenanceThreshold> MaintenanceThresholds { get; set; } = null!;
        public DbSet<MaintenanceWorkOrder> MaintenanceWorkOrders { get; set; } = null!;

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
                .OnDelete(DeleteBehavior.Cascade) ;

               

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