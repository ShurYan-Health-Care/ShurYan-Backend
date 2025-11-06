using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.PharmacyConfigurations
{
    public class PharmacyOrderItemEntityConfiguration : AuditableEntityConfiguration<PharmacyOrderItem>
    {
        public override void Configure(EntityTypeBuilder<PharmacyOrderItem> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("PharmacyOrderItems");

            // Properties
            builder.Property(poi => poi.PharmacyOrderId).IsRequired();
            builder.Property(poi => poi.RequestedMedicationId).IsRequired();
            builder.Property(poi => poi.Status).IsRequired().HasConversion<int>();
            builder.Property(poi => poi.AvailableQuantity).IsRequired(false);
            builder.Property(poi => poi.UnitPrice).IsRequired(false).HasPrecision(10, 2);
            builder.Property(poi => poi.TotalPrice).IsRequired(false).HasPrecision(10, 2);
            builder.Property(poi => poi.AlternativeMedicationId).IsRequired(false);
            builder.Property(poi => poi.AlternativeUnitPrice).IsRequired(false).HasPrecision(10, 2);
            builder.Property(poi => poi.AlternativeNotes).IsRequired(false).HasMaxLength(500);

            // Indexes
            builder.HasIndex(poi => poi.PharmacyOrderId)
                .HasDatabaseName("IX_PharmacyOrderItem_OrderId");

            builder.HasIndex(poi => poi.RequestedMedicationId)
                .HasDatabaseName("IX_PharmacyOrderItem_RequestedMedicationId");

            builder.HasIndex(poi => poi.Status)
                .HasDatabaseName("IX_PharmacyOrderItem_Status");

            builder.HasIndex(poi => new { poi.PharmacyOrderId, poi.RequestedMedicationId })
                .IsUnique()
                .HasDatabaseName("IX_PharmacyOrderItem_Order_Medication");

            // Pharmacy Order Relationship (Many-to-One)
            builder.HasOne(poi => poi.PharmacyOrder)
                .WithMany(po => po.OrderItems)
                .HasForeignKey(poi => poi.PharmacyOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Requested Medication Relationship (Many-to-One)
            builder.HasOne(poi => poi.RequestedMedication)
                .WithMany()
                .HasForeignKey(poi => poi.RequestedMedicationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Alternative Medication Relationship (Many-to-One, Optional)
            builder.HasOne(poi => poi.AlternativeMedication)
                .WithMany()
                .HasForeignKey(poi => poi.AlternativeMedicationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Check Constraints
            builder.HasCheckConstraint("CK_PharmacyOrderItem_Prices", 
                "[UnitPrice] IS NULL OR [UnitPrice] >= 0");
            
            builder.HasCheckConstraint("CK_PharmacyOrderItem_AlternativePrices", 
                "[AlternativeUnitPrice] IS NULL OR [AlternativeUnitPrice] >= 0");
        }
    }
}
