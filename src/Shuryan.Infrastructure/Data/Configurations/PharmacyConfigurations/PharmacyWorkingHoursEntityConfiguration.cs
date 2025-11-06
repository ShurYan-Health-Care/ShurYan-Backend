using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Data.Configurations.PharmacyConfigurations
{
    public class PharmacyWorkingHoursEntityConfiguration : AuditableEntityConfiguration<PharmacyWorkingHours>
    {
        public override void Configure(EntityTypeBuilder<PharmacyWorkingHours> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("PharmacyWorkingHours");

            // Properties
            builder.Property(pwh => pwh.PharmacyId).IsRequired();
            builder.Property(pwh => pwh.DayOfWeek).IsRequired().HasConversion<int>();
            builder.Property(pwh => pwh.StartTime).IsRequired();
            builder.Property(pwh => pwh.EndTime).IsRequired();

            // Indexes
            builder.HasIndex(pwh => pwh.PharmacyId)
                .HasDatabaseName("IX_PharmacyWorkingHours_PharmacyId");

            builder.HasIndex(pwh => new { pwh.PharmacyId, pwh.DayOfWeek })
                .HasDatabaseName("IX_PharmacyWorkingHours_Pharmacy_Day");

            builder.HasIndex(pwh => new { pwh.PharmacyId, pwh.DayOfWeek, pwh.StartTime, pwh.EndTime })
                .IsUnique()
                .HasDatabaseName("IX_PharmacyWorkingHours_Unique");

            // Pharmacy Relationship (Many-to-One)
            builder.HasOne(pwh => pwh.Pharmacy)
                .WithMany(p => p.WorkingHours)
                .HasForeignKey(pwh => pwh.PharmacyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Check Constraint - Ensure StartTime is before EndTime
            builder.HasCheckConstraint("CK_PharmacyWorkingHours_Time", "[StartTime] < [EndTime]");
        }
    }
}
