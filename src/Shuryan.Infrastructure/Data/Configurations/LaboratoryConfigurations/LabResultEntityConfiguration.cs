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
    public class LabResultEntityConfiguration : AuditableEntityConfiguration<LabResult>
	{
		public override void Configure(EntityTypeBuilder<LabResult> builder)
		{
			base.Configure(builder);

			builder.HasKey(lr => lr.Id);

			builder.Property(lr => lr.ResultValue)
				   .IsRequired()
				   .HasMaxLength(500);

			builder.Property(lr => lr.ReferenceRange)
				   .HasMaxLength(200);

			builder.Property(lr => lr.Unit)
				   .HasMaxLength(50);

			builder.Property(lr => lr.Notes)
				   .HasMaxLength(1000);

			builder.Property(lr => lr.AttachmentUrl)
				   .HasMaxLength(500);


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
