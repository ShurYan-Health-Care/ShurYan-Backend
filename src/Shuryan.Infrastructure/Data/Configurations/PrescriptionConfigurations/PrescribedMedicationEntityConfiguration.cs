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
    public class PrescribedMedicationEntityConfiguration : IEntityTypeConfiguration<PrescribedMedication>
    {
        public void Configure(EntityTypeBuilder<PrescribedMedication> builder)
        {
            // Table Mapping
            builder.ToTable("PrescribedMedications");

            // Composite Primary Key
            builder.HasKey(pm => new { pm.MedicationPrescriptionId, pm.MedicationId });

            // Properties
            builder.Property(pm => pm.MedicationPrescriptionId).IsRequired();
            builder.Property(pm => pm.MedicationId).IsRequired();
            builder.Property(pm => pm.Dosage).IsRequired().HasMaxLength(100);
            builder.Property(pm => pm.Frequency).IsRequired().HasMaxLength(100);
            builder.Property(pm => pm.DurationDays).IsRequired();
            builder.Property(pm => pm.SpecialInstructions).IsRequired(false).HasMaxLength(500);

            // Indexes
            builder.HasIndex(pm => pm.MedicationPrescriptionId)
                .HasDatabaseName("IX_PrescribedMedication_PrescriptionId");

            builder.HasIndex(pm => pm.MedicationId)
                .HasDatabaseName("IX_PrescribedMedication_MedicationId");

            // Prescription Relationship (Many-to-One)
            builder.HasOne(pm => pm.MedicationPrescription)
                .WithMany(p => p.PrescribedMedications)
                .HasForeignKey(pm => pm.MedicationPrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Medication Relationship (Many-to-One)
            builder.HasOne(pm => pm.Medication)
                .WithMany(m => m.PrescribedMedications)
                .HasForeignKey(pm => pm.MedicationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Check Constraint - Ensure DurationDays is positive
            builder.HasCheckConstraint("CK_PrescribedMedication_Duration", "[DurationDays] > 0");
        }
    }
}
