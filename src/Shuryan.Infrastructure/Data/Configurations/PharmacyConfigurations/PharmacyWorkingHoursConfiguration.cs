using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Data.Configurations.PharmacyConfigurations
{
    public class PharmacyWorkingHoursConfiguration : IEntityTypeConfiguration<PharmacyWorkingHours>
    {
        public void Configure(EntityTypeBuilder<PharmacyWorkingHours> builder)
        {
            builder.HasKey(pwh => pwh.Id);

            builder.Property(pwh => pwh.DayOfWeek)
                .HasConversion<string>()
                .HasMaxLength(20);

            // تعريف العلاقة مع الصيدلية
            builder.HasOne(pwh => pwh.Pharmacy)
                .WithMany(p => p.WorkingHours)
                .HasForeignKey(pwh => pwh.PharmacyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
