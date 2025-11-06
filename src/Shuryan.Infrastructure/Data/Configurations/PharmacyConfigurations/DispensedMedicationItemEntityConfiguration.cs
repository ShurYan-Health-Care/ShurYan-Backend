using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.PharmacyConfigurations
{
    public class DispensedMedicationItemEntityConfiguration : AuditableEntityConfiguration<DispensedMedicationItem>
    {
        public override void Configure(EntityTypeBuilder<DispensedMedicationItem> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("DispensedMedicationItems");

            // Properties
            builder.Property(dmi => dmi.DispensingRecordId).IsRequired();
            builder.Property(dmi => dmi.MedicationId).IsRequired();
            builder.Property(dmi => dmi.QuantityDispensed).IsRequired();
            builder.Property(dmi => dmi.UnitPrice).IsRequired().HasPrecision(10, 2);
            builder.Property(dmi => dmi.TotalPrice).IsRequired().HasPrecision(10, 2);

            // Indexes
            builder.HasIndex(dmi => dmi.DispensingRecordId)
                .HasDatabaseName("IX_DispensedMedicationItem_DispensingRecordId");

            builder.HasIndex(dmi => dmi.MedicationId)
                .HasDatabaseName("IX_DispensedMedicationItem_MedicationId");

            builder.HasIndex(dmi => new { dmi.DispensingRecordId, dmi.MedicationId })
                .HasDatabaseName("IX_DispensedMedicationItem_Record_Medication");

            // Dispensing Record Relationship (Many-to-One)
            builder.HasOne(dmi => dmi.DispensingRecord)
                .WithMany(dr => dr.DispensedMedications)
                .HasForeignKey(dmi => dmi.DispensingRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            // Medication Relationship (Many-to-One)
            builder.HasOne(dmi => dmi.Medication)
                .WithMany()
                .HasForeignKey(dmi => dmi.MedicationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Check Constraints
            builder.HasCheckConstraint("CK_DispensedMedicationItem_Quantity", "[QuantityDispensed] > 0");
            builder.HasCheckConstraint("CK_DispensedMedicationItem_Prices", "[UnitPrice] >= 0 AND [TotalPrice] >= 0");
        }
    }
}
