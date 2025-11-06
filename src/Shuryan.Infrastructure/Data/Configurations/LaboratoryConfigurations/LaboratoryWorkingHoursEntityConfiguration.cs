using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.LaboratoryConfigurations
{
    public class LaboratoryWorkingHoursEntityConfiguration : AuditableEntityConfiguration<LabWorkingHours>
    {
        public override void Configure(EntityTypeBuilder<LabWorkingHours> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("LabWorkingHours");

            // Properties
            builder.Property(wh => wh.LaboratoryId).IsRequired();
            builder.Property(wh => wh.Day).IsRequired().HasConversion<int>();
            builder.Property(wh => wh.StartTime).IsRequired();
            builder.Property(wh => wh.EndTime).IsRequired();
            builder.Property(wh => wh.IsActive).IsRequired().HasDefaultValue(true);

            // Indexes
            builder.HasIndex(wh => wh.LaboratoryId)
                .HasDatabaseName("IX_LabWorkingHours_LaboratoryId");

            builder.HasIndex(wh => new { wh.LaboratoryId, wh.Day })
                .HasDatabaseName("IX_LabWorkingHours_Laboratory_Day");

            builder.HasIndex(wh => new { wh.LaboratoryId, wh.Day, wh.StartTime, wh.EndTime })
                .IsUnique()
                .HasDatabaseName("IX_LabWorkingHours_Unique");

            // Laboratory Relationship (Many-to-One)
            builder.HasOne(wh => wh.Laboratory)
                .WithMany(l => l.WorkingHours)
                .HasForeignKey(wh => wh.LaboratoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Check Constraint - Ensure StartTime is before EndTime
            builder.HasCheckConstraint("CK_LaboratoryWorkingHours_Time", "[StartTime] < [EndTime]");
        }
    }
}
