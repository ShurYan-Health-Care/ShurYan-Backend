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

            // Table Mapping
            builder.ToTable("DoctorAvailabilities");

            // Properties
            builder.Property(da => da.DoctorId).IsRequired();
            builder.Property(da => da.DayOfWeek).IsRequired().HasConversion<int>();
            builder.Property(da => da.StartTime).IsRequired();
            builder.Property(da => da.EndTime).IsRequired();

            // Indexes
            builder.HasIndex(da => da.DoctorId)
                .HasDatabaseName("IX_DoctorAvailability_DoctorId");

            builder.HasIndex(da => new { da.DoctorId, da.DayOfWeek })
                .HasDatabaseName("IX_DoctorAvailability_Doctor_Day");

            builder.HasIndex(da => new { da.DoctorId, da.DayOfWeek, da.StartTime, da.EndTime })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_DoctorAvailability_Unique");

            // Doctor Relationship (Many-to-One)
            builder.HasOne(da => da.Doctor)
                .WithMany(d => d.Availabilities)
                .HasForeignKey(da => da.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Check Constraint - Ensure StartTime is before EndTime
            builder.HasCheckConstraint("CK_DoctorAvailability_TimeValidation", "[StartTime] < [EndTime]");
        }
    }
}
