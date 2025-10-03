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
	public class LabResultEntityConfiguration : IEntityTypeConfiguration<LabResult>
	{
		public void Configure(EntityTypeBuilder<LabResult> builder)
		{
			builder.HasKey(lr => lr.Id);

			builder.Property(lr => lr.ResultFileUrl)
				   .HasMaxLength(500);

			builder.Property(lr => lr.LabNotes)
				   .HasMaxLength(1000);

			builder.Property(lr => lr.RecordedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

			// Relationships
			builder.HasOne(lr => lr.LabOrder)
				   .WithMany(lo => lo.LabResults)
				   .HasForeignKey(lr => lr.LabOrderId)
				   .OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(lr => lr.LabTest)
				   .WithMany()
				   .HasForeignKey(lr => lr.LabTestId)
				   .OnDelete(DeleteBehavior.Restrict);
		}
	}
}
