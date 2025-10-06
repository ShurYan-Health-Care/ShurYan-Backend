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

            builder.HasKey(pwh => pwh.Id);

            builder.Property(pwh => pwh.DayOfWeek)
                .HasConversion<int>()
                .HasMaxLength(20);

            builder.HasOne(pwh => pwh.Pharmacy)
                .WithMany(p => p.WorkingHours)
                .HasForeignKey(pwh => pwh.PharmacyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
