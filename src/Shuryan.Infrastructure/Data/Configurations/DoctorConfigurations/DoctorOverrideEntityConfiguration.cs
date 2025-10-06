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

            builder.HasKey(do_override => do_override.Id);

			builder.Property(do_override => do_override.StartTime)
				   .IsRequired();

			builder.Property(do_override => do_override.EndTime)
				   .IsRequired();

			builder.Property(do_override => do_override.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.HasOne(do_override => do_override.Doctor)
                   .WithMany(d => d.Overrides)
                   .HasForeignKey(do_override => do_override.DoctorId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Constraints
            builder.HasCheckConstraint("CK_DoctorOverride_TimeValidation", "[StartTime] < [EndTime]");
        }
    }
}
