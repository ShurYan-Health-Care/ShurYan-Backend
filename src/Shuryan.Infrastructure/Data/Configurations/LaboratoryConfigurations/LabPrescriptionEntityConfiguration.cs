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
    public class LabPrescriptionEntityConfiguration : AuditableEntityConfiguration<LabPrescription>
	{
		public override void Configure(EntityTypeBuilder<LabPrescription> builder)
		{
			base.Configure(builder);

			// Table Mapping
			builder.ToTable("LabPrescriptions");

			// Properties
			builder.Property(lp => lp.AppointmentId).IsRequired();
			builder.Property(lp => lp.DoctorId).IsRequired();
			builder.Property(lp => lp.PatientId).IsRequired();
			builder.Property(lp => lp.GeneralNotes).IsRequired(false).HasMaxLength(1000);

			// Indexes
			builder.HasIndex(lp => lp.AppointmentId)
				.IsUnique()
				.HasDatabaseName("IX_LabPrescription_AppointmentId");

			builder.HasIndex(lp => lp.DoctorId)
				.HasDatabaseName("IX_LabPrescription_DoctorId");

			builder.HasIndex(lp => lp.PatientId)
				.HasDatabaseName("IX_LabPrescription_PatientId");

			// Appointment Relationship (One-to-One)
			builder.HasOne(lp => lp.Appointment)
				.WithMany(a => a.LabPrescription)
				.HasForeignKey(lp => lp.AppointmentId)
				.OnDelete(DeleteBehavior.Restrict);

			// Doctor Relationship (Many-to-One)
			builder.HasOne(lp => lp.Doctor)
				.WithMany(d => d.LabPrescriptions)
				.HasForeignKey(lp => lp.DoctorId)
				.OnDelete(DeleteBehavior.Restrict);

			// Patient Relationship (Many-to-One)
			builder.HasOne(lp => lp.Patient)
				.WithMany()
				.HasForeignKey(lp => lp.PatientId)
				.OnDelete(DeleteBehavior.Restrict);

			// Prescription Items Relationship (One-to-Many)
			builder.HasMany(lp => lp.Items)
				.WithOne(lpi => lpi.LabPrescription)
				.HasForeignKey(lpi => lpi.LabPrescriptionId)
				.OnDelete(DeleteBehavior.Cascade);

			// Lab Order Relationship (One-to-One, Optional)
			builder.HasOne(lp => lp.LabOrder)
				.WithOne(lo => lo.LabPrescription)
				.HasForeignKey<LabOrder>(lo => lo.LabPrescriptionId)
				.IsRequired(false)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}