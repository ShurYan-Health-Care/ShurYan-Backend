using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Laboratories;

namespace Shuryan.Infrastructure.Data.Configurations.LaboratoryConfigurations
{
    public class LabPrescriptionItemEntityConfiguration : IEntityTypeConfiguration<LabPrescriptionItem>
	{
		public void Configure(EntityTypeBuilder<LabPrescriptionItem> builder)
		{
			builder.HasKey(lpi => lpi.Id);

			builder.Property(lpi => lpi.DoctorNotes)
				   .HasMaxLength(500);

			builder.Property(lpi => lpi.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

			// Relationships
			builder.HasOne(lpi => lpi.LabPrescription)
				   .WithMany(lp => lp.Items)
				   .HasForeignKey(lpi => lpi.LabPrescriptionId)
				   .OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(lpi => lpi.LabTest)
				   .WithMany(lt => lt.PrescriptionItems)
				   .HasForeignKey(lpi => lpi.LabTestId)
				   .OnDelete(DeleteBehavior.Restrict);
		}
	}
}