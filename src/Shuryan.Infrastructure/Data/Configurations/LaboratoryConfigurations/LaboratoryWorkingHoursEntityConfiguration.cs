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
