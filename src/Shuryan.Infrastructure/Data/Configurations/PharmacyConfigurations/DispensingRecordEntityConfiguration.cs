using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.PharmacyConfigurations
{
    public class DispensingRecordEntityConfiguration : AuditableEntityConfiguration<DispensingRecord>
    {
        public override void Configure(EntityTypeBuilder<DispensingRecord> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("DispensingRecords");

            // Properties
            builder.Property(dr => dr.PrescriptionId).IsRequired();
            builder.Property(dr => dr.PharmacyId).IsRequired();
            builder.Property(dr => dr.PatientId).IsRequired();
            builder.Property(dr => dr.DispensedAt).IsRequired();
            builder.Property(dr => dr.ReceiptNumber).IsRequired().HasMaxLength(50);
            builder.Property(dr => dr.TotalCost).IsRequired().HasPrecision(10, 2);
            builder.Property(dr => dr.PaymentMethod).IsRequired(false).HasMaxLength(50);
            builder.Property(dr => dr.Notes).IsRequired(false).HasMaxLength(1000);

            // Indexes
            builder.HasIndex(dr => dr.ReceiptNumber)
                .IsUnique()
                .HasDatabaseName("IX_DispensingRecord_ReceiptNumber");

            builder.HasIndex(dr => dr.PrescriptionId)
                .HasDatabaseName("IX_DispensingRecord_PrescriptionId");

            builder.HasIndex(dr => dr.PharmacyId)
                .HasDatabaseName("IX_DispensingRecord_PharmacyId");

            builder.HasIndex(dr => dr.PatientId)
                .HasDatabaseName("IX_DispensingRecord_PatientId");
                
            builder.HasIndex(dr => new { dr.PharmacyId, dr.DispensedAt })
                .HasDatabaseName("IX_DispensingRecord_Pharmacy_Date");

            builder.HasIndex(dr => new { dr.PatientId, dr.DispensedAt })
                .HasDatabaseName("IX_DispensingRecord_Patient_Date");

            // Prescription Relationship (Many-to-One)
            builder.HasOne(dr => dr.Prescription)
                .WithMany()
                .HasForeignKey(dr => dr.PrescriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Pharmacy Relationship (Many-to-One)
            builder.HasOne(dr => dr.Pharmacy)
                .WithMany()
                .HasForeignKey(dr => dr.PharmacyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient Relationship (Many-to-One)
            builder.HasOne(dr => dr.Patient)
                .WithMany()
                .HasForeignKey(dr => dr.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Dispensed Medications Relationship (One-to-Many)
            builder.HasMany(dr => dr.DispensedMedications)
                .WithOne(dmi => dmi.DispensingRecord)
                .HasForeignKey(dmi => dmi.DispensingRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            // Check Constraints
            builder.HasCheckConstraint("CK_DispensingRecord_TotalCost", "[TotalCost] >= 0");
        }
    }
}
