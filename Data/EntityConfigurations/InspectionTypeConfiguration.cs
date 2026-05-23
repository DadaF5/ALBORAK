using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class InspectionTypeConfiguration : IEntityTypeConfiguration<InspectionType>
    {
        public void Configure(EntityTypeBuilder<InspectionType> builder)
        {
            builder.ToTable("InspectionTypes", schema: "dbo");

            builder.HasKey(x => x.Id);

            // LookupBase properties
            builder.Property(x => x.Code)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(250)
                .IsRequired(false);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue((byte)99);

            // Unique: Code must be unique per AcType
            builder.HasIndex(x => new { x.AcTypeId, x.Code })
                .IsUnique()
                .HasDatabaseName("UX_InspectionType_AcType_Code");

            // FK: AcType (Restrict so deleting AcType requires removing inspection types first)
            builder.HasOne(x => x.AcType)
                .WithMany()
                .HasForeignKey(x => x.AcTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Self-reference: NextInspectionType (nullable, Restrict to prevent cascade cycles)
            builder.HasOne(x => x.NextInspectionType)
                .WithMany(x => x.PrecedingInspectionTypes)
                .HasForeignKey(x => x.NextInspectionTypeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
