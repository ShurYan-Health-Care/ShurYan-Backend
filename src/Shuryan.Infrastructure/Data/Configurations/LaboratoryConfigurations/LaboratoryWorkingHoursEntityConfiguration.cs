using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical;

namespace Shuryan.Infrastructure.Data.Configurations.LaboratoryConfigurations
{
    public class LaboratoryWorkingHoursEntityConfiguration : IEntityTypeConfiguration<LaboratoryWorkingHours>
    {
        public void Configure(EntityTypeBuilder<LaboratoryWorkingHours> builder)
        {
            builder.HasKey(wh => wh.Id);

            builder.Property(wh => wh.Day)
                   .HasConversion<int>()
                   .IsRequired();

			builder.Property(wh => wh.StartTime)
				   .IsRequired();

			builder.Property(wh => wh.EndTime)
				   .IsRequired();

			builder.Property(wh => wh.IsActive)
                   .IsRequired()
                   .HasDefaultValue(true);

            builder.HasOne(wh => wh.Laboratory)
                   .WithMany(l => l.WorkingHours)
                   .HasForeignKey(wh => wh.LaboratoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasCheckConstraint("CK_LaboratoryWorkingHours_Time", "[StartTime] < [EndTime]");
        }
    }
}
