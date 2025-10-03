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
    public class LabServiceEntityConfiguration : IEntityTypeConfiguration<LabService>
    {
        public void Configure(EntityTypeBuilder<LabService> builder)
        {
			builder.HasKey(ls => ls.Id);

			builder.Property(ls => ls.Price)
				   .IsRequired()
				   .HasPrecision(10, 2);

			builder.Property(ls => ls.IsAvailable)
				   .IsRequired()
				   .HasDefaultValue(true);

			builder.Property(ls => ls.LabSpecificNotes)
				   .HasMaxLength(500);

			// Relationships
			builder.HasOne(ls => ls.Laboratory)
				   .WithMany(l => l.LabServices)
				   .HasForeignKey(ls => ls.LaboratoryId)
				   .OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(ls => ls.LabTest)
				   .WithMany(lt => lt.LabServices)
				   .HasForeignKey(ls => ls.LabTestId)
				   .OnDelete(DeleteBehavior.Restrict);

			// Constraints
			builder.HasCheckConstraint("CK_LabService_Price", "[Price] >= 0");
		}
	}
}