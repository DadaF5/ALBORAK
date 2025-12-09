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


            // Enforce 1:1 relationship between Person and CrewMember by making PersonId unique
            modelBuilder.Entity<CrewMember>(b =>
            {
                b.HasKey(cm => cm.Id);

                b.HasIndex(cm => cm.PersonId).IsUnique();    // enforces 1:1

                b.HasOne(cm => cm.Person)
                    .WithOne(p => p.CrewMember)             // make sure Person has a CrewMember nav property
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
            
            // Odv configuration
            modelBuilder.Entity<Odv>(b =>
            {
                b.ToTable("Odvs");

                b.HasKey(x => x.Id);

                // date-only column
                b.Property(x => x.OdvDate)
                    .HasColumnType("date")
                    .IsRequired();

                // TimeSpan -> SQL time
                b.Property(x => x.TOFF)
                    .HasColumnType("time");

                // Audit columns
                b.Property(x => x.CreatedAtUtc)
                    .HasColumnType("datetime2")
                    .IsRequired();

                b.Property(x => x.UpdatedAtUtc)
                    .HasColumnType("datetime2");

               
                b.Property(x => x.Zone)
                    .HasConversion(zoneConverter)
                    .HasMaxLength(50)
                    .HasColumnName("Zone");

                b.Property(x => x.MissionType)
                    .HasConversion(missionTypeConverter)
                    .HasMaxLength(50)
                    .HasColumnName("MissionType");

                b.Property(x => x.OdvStatus)
                    .HasConversion(odvStatusConverter)
                    .HasMaxLength(50)
                    .HasColumnName("OdvStatus");

                // FKs naming (optional explicit configuration)
                b.HasOne(x => x.Squadron)
                    .WithMany() // adjust if Squadron entity has collection navigation
                    .HasForeignKey(x => x.SquadronId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Mission)
                    .WithMany()
                    .HasForeignKey(x => x.MissionId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.AcMainGroup)
                    .WithMany()
                    .HasForeignKey(x => x.AcMainGroupId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Sortie configuration
            modelBuilder.Entity<Sortie>(b =>
            {
                b.ToTable("Sorties");
                b.HasKey(s => s.Id);

                // FK to Odv
                b.HasOne(s => s.Odv)
                    .WithMany(o => o.Sorties)
                    .HasForeignKey(s => s.OdvId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Optional FK to Aircraft (restrict delete to avoid cascade removing sorties when removing aircraft)
                b.HasOne(s => s.Aircraft).WithMany().HasForeignKey(s => s.AircraftId).OnDelete(DeleteBehavior.Restrict);

                // column types
                b.Property(s => s.FuelQuantity).HasColumnType("decimal(10,2)"); // adjust precision as needed
                b.Property(s => s.StartTime).HasColumnType("datetime2");
                b.Property(s => s.LandingTime).HasColumnType("datetime2");
                b.Property(s => s.TOFF).HasColumnType("time");
                b.Property(s => s.IsCompleted).HasDefaultValue(false);
                b.Property(s => s.CompletedAtUtc).HasColumnType("datetime2");
            });
            // SortieCrew configuration
            modelBuilder.Entity<SortieCrew>(b =>
            {
                b.ToTable("SortieCrews");
                b.HasKey(c => c.Id);

                b.HasOne(c => c.Sortie).WithMany(s => s.SortieCrews).HasForeignKey(c => c.SortieId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(c => c.CrewMember).WithMany().HasForeignKey(c => c.CrewMemberId).OnDelete(DeleteBehavior.Restrict);

                b.Property(c => c.Role).HasMaxLength(100);
                b.Property(c => c.IsPrimary).HasDefaultValue(false);
                b.Property(c => c.Remarks).HasMaxLength(1000);
            });
            // --- Ensure Squadron and Mission have Odv navigations (if not already configured elsewhere) ---
            modelBuilder.Entity<Squadron>(entity =>
            {
                entity.HasMany(s => s.Odvs)
                      .WithOne(o => o.Squadron)
                      .HasForeignKey(o => o.SquadronId);
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
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Configuration).HasMaxLength(200);
                entity.Property(s => s.Notes).HasColumnType("nvarchar(max)");

                entity.HasOne(s => s.Odv)
                      .WithMany(o => o.Sorties)
                      .HasForeignKey(s => s.OdvId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Aircraft)
                      .WithMany(a => a.Sorties) // add ICollection<Sortie> Sorties in Aircraft model if desired
                      .HasForeignKey(s => s.AircraftId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(s => new { s.OdvId });
            });

            modelBuilder.Entity<SortieCrew>(entity =>
            {
                entity.HasKey(sc => sc.Id);

                entity.HasOne(sc => sc.Sortie)
                      .WithMany(s => s.SortieCrews)
                      .HasForeignKey(sc => sc.SortieId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sc => sc.CrewMember)
                      .WithMany() // optionally add navigation ICollection<SortieCrew> to Person
                      .HasForeignKey(sc => sc.Id)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(sc => new { sc.SortieId });
            });
        }
    }
}