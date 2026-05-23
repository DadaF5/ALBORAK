using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FRAProject.Data.EntityConfigurations
{
    public class JobCardPlanningRuleConfiguration : IEntityTypeConfiguration<JobCardPlanningRule>
    {
        public void Configure(EntityTypeBuilder<JobCardPlanningRule> builder)
        {
            builder.ToTable("JobCardPlanningRules");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RuleName)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.ConditionText)
                .HasMaxLength(500);

            builder.Property(x => x.InitialCalendarUnit)
                .HasMaxLength(10);

            builder.Property(x => x.RecurringCalendarUnit)
                .HasMaxLength(10);

            builder.Property(x => x.RequiredComplianceCode)
                .HasMaxLength(100);

            builder.Property(x => x.ForbiddenComplianceCode)
                .HasMaxLength(100);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue(100);

            builder.Property(x => x.IsApplicable)
                .HasDefaultValue(true);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(x => new { x.JobCardId, x.SortOrder });

            builder.HasOne(x => x.JobCard)
                .WithMany(x => x.PlanningRules)
                .HasForeignKey(x => x.JobCardId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}