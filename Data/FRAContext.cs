using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using FRAProject.Models;
using FRAProject.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


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




        public DbSet<Sortie> Sorties { get; set; } = null!;
        public DbSet<SortieCrew> SortieCrews { get; set; } = null!;

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
            modelBuilder.Entity<FlightLog>(e =>
            {
                e.HasKey(f => f.Id);
                e.HasOne(f => f.Sortie).WithOne().HasForeignKey<FlightLog>(f => f.SortieId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(f => f.Aircraft).WithMany().HasForeignKey(f => f.AircraftId).OnDelete(DeleteBehavior.Restrict);
            });

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
        // =============================
        // Air activity Related DbSets
        // =============================
        public DbSet<Mission> Missions { get; set; } = null!;
        public DbSet<Phase> Phases { get; set; } = null!;
        public DbSet<CallSign> CallSigns { get; set; } = null!;
        public DbSet<MenuItem> MenuItems { get; set; } = null!;

        // Scheduling / assignments table (ODV)
        public DbSet<Odv> Odvs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Decimal precision configuration in partial class
            ConfigureDecimalPrecision(modelBuilder);

            // Sortie configuration in partial class
            ConfigureSorties(modelBuilder);

            // Maintenance configuration in partial class
            ConfigureMaintenance(modelBuilder);

            // FlightLog configuration in partial class
            ConfigureFlightLog(modelBuilder);

            // CallSign configuration in partial class
            ConfigureCallSign (modelBuilder);

            // Menu configuration in partial class
            ConfigureMenus(modelBuilder);



            // ===== general hints / TODOs =====
            // - Add further indexes / constraints as your domain requires.
            // - If any entity already contains navigation/configuration via Data Annotations,
            //   duplicate Fluent config is not required.

            // Prevent duplicates : tailNo + RegistrationNumber + IntCode per AcType
            // (Add index configuration for Aircraft if needed)

            // Wing -> Squadron: prevent cascade delete so deleting a Wing won't delete Squadrons
            modelBuilder.Entity<Wing>()
                .HasMany(w => w.Squadrons)
                .WithOne(s => s.Wing)
                .HasForeignKey(s => s.WingId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Mission -> Phase (required) and optional unique index for Code per Phase ---
            modelBuilder.Entity<Mission>(entity =>
            {
                entity.HasOne(m => m.Phase)
                      .WithMany(p => p.Missions)
                      .HasForeignKey(m => m.PhaseId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Optional unique index on (PhaseId, Code) where Code is not null.
                // HasFilter uses SQL Server syntax; remove or adapt for other providers.
                entity.HasIndex(m => new { m.PhaseId, m.Code })
                      .IsUnique()
                      .HasFilter("[Code] IS NOT NULL");
            });

            // --- Enum-to-string converters for Odv enum-backed fields ---
            var zoneConverter = new EnumToStringConverter<Zone>();
            var missionTypeConverter = new EnumToStringConverter<MissionType>();
            var odvStatusConverter = new EnumToStringConverter<OdvStatus>();

            // --- ODV mapping ---
            modelBuilder.Entity<Odv>(entity =>
            {
                entity.ToTable("ODV");
                entity.HasKey(o => o.OdvID);

                entity.Property(o => o.OdvID).HasColumnName("OdvID");
                entity.Property(o => o.SquadronID).HasColumnName("SquadronID");
                entity.Property(o => o.MissionId).HasColumnName("MissionId");

                // Date-only mapping
                entity.Property(o => o.OdvDate)
                      .HasColumnType("date")
                      .IsRequired();

                // time(7) mapping
                entity.Property(o => o.TOFF)
                      .HasColumnType("time(7)");

                // Area (free-text), CallSignId, Obs
                entity.Property(o => o.Area)
                      .HasMaxLength(200)
                      .HasColumnType("nvarchar(200)")
                      .IsRequired();

                entity.Property(o => o.CallSignId)
                      .HasMaxLength(20)
                      .HasColumnType("nvarchar(20)");

                entity.Property(o => o.Obs)
                      .HasColumnType("nvarchar(max)");

                // AcMainGroup snapshot
                entity.Property(o => o.AcMainGroupID)
                      .HasColumnName("AcMainGroupID")
                      .IsRequired();

                // Enum-backed saved as strings (readable DB values)
                entity.Property(o => o.ZoneID)
                      .HasConversion(zoneConverter)
                      .HasMaxLength(20)
                      .HasColumnType("nvarchar(20)")
                      .IsRequired();

                entity.Property(o => o.MissionTypeID)
                      .HasConversion(missionTypeConverter)
                      .HasMaxLength(50)
                      .HasColumnType("nvarchar(50)")
                      .IsRequired();

                entity.Property(o => o.OdvStatus)
                      .HasConversion(odvStatusConverter)
                      .HasMaxLength(50)
                      .HasColumnType("nvarchar(50)")
                      .IsRequired(false)
                      .HasDefaultValue(OdvStatus.Planned);
                

                // Relationships (use Restrict to avoid accidental cascade deletes)
                entity.HasOne(o => o.Squadron)
                      .WithMany(s => s.Odvs)
                      .HasForeignKey(o => o.SquadronID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Mission)
                      .WithMany(m => m.Odvs)
                      .HasForeignKey(o => o.MissionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.AcMainGroup)
                      .WithMany(amg => amg.Odvs)
                      .HasForeignKey(o => o.AcMainGroupID)
                      .OnDelete(DeleteBehavior.Restrict);

                // Helpful indexes for common queries
                entity.HasIndex(o => new { o.SquadronID, o.OdvDate });
                entity.HasIndex(o => new { o.MissionId, o.OdvDate });
                entity.HasIndex(o => new { o.AcMainGroupID, o.OdvDate });
            });

            // --- Ensure Squadron and Mission have Odv navigations (if not already configured elsewhere) ---
            modelBuilder.Entity<Squadron>(entity =>
            {
                entity.HasMany(s => s.Odvs)
                      .WithOne(o => o.Squadron)
                      .HasForeignKey(o => o.SquadronID);
            });

            modelBuilder.Entity<Mission>(entity =>
            {
                entity.HasMany(m => m.Odvs)
                      .WithOne(o => o.Mission)
                      .HasForeignKey(o => o.MissionId);
            }
            

            
            
            );
            

        // Additional model configuration (Wings, Bases, AcMainGroups, etc.) should be kept

        // in their respective configuration areas if you split configuration into partials.
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

        // In your OnModelCreating (or a partial), add:
        private void ConfigureSorties(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sortie>(entity =>
            {
                entity.HasKey(s => s.SortieId);
                entity.Property(s => s.Configuration).HasMaxLength(200);
                entity.Property(s => s.Notes).HasColumnType("nvarchar(max)");

                entity.HasOne(s => s.Odv)
                      .WithMany(o => o.Sorties)
                      .HasForeignKey(s => s.OdvID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Aircraft)
                      .WithMany(a => a.Sorties) // add ICollection<Sortie> Sorties in Aircraft model if desired
                      .HasForeignKey(s => s.AircraftId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(s => new { s.OdvID });
            });

            modelBuilder.Entity<SortieCrew>(entity =>
            {
                entity.HasKey(sc => sc.SortieCrewId);

                entity.HasOne(sc => sc.Sortie)
                      .WithMany(s => s.CrewMembers)
                      .HasForeignKey(sc => sc.SortieId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sc => sc.Person)
                      .WithMany() // optionally add navigation ICollection<SortieCrew> to Person
                      .HasForeignKey(sc => sc.PersonId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(sc => new { sc.SortieId });
            });
        }
    }
}