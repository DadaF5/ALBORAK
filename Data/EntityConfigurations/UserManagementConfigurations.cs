using FRAProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.Configurations
{
    // ════════════════════════════════════════════════════════════════════
    //  UserProfileConfiguration
    // ════════════════════════════════════════════════════════════════════
    /// <summary>
    /// Applied in FRAContext via:
    ///   modelBuilder.ApplyConfiguration(new UserProfileConfiguration());
    ///
    /// Also add to FRAContext:
    ///   public DbSet&lt;UserProfile&gt; UserProfiles { get; set; } = null!;
    ///
    /// Requires ApplicationUser.Profile navigation property to exist.
    /// </summary>
    public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.ToTable("UserProfiles");

            // PK = FK — UserId is both primary key and foreign key
            builder.HasKey(p => p.UserId);

            builder.Property(p => p.UserId)
                .HasColumnType("nvarchar(450)")
                .IsRequired();

            // One-to-one with ApplicationUser
            builder.HasOne(p => p.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_UserProfiles_AspNetUsers");

            builder.Property(p => p.FullOfficialName)
                .HasColumnType("nvarchar(200)")
                .IsRequired(false);

            builder.Property(p => p.Specialty)
                .HasColumnType("nvarchar(20)")
                .IsRequired(false);

            builder.Property(p => p.LMAMNumber)
                .HasColumnType("nvarchar(50)")
                .IsRequired(false);

            builder.Property(p => p.Section)
                .HasColumnType("nvarchar(100)")
                .IsRequired(false);

            builder.Property(p => p.InternalPhone)
                .HasColumnType("nvarchar(30)")
                .IsRequired(false);

            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  ModuleConfiguration
    // ════════════════════════════════════════════════════════════════════
    /// <summary>
    /// Applied in FRAContext via:
    ///   modelBuilder.ApplyConfiguration(new ModuleConfiguration());
    ///
    /// Also add to FRAContext:
    ///   public DbSet&lt;Module&gt; Modules { get; set; } = null!;
    /// </summary>
    public class ModuleConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            builder.ToTable("Modules");

            // PK is string Code — not int
            builder.HasKey(m => m.Code);

            builder.Property(m => m.Code)
                .HasColumnType("nvarchar(20)")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(m => m.Name)
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(m => m.Description)
                .HasColumnType("nvarchar(250)")
                .IsRequired(false);

            builder.Property(m => m.IconClass)
                .HasColumnType("nvarchar(50)")
                .IsRequired(false);

            builder.Property(m => m.IsActive)
                .HasDefaultValue(true);

            builder.Property(m => m.SortOrder)
                .HasDefaultValue((byte)99);

            // ── Seed data ────────────────────────────────────────────────
            builder.HasData(
                new Module
                {
                    Code = "MAINTENANCE",
                    Name = "Maintenance Aéronefs",
                    Description = "Maintenance planifiée et corrective",
                    IconClass = "fas fa-wrench",
                    IsActive = true,
                    SortOrder = 10
                },
                new Module
                {
                    Code = "HR",
                    Name = "Ressources Humaines",
                    Description = "Gestion du personnel",
                    IconClass = "fas fa-users",
                    IsActive = true,
                    SortOrder = 20
                },
                new Module
                {
                    Code = "HEALTHCARE",
                    Name = "Service Médical",
                    Description = "Suivi médical du personnel navigant",
                    IconClass = "fas fa-heartbeat",
                    IsActive = true,
                    SortOrder = 30
                },
                new Module
                {
                    Code = "SQUADRONOPS",
                    Name = "Opérations Escadron",
                    Description = "Planification et suivi des sorties",
                    IconClass = "fas fa-plane-departure",
                    IsActive = true,
                    SortOrder = 40
                },
                new Module
                {
                    Code = "SETTINGS",
                    Name = "Administration Système",
                    Description = "Paramétrage de la plateforme",
                    IconClass = "fas fa-cog",
                    IsActive = true,
                    SortOrder = 99
                }
            );
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  ModuleRoleConfiguration
    // ════════════════════════════════════════════════════════════════════
    /// <summary>
    /// Applied in FRAContext via:
    ///   modelBuilder.ApplyConfiguration(new ModuleRoleConfiguration());
    ///
    /// Also add to FRAContext:
    ///   public DbSet&lt;ModuleRole&gt; ModuleRoles { get; set; } = null!;
    /// </summary>
    public class ModuleRoleConfiguration : IEntityTypeConfiguration<ModuleRole>
    {
        public void Configure(EntityTypeBuilder<ModuleRole> builder)
        {
            builder.ToTable("ModuleRoles");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.ModuleCode)
                .HasColumnType("nvarchar(20)")
                .IsRequired();

            builder.Property(r => r.RoleCode)
                .HasColumnType("nvarchar(30)")
                .IsRequired();

            builder.Property(r => r.RoleName)
                .HasColumnType("nvarchar(100)")
                .IsRequired();

            builder.Property(r => r.Description)
                .HasColumnType("nvarchar(250)")
                .IsRequired(false);

            builder.Property(r => r.SignOffLevel)
                .HasColumnType("nvarchar(20)")
                .IsRequired(false);

            builder.Property(r => r.CanWrite).HasDefaultValue(true);
            builder.Property(r => r.ShowBaseScope).HasDefaultValue(true);
            builder.Property(r => r.ShowGroupScope).HasDefaultValue(true);
            builder.Property(r => r.ShowWingScope).HasDefaultValue(false);
            builder.Property(r => r.IsActive).HasDefaultValue(true);
            builder.Property(r => r.SortOrder).HasDefaultValue((byte)99);

            // FK → Module
            builder.HasOne(r => r.Module)
                .WithMany(m => m.Roles)
                .HasForeignKey(r => r.ModuleCode)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ModuleRoles_Modules");

            builder.HasIndex(r => new { r.ModuleCode, r.RoleCode })
                .IsUnique()
                .HasDatabaseName("IX_ModuleRoles_Module_RoleCode");

            // ── Seed data — 17 roles across 4 operational modules ─────────
            builder.HasData(
                // ── MAINTENANCE roles ───────────────────────────────────
                new ModuleRole
                {
                    Id = 1,
                    ModuleCode = "MAINTENANCE",
                    RoleCode = "TECHNICIAN",
                    RoleName = "Technicien",
                    CanWrite = true,
                    SignOffLevel = "TECHNICIAN",
                    ShowBaseScope = true,
                    ShowGroupScope = true,
                    ShowWingScope = false,
                    SortOrder = 10
                },
                new ModuleRole
                {
                    Id = 2,
                    ModuleCode = "MAINTENANCE",
                    RoleCode = "APRS",
                    RoleName = "Inspecteur APRS",
                    CanWrite = true,
                    SignOffLevel = "APRS",
                    ShowBaseScope = true,
                    ShowGroupScope = true,
                    ShowWingScope = false,
                    SortOrder = 20
                },
                new ModuleRole
                {
                    Id = 3,
                    ModuleCode = "MAINTENANCE",
                    RoleCode = "NAVIGABILITY_OFFICER",
                    RoleName = "Officier de Navigabilité",
                    CanWrite = true,
                    SignOffLevel = "NAVIGABILITY",
                    ShowBaseScope = true,
                    ShowGroupScope = true,
                    ShowWingScope = false,
                    SortOrder = 30
                },
                new ModuleRole
                {
                    Id = 4,
                    ModuleCode = "MAINTENANCE",
                    RoleCode = "COMMANDER",
                    RoleName = "Commandant",
                    CanWrite = true,
                    SignOffLevel = "COMMANDER",
                    ShowBaseScope = true,
                    ShowGroupScope = false,
                    ShowWingScope = false,
                    SortOrder = 40
                },
                new ModuleRole
                {
                    Id = 5,
                    ModuleCode = "MAINTENANCE",
                    RoleCode = "BASE_SUPERVISOR",
                    RoleName = "Superviseur de Base",
                    CanWrite = false,
                    SignOffLevel = null,
                    ShowBaseScope = true,
                    ShowGroupScope = false,
                    ShowWingScope = false,
                    SortOrder = 50
                },
                new ModuleRole
                {
                    Id = 6,
                    ModuleCode = "MAINTENANCE",
                    RoleCode = "MASTER_SUPERVISOR",
                    RoleName = "Superviseur Central",
                    CanWrite = false,
                    SignOffLevel = null,
                    ShowBaseScope = false,
                    ShowGroupScope = false,
                    ShowWingScope = false,
                    SortOrder = 60
                },

                // ── HR roles ─────────────────────────────────────────────
                new ModuleRole
                {
                    Id = 7,
                    ModuleCode = "HR",
                    RoleCode = "HR_OFFICER",
                    RoleName = "Officier RH",
                    CanWrite = true,
                    SignOffLevel = null,
                    ShowBaseScope = true,
                    ShowGroupScope = false,
                    ShowWingScope = false,
                    SortOrder = 10
                },
                new ModuleRole
                {
                    Id = 8,
                    ModuleCode = "HR",
                    RoleCode = "HR_MANAGER",
                    RoleName = "Chef du personnel",
                    CanWrite = true,
                    SignOffLevel = null,
                    ShowBaseScope = false,
                    ShowGroupScope = false,
                    ShowWingScope = false,
                    SortOrder = 20
                },
                new ModuleRole
                {
                    Id = 9,
                    ModuleCode = "HR",
                    RoleCode = "HR_READONLY",
                    RoleName = "Consultation RH",
                    CanWrite = false,
                    SignOffLevel = null,
                    ShowBaseScope = true,
                    ShowGroupScope = false,
                    ShowWingScope = false,
                    SortOrder = 30
                },

                // ── HEALTHCARE roles ──────────────────────────────────────
                new ModuleRole
                {
                    Id = 10,
                    ModuleCode = "HEALTHCARE",
                    RoleCode = "DOCTOR",
                    RoleName = "Médecin",
                    CanWrite = true,
                    SignOffLevel = null,
                    ShowBaseScope = true,
                    ShowGroupScope = false,
                    ShowWingScope = false,
                    SortOrder = 10
                },
                new ModuleRole
                {
                    Id = 11,
                    ModuleCode = "HEALTHCARE",
                    RoleCode = "NURSE",
                    RoleName = "Infirmier",
                    CanWrite = true,
                    SignOffLevel = null,
                    ShowBaseScope = true,
                    ShowGroupScope = false,
                    ShowWingScope = false,
                    SortOrder = 20
                },
                new ModuleRole
                {
                    Id = 12,
                    ModuleCode = "HEALTHCARE",
                    RoleCode = "MEDICAL_ADMIN",
                    RoleName = "Admin médical",
                    CanWrite = false,
                    SignOffLevel = null,
                    ShowBaseScope = true,
                    ShowGroupScope = false,
                    ShowWingScope = false,
                    SortOrder = 30
                },

                // ── SQUADRONOPS roles ─────────────────────────────────────
                new ModuleRole
                {
                    Id = 13,
                    ModuleCode = "SQUADRONOPS",
                    RoleCode = "PILOT",
                    RoleName = "Pilote",
                    CanWrite = true,
                    SignOffLevel = null,
                    ShowBaseScope = true,
                    ShowGroupScope = true,
                    ShowWingScope = true,
                    SortOrder = 10
                },
                new ModuleRole
                {
                    Id = 14,
                    ModuleCode = "SQUADRONOPS",
                    RoleCode = "INSTRUCTOR",
                    RoleName = "Instructeur de vol",
                    CanWrite = true,
                    SignOffLevel = null,
                    ShowBaseScope = true,
                    ShowGroupScope = true,
                    ShowWingScope = true,
                    SortOrder = 20
                },
                new ModuleRole
                {
                    Id = 15,
                    ModuleCode = "SQUADRONOPS",
                    RoleCode = "OPS_SCHEDULER",
                    RoleName = "Planificateur OPS",
                    CanWrite = true,
                    SignOffLevel = null,
                    ShowBaseScope = true,
                    ShowGroupScope = true,
                    ShowWingScope = true,
                    SortOrder = 30
                },
                new ModuleRole
                {
                    Id = 16,
                    ModuleCode = "SQUADRONOPS",
                    RoleCode = "OPS_OFFICER",
                    RoleName = "Officier OPS",
                    CanWrite = true,
                    SignOffLevel = null,
                    ShowBaseScope = true,
                    ShowGroupScope = false,
                    ShowWingScope = false,
                    SortOrder = 40
                },
                new ModuleRole
                {
                    Id = 17,
                    ModuleCode = "SQUADRONOPS",
                    RoleCode = "OPS_COMMANDER",
                    RoleName = "Commandant OPS",
                    CanWrite = false,
                    SignOffLevel = null,
                    ShowBaseScope = true,
                    ShowGroupScope = false,
                    ShowWingScope = false,
                    SortOrder = 50
                }
            );
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  UserAssignmentConfiguration — DELIBERATELY OMITTED FROM THIS FILE
    // ════════════════════════════════════════════════════════════════════
    //  Held back per USER_MANAGEMENT_MERGE_PLAN.md §6 (scoping migration,
    //  not yet planned) AND because it will not currently compile:
    //
    //    builder.HasOne<FRAProject.Areas.SquadronOps.Models.Wing>()
    //
    //  references Wing in the SquadronOps namespace, but the live Wing.cs
    //  is in Areas/HR/Models. Confirmed via FRAContext.cs using-statements
    //  and the solution tree. Fix the namespace mismatch (or update this
    //  configuration to reference Areas.HR.Models.Wing) before this
    //  configuration is usable — and only once ApplicationUser.UserAssignments
    //  and Wing.UserAssignments navigation collections actually exist.
}