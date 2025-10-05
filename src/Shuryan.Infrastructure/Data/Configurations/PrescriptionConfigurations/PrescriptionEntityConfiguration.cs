using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External.Pharmacies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Data.Configurations.PrescriptionConfigurations
{
    public class PrescriptionEntityConfiguration : IEntityTypeConfiguration<Prescription>
    {
        public void Configure(EntityTypeBuilder<Prescription> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.PrescriptionNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.DigitalSignature)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.GeneralInstructions)
                .HasMaxLength(1000);

            builder.Property(p => p.FollowUpInstructions)
                .HasMaxLength(1000);


            builder.Property(p => p.IssuedDate).IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

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
