using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical.Schedules;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.DoctorConfigurations
{
    public class DoctorAvailabilityEntityConfiguration : SoftDeletableEntityConfiguration<DoctorAvailability>
    {
        public override void Configure(EntityTypeBuilder<DoctorAvailability> builder)
        {
            base.Configure(builder);

            builder.HasKey(da => da.Id);

            builder.Property(da => da.DayOfWeek)
                   .HasConversion<int>()
                   .IsRequired();

			builder.Property(da => da.StartTime)
				   .IsRequired();

			builder.Property(da => da.EndTime)
				   .IsRequired();

			// Relationships
			builder.HasOne(da => da.Doctor)
                   .WithMany(d => d.Availabilities)
                   .HasForeignKey(da => da.DoctorId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Constraints
            builder.HasCheckConstraint("CK_DoctorAvailability_TimeValidation", "[StartTime] < [EndTime]");
        }
    }
}
