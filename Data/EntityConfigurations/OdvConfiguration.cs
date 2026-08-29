using FRAProject.Areas.SquadronOps.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class OdvConfiguration : IEntityTypeConfiguration<Odv>
    {
        public void Configure(EntityTypeBuilder<Odv> builder)
        {
            builder.ToTable("Odvs");

            builder.HasKey(o => o.Id);

            // Basic properties
            builder.Property(o => o.OdvDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(o => o.TOFF)
                .HasColumnType("time")
                .IsRequired(false);

            builder.Property(o => o.Area)
                .HasMaxLength(200)
                .IsRequired();

            // CallSign
            builder.HasOne(o => o.CallSign)
                 .WithMany()
                 .HasForeignKey(o => o.CallSignId)
                 .OnDelete(DeleteBehavior.Restrict);


            builder.Property(o => o.Obs)
                .HasMaxLength(2000)
                .IsRequired(false);

            builder.Property(o => o.CreatedAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(o => o.UpdatedAtUtc)
                .HasColumnType("datetime2")
                .IsRequired(false);

            // RowVersion / concurrency
            builder.Property(o => o.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken()
                .HasColumnName("RowVersion");

            // Enum -> string conversions (keeps readable DB values and is future-friendly)
            builder.Property(o => o.Zone)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(o => o.MissionType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            // Nullable enum
            builder.Property(o => o.OdvStatus)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired(false);

            // Relationships (FKs)
            // Squadron
            builder.HasIndex(o => new { o.SquadronId, o.OdvDate })
                .IsUnique()
                .HasDatabaseName("UX_Odv_Squadron_OdvDate");

            builder.HasOne(o => o.Squadron)
                .WithMany(s => s.Odvs) // explicit: Squadron must have public ICollection<Odv> Odvs
                .HasForeignKey(o => o.SquadronId)
                .OnDelete(DeleteBehavior.Restrict);

            // Base (optional)
            builder.HasOne(o => o.Base)
                .WithMany() // if Base does not expose Odvs collection, keep WithMany()
                .HasForeignKey(o => o.BaseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Mission
            builder.HasOne(o => o.Mission)
               .WithMany(m => m.Odvs) // explicit: Mission must have public ICollection<Odv> Odvs
               .HasForeignKey(o => o.MissionId)
               .OnDelete(DeleteBehavior.Restrict);

            // AcMainGroup
            builder.HasOne(o => o.AcMainGroup)
                .WithMany(g => g.Odvs)
                .HasForeignKey(o => o.AcMainGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sorties collection
            // Assumes Sortie has OdvId FK and Odv navigation property
            builder.HasMany(o => o.Sorties)
               .WithOne(s => s.Odv!)
               .HasForeignKey(s => s.OdvId)
               .OnDelete(DeleteBehavior.Cascade);

            // IsPreflightApproved default
            builder.Property(o => o.IsPreflightApproved)
                .HasDefaultValue(false)
                .IsRequired();
        }
    }
}
