using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Data.Configurations.PrescriptionConfigurations
{
    public class PrescriptionEntityConfiguration : AuditableEntityConfiguration<Prescription>
    {
        public override void Configure(EntityTypeBuilder<Prescription> builder)
        {
			base.Configure(builder);

            // Table Mapping
            builder.ToTable("Prescriptions");

            // Properties
            builder.Property(p => p.PrescriptionNumber).IsRequired().HasMaxLength(50);
            builder.Property(p => p.DigitalSignature).IsRequired().HasMaxLength(200);
            builder.Property(p => p.GeneralInstructions).IsRequired(false).HasMaxLength(1000);
            builder.Property(p => p.Status).IsRequired().HasConversion<int>();
            builder.Property(p => p.DispensedAt).IsRequired(false);
            builder.Property(p => p.CancellationReason).IsRequired(false).HasMaxLength(500);
            builder.Property(p => p.CancelledAt).IsRequired(false);
            builder.Property(p => p.DoctorId).IsRequired();
            builder.Property(p => p.PatientId).IsRequired();
            builder.Property(p => p.AppointmentId).IsRequired(false);

            // Indexes
            builder.HasIndex(p => p.PrescriptionNumber)
                .IsUnique().HasDatabaseName("IX_Prescription_Number");

            builder.HasIndex(p => p.DoctorId)
                .HasDatabaseName("IX_Prescription_DoctorId");

            builder.HasIndex(p => p.PatientId)
                .HasDatabaseName("IX_Prescription_PatientId");

            builder.HasIndex(p => p.Status)
                .HasDatabaseName("IX_Prescription_Status");

            builder.HasIndex(p => new { p.PatientId, p.Status })
                .HasDatabaseName("IX_Prescription_Patient_Status");


			builder.HasOne(p => p.Doctor)
                    .WithMany(d => d.Prescriptions) 
                    .HasForeignKey(p => p.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(p => p.Patient)
                    .WithMany(pat => pat.Prescriptions)
                    .HasForeignKey(p => p.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(p => p.Appointment)
                    .WithOne(a => a.Prescription)
                    .HasForeignKey<Prescription>(p => p.AppointmentId)
                    .OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(p => p.PharmacyOrder)
                    .WithOne(po => po.Prescription)
                    .HasForeignKey<PharmacyOrder>(po => po.PrescriptionId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

			builder.HasMany(p => p.PrescribedMedications)
                    .WithOne(pm => pm.MedicationPrescription)
                    .HasForeignKey(pm => pm.MedicationPrescriptionId)
                    .OnDelete(DeleteBehavior.Cascade);
		}
    }
}
