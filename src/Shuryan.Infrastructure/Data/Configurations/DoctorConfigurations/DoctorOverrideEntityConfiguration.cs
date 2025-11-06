using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Enums;
using Shuryan.Core.Entities.Medical.Schedules;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.DoctorConfigurations
{
    public class DoctorOverrideEntityConfiguration : AuditableEntityConfiguration<DoctorOverride>
    {
        public override void Configure(EntityTypeBuilder<DoctorOverride> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("DoctorOverrides");

            // Properties
            builder.Property(do_override => do_override.DoctorId).IsRequired();
            builder.Property(do_override => do_override.StartTime).IsRequired();
            builder.Property(do_override => do_override.EndTime).IsRequired();
            builder.Property(do_override => do_override.Type).IsRequired().HasConversion<int>();

            // Indexes
            builder.HasIndex(do_override => do_override.DoctorId)
                .HasDatabaseName("IX_DoctorOverride_DoctorId");

            builder.HasIndex(do_override => do_override.StartTime)
                .HasDatabaseName("IX_DoctorOverride_StartTime");

            builder.HasIndex(do_override => new { do_override.DoctorId, do_override.Type })
                .HasDatabaseName("IX_DoctorOverride_Doctor_Type");

            // Doctor Relationship (Many-to-One)
            builder.HasOne(do_override => do_override.Doctor)
                .WithMany(d => d.Overrides)
                .HasForeignKey(do_override => do_override.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Check Constraint - Ensure StartTime is before EndTime
            builder.HasCheckConstraint("CK_DoctorOverride_TimeValidation", "[StartTime] < [EndTime]");
        }
    }
}
