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
    public class LabPrescriptionItemEntityConfiguration : AuditableEntityConfiguration<LabPrescriptionItem>
	{
		public override void Configure(EntityTypeBuilder<LabPrescriptionItem> builder)
		{
			base.Configure(builder);

			// Table Mapping
			builder.ToTable("LabPrescriptionItems");

			// Properties
			builder.Property(lpi => lpi.LabPrescriptionId).IsRequired();
			builder.Property(lpi => lpi.LabTestId).IsRequired();
			builder.Property(lpi => lpi.DoctorNotes).IsRequired(false).HasMaxLength(500);

			// Indexes
			builder.HasIndex(lpi => lpi.LabPrescriptionId)
				.HasDatabaseName("IX_LabPrescriptionItem_LabPrescriptionId");

			builder.HasIndex(lpi => lpi.LabTestId)
				.HasDatabaseName("IX_LabPrescriptionItem_LabTestId");

			builder.HasIndex(lpi => new { lpi.LabPrescriptionId, lpi.LabTestId })
				.IsUnique()
				.HasDatabaseName("IX_LabPrescriptionItem_Prescription_Test");

			// LabPrescription Relationship (Many-to-One)
			builder.HasOne(lpi => lpi.LabPrescription)
				.WithMany(lp => lp.Items)
				.HasForeignKey(lpi => lpi.LabPrescriptionId)
				.OnDelete(DeleteBehavior.Cascade);

			// LabTest Relationship (Many-to-One)
			builder.HasOne(lpi => lpi.LabTest)
				.WithMany(lt => lt.PrescriptionItems)
				.HasForeignKey(lpi => lpi.LabTestId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}