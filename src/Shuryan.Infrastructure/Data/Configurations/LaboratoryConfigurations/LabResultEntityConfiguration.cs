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

			// Table Mapping
			builder.ToTable("LabResults");

			// Properties
			builder.Property(lr => lr.LabOrderId).IsRequired();
			builder.Property(lr => lr.LabTestId).IsRequired();
			builder.Property(lr => lr.ResultValue).IsRequired().HasMaxLength(500);
			builder.Property(lr => lr.ReferenceRange).IsRequired(false).HasMaxLength(200);
			builder.Property(lr => lr.Unit).IsRequired(false).HasMaxLength(50);
			builder.Property(lr => lr.Notes).IsRequired(false).HasMaxLength(1000);
			builder.Property(lr => lr.AttachmentUrl).IsRequired(false).HasMaxLength(500);

			// Indexes
			builder.HasIndex(lr => lr.LabOrderId)
				.HasDatabaseName("IX_LabResult_LabOrderId");

			builder.HasIndex(lr => lr.LabTestId)
				.HasDatabaseName("IX_LabResult_LabTestId");

			builder.HasIndex(lr => new { lr.LabOrderId, lr.LabTestId })
				.IsUnique()
				.HasDatabaseName("IX_LabResult_Order_Test");

			// LabOrder Relationship (Many-to-One)
			builder.HasOne(lr => lr.LabOrder)
				.WithMany(lo => lo.LabResults)
				.HasForeignKey(lr => lr.LabOrderId)
				.OnDelete(DeleteBehavior.Cascade);

			// LabTest Relationship (Many-to-One)
			builder.HasOne(lr => lr.LabTest)
				.WithMany(lt => lt.LabResults)
				.HasForeignKey(lr => lr.LabTestId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
