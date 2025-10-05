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
    public class LabPrescriptionEntityConfiguration : IEntityTypeConfiguration<LabPrescription>
	{
		public void Configure(EntityTypeBuilder<LabPrescription> builder)
		{
			builder.HasKey(lp => lp.Id);

			builder.Property(lp => lp.GeneralNotes)
					.HasMaxLength(1000);

			builder.Property(lp => lp.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

			// Relationships
			builder.HasOne(lp => lp.Appointment)
					.WithMany(a => a.LabPrescription)
					.HasForeignKey(lp => lp.AppointmentId)
					.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(lp => lp.Doctor)
					.WithMany(d => d.LabPrescriptions)
					.HasForeignKey(lp => lp.DoctorId)
					.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(lp => lp.Patient)
					.WithMany()
					.HasForeignKey(lp => lp.PatientId)
					.OnDelete(DeleteBehavior.Restrict);

			builder.HasMany(lp => lp.Items)
					.WithOne(lpi => lpi.LabPrescription)
					.HasForeignKey(lpi => lpi.LabPrescriptionId)
					.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(lp => lp.LabOrder)
					.WithOne(lo => lo.LabPrescription)
					.HasForeignKey<LabOrder>(lo => lo.LabPrescriptionId)
					.IsRequired(false)
					.OnDelete(DeleteBehavior.Cascade);
		}
	}
}