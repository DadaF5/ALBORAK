using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using FRAProject.Models;
using FRAProject.Enums;


namespace FRAProject.Data
{
    public class FRAContext : DbContext
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

        //===============================
        // Aircraft Related DbSets
        //===============================
        public DbSet<AcCategory> AcCategories { get; set; } = null!;
        public DbSet<AcMainGroup> AcMainGroups { get; set; } = null!;
        public DbSet<AcType> AcTypes { get; set; } = null!;
        public DbSet<AcStatusType> AcStatusTypes { get; set; } = null!;
        public DbSet<Aircraft> Aircrafts { get; set; } = null!;

        // =============================
        // Air activity Related DbSets
        // =============================
        public DbSet<Mission> Missions { get; set; } = null!;
        public DbSet<Phase> Phases { get; set; } = null!;

        // Scheduling / assignments table (ODV)
        public DbSet<Odv> Odvs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
            });

            // Additional model configuration (Wings, Bases, AcMainGroups, etc.) should be kept
            // in their respective configuration areas if you split configuration into partials.
        }
    }
}