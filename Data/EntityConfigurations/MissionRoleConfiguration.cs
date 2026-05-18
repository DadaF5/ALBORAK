using FRAProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.Configurations
{
    /// <summary>
    /// Fluent API configuration for MissionRole.
    /// Applied in FRAContext via:
    ///   builder.ApplyConfiguration(new MissionRoleConfiguration());
    ///
    /// FK to AcCategory is optional (nullable).
    /// DeleteBehavior.SetNull — deactivating a category sets
    /// MissionRole.AcCategoryId to NULL rather than cascading.
    ///
    /// Seed data: 11 rows matching Form 5a Step 2 DDL exactly.
    /// AcCategoryId references use the Id values seeded in
    /// AcCategoryConfiguration (AVION=1, HELI=2, UAS=3).
    /// NEVER change those Id values.
    /// </summary>
    public class MissionRoleConfiguration : IEntityTypeConfiguration<MissionRole>
    {
        public void Configure(EntityTypeBuilder<MissionRole> builder)
        {
            // ── Table ────────────────────────────────────────────────────
            builder.ToTable("MissionRole");

            // ── Primary key ──────────────────────────────────────────────
            builder.HasKey(m => m.Id);

            // ── Code ─────────────────────────────────────────────────────
            builder.Property(m => m.Code)
                .HasColumnType("nvarchar(10)")
                .IsRequired()
                .HasMaxLength(10);

            builder.HasIndex(m => m.Code)
                .IsUnique()
                .HasDatabaseName("UX_MissionRole_Code");

            // ── Name ─────────────────────────────────────────────────────
            builder.Property(m => m.Name)
                .HasColumnType("nvarchar(100)")
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(m => m.Name)
                .IsUnique()
                .HasDatabaseName("UX_MissionRole_Name");

            // ── AcCategoryId (optional FK) ────────────────────────────────
            // SetNull: if AcCategory is soft-deleted or hard-deleted,
            // MissionRole.AcCategoryId becomes NULL.
            // Restrict would prevent deleting a category that has roles.
            builder.HasOne(m => m.AcCategory)
                .WithMany()
                .HasForeignKey(m => m.AcCategoryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_MissionRole_AcCategory");

            // ── SortOrder ────────────────────────────────────────────────
            builder.Property(m => m.SortOrder)
                .HasDefaultValue(0);

            // ── IsActive ─────────────────────────────────────────────────
            builder.Property(m => m.IsActive)
                .HasDefaultValue(true);

            // ── Seed data ────────────────────────────────────────────────
            // AcCategoryId values:
            //   1 = AVION  (from AcCategoryConfiguration)
            //   2 = HELI
            //   3 = UAS
            //   null = applies to multiple categories
            builder.HasData(
                // ── Avion roles ──────────────────────────────────────────
                new MissionRole { Id =  1, Code = "CHASSE",
                    Name = "Chasse / Interception",
                    AcCategoryId = 1, SortOrder =  1, IsActive = true },
                new MissionRole { Id =  2, Code = "APPUI",
                    Name = "Appui sol",
                    AcCategoryId = 1, SortOrder =  2, IsActive = true },
                new MissionRole { Id =  3, Code = "RAVITO",
                    Name = "Ravitaillement en vol",
                    AcCategoryId = 1, SortOrder =  3, IsActive = true },
                new MissionRole { Id =  4, Code = "FORMATION",
                    Name = "Entrainement / Formation",
                    AcCategoryId = 1, SortOrder =  4, IsActive = true },
                new MissionRole { Id =  5, Code = "MARITIME",
                    Name = "Maritime / Patrouille",
                    AcCategoryId = 1, SortOrder =  5, IsActive = true },

                // ── Helicopter roles ─────────────────────────────────────
                new MissionRole { Id =  6, Code = "ASSAULT",
                    Name = "Helicoptere d'assaut",
                    AcCategoryId = 2, SortOrder =  6, IsActive = true },

                // ── Cross-category roles (null = AVION + HELI) ───────────
                new MissionRole { Id =  7, Code = "TRANSPORT",
                    Name = "Transport tactique",
                    AcCategoryId = null, SortOrder =  7, IsActive = true },
                new MissionRole { Id =  8, Code = "SAR",
                    Name = "SAR / CSAR",
                    AcCategoryId = null, SortOrder =  8, IsActive = true },
                new MissionRole { Id =  9, Code = "ISR",
                    Name = "Reconnaissance / ISR",
                    AcCategoryId = null, SortOrder =  9, IsActive = true },

                // ── UAS roles ────────────────────────────────────────────
                new MissionRole { Id = 10, Code = "UAV-ISR",
                    Name = "Drone ISR",
                    AcCategoryId = 3, SortOrder = 10, IsActive = true },
                new MissionRole { Id = 11, Code = "UAV-ARM",
                    Name = "Drone arme",
                    AcCategoryId = 3, SortOrder = 11, IsActive = true }
            );
        }
    }
}
