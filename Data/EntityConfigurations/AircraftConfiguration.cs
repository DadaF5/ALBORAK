using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.Configurations
{
    public class AircraftConfiguration : IEntityTypeConfiguration<Aircraft>
    {
        public void Configure(EntityTypeBuilder<Aircraft> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Registration)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(a => a.Registration)
                .IsUnique()
                .HasDatabaseName("UX_Aircraft_Registration");

            builder.HasIndex(a => a.TailNo)
                .IsUnique()
                .HasDatabaseName("UX_Aircraft_TailNo");

            builder.Property(a => a.SerialNumber)
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(a => a.Manufacturer)
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(a => a.Model)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(a => a.IntCode)
                .HasColumnType("nvarchar(10)")
                .HasMaxLength(10)
                .IsRequired(false);

            builder.Property(a => a.Obs)
                .HasColumnType("nvarchar(500)")
                .IsRequired(false);

            builder.Property(a => a.TotalFlightMinutes).HasDefaultValue(0);
            builder.Property(a => a.TotalCycles).HasDefaultValue(0);
            builder.Property(a => a.TotalLandings).HasDefaultValue(0);
            builder.Property(a => a.SortOrder).HasDefaultValue((byte)99);

            builder.Property(a => a.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(a => a.CreatedByUserId)
                .HasColumnType("nvarchar(450)")
                .IsRequired(false);

            builder.Ignore(a => a.Description);
            builder.Ignore(a => a.TailNumber);
            builder.Ignore(a => a.DisplayName);
            builder.Ignore(a => a.FlightHoursDisplay);
            builder.Ignore(a => a.StatusBadgeClass);
            builder.Ignore(a => a.ShortLabel);

            // Required FK → AcType
            builder.HasOne(a => a.AcType)
                .WithMany(t => t.Aircrafts)
                .HasForeignKey(a => a.AcTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Aircraft_AcType");

            // Required FK → AcStatusType
            builder.HasOne(a => a.AcStatusType)
                .WithMany(s => s.Aircrafts)
                .HasForeignKey(a => a.AcStatusTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Aircraft_AcStatusType");

            // Optional FK → AircraftVersion
            builder.HasOne(a => a.AircraftVersion)
                .WithMany()
                .HasForeignKey(a => a.AircraftVersionId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Aircraft_AircraftVersion");

            // Optional FK → AircraftManufacturer
            builder.HasOne(a => a.AircraftManufacturerNav)
                .WithMany()
                .HasForeignKey(a => a.ManufacturerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Aircraft_ManufacturerFk");

            // Optional FK → Base
            builder.HasOne(a => a.Base)
                .WithMany(b => b.Aircrafts)
                .HasForeignKey(a => a.BaseId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Aircraft_Base");

            // Optional FK → MissionRole
            builder.HasOne(a => a.MissionRole)
                .WithMany()
                .HasForeignKey(a => a.MissionRoleId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Aircraft_MissionRole");

            // Optional FK → Country
            builder.HasOne(a => a.OriginCountry)
                .WithMany()
                .HasForeignKey(a => a.OriginCountryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Aircraft_OriginCountry");

            // Optional FK → Dossier
            builder.HasOne(a => a.Dossier)
                .WithMany()
                .HasForeignKey(a => a.DossierId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Aircraft_Dossier");
        }
    }
}