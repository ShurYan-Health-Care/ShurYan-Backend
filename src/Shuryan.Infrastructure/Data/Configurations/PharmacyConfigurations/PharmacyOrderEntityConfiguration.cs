using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Entities.System.Review;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Data.Configurations.PharmacyConfigurations
{
    public class PharmacyOrderEntityConfiguration : AuditableEntityConfiguration<PharmacyOrder>
    {
        public override void Configure(EntityTypeBuilder<PharmacyOrder> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("PharmacyOrders");

            // Properties
            builder.Property(po => po.OrderNumber).IsRequired().HasMaxLength(50);
            builder.Property(po => po.PatientId).IsRequired();
            builder.Property(po => po.PharmacyId).IsRequired();
            builder.Property(po => po.PrescriptionId).IsRequired(false);
            builder.Property(po => po.Status).IsRequired().HasConversion<int>();
            builder.Property(po => po.TotalCost).IsRequired().HasPrecision(10, 2);
            builder.Property(po => po.DeliveryFee).IsRequired().HasPrecision(10, 2);
            builder.Property(po => po.DeliveryType).IsRequired().HasConversion<int>();
            builder.Property(po => po.EstimatedDeliveryTime).IsRequired(false);
            builder.Property(po => po.DeliveryPersonPhone).IsRequired().HasMaxLength(20);
            builder.Property(po => po.DeliveryPersonName).IsRequired(false).HasMaxLength(100);
            builder.Property(po => po.DeliveryNotes).IsRequired(false).HasMaxLength(500);
            builder.Property(po => po.ActualDeliveryTime).IsRequired(false);
            builder.Property(po => po.PatientConfirmed).IsRequired(false);
            builder.Property(po => po.PatientConfirmedAt).IsRequired(false);
            builder.Property(po => po.PatientNotes).IsRequired(false).HasMaxLength(1000);
            builder.Property(po => po.PatientDigitalSignature).IsRequired(false).HasMaxLength(5000);

            // Indexes
            builder.HasIndex(po => po.OrderNumber)
                .IsUnique()
                .HasDatabaseName("IX_PharmacyOrder_OrderNumber");

            builder.HasIndex(po => po.PatientId)
                .HasDatabaseName("IX_PharmacyOrder_PatientId");

            builder.HasIndex(po => po.PharmacyId)
                .HasDatabaseName("IX_PharmacyOrder_PharmacyId");

            builder.HasIndex(po => po.Status)
                .HasDatabaseName("IX_PharmacyOrder_Status");

            builder.HasIndex(po => new { po.PharmacyId, po.Status })
                .HasDatabaseName("IX_PharmacyOrder_Pharmacy_Status");

            builder.HasIndex(po => new { po.PatientId, po.Status })
                .HasDatabaseName("IX_PharmacyOrder_Patient_Status");

            // Patient Relationship (Many-to-One)
            builder.HasOne(po => po.Patient)
                .WithMany(p => p.PharmacyOrders)
                .HasForeignKey(po => po.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Pharmacy Relationship (Many-to-One)
            builder.HasOne(po => po.Pharmacy)
                .WithMany(ph => ph.Orders)
                .HasForeignKey(po => po.PharmacyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prescription Relationship (One-to-One, Optional)
            builder.HasOne(po => po.Prescription)
                .WithOne(p => p.PharmacyOrder)
                .HasForeignKey<PharmacyOrder>(po => po.PrescriptionId)
                .IsRequired(false)
				.OnDelete(DeleteBehavior.SetNull);

            // Pharmacy Review Relationship (One-to-One, Optional)
            builder.HasOne(po => po.PharmacyReview)
                .WithOne(pr => pr.PharmacyOrder)
                .HasForeignKey<PharmacyReview>(pr => pr.PharmacyOrderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Order Items Relationship (One-to-Many)
            builder.HasMany(po => po.OrderItems)
                .WithOne(oi => oi.PharmacyOrder)
                .HasForeignKey(oi => oi.PharmacyOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Check Constraints - Ensure costs are non-negative
            builder.HasCheckConstraint("CK_PharmacyOrder_Costs", "[TotalCost] >= 0 AND [DeliveryFee] >= 0");
        }
    }
}
