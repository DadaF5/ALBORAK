
using FRAProject.Areas.SquadronOps.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;


namespace FRAProject.Data.EntityConfigurations
{
    public class SortieConfiguration : IEntityTypeConfiguration<Sortie>
    {
        public void Configure(EntityTypeBuilder<Sortie> builder)
        {
            builder.ToTable("Sorties");

            builder.HasKey(s => s.Id); 
            
            // Relationship to Odv (principal)
            builder.HasOne(s => s.Odv)
                 .WithMany(o => o.Sorties)
                 .HasForeignKey(s => s.OdvId)
                 .OnDelete(DeleteBehavior.Cascade);

            // Relationship to AcType (required)
            builder.HasOne(s => s.AcType)
                .WithMany(t => t.Sorties)
                .HasForeignKey(s => s.AcTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship to Aircraft (optional)
            builder.HasOne(s => s.Aircraft)
                .WithMany(a => a.Sorties)
                .HasForeignKey(s => s.AircraftId)
                .OnDelete(DeleteBehavior.SetNull);

            // Date/time related fields
            builder.Property(s => s.StartTime)
                .HasColumnType("datetime2")
                .IsRequired(false);

            builder.Property(s => s.LandingTime)
                .HasColumnType("datetime2")
                .IsRequired(false);

            builder.Property(s => s.TOFF)
                .HasColumnType("time")
                .IsRequired(false);

            // Fuel quantity with precision
            builder.Property(s => s.FuelQuantity)
                .HasColumnType("decimal(12,2)")
                .IsRequired(false);
            // Fuel used
            builder.Property(s => s.FuelUsedLiters)
                .HasColumnType("decimal(12,2)")
                .IsRequired(false);

            // Text fields
            builder.Property(s => s.Configuration)
                .HasMaxLength(200)
                .IsRequired(false);

            builder.Property(s => s.Notes)
                .HasMaxLength(2000)
                .IsRequired(false);

            // Audit fields
            builder.Property(s => s.CreatedAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(s => s.UpdatedAtUtc)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // Enum -> string conversion for readability
            builder.Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            // Concurrency token
            builder.Property(s => s.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken()
                .HasColumnName("RowVersion");

            builder.Property(s => s.CompletedAtUtc).HasColumnType("datetime2");
            builder.Property(s => s.CreatedBy).HasMaxLength(200);
            builder.Property(s => s.UpdatedBy).HasMaxLength(200);
            builder.Property(s => s.CompletedBy).HasMaxLength(200);

            // Indexes for common queries
            builder.HasIndex(s => s.OdvId).HasDatabaseName("IX_Sortie_OdvId");
            builder.HasIndex(s => s.BaseId).HasDatabaseName("IX_Sortie_BaseId");
            builder.HasIndex(s => s.AircraftId).HasDatabaseName("IX_Sortie_AircraftId");
            builder.HasIndex(s => new { s.OdvId, s.IsCompleted }).HasDatabaseName("IX_Sorties_Odv_Completed");
            // RowVersion concurrency token
            builder.Property(s => s.RowVersion).IsRowVersion().IsConcurrencyToken();

            // Useful indexes
            builder.HasIndex(s => s.OdvId);
            builder.HasIndex(s => s.IsCompleted);

            // If SortieCrew exists in your model, the following relationship is expected:
            // builder.HasMany(s => s.SortieCrews)
            //     .WithOne(sc => sc.Sortie!)
            //     .HasForeignKey(sc => sc.SortieId)
            //     .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
