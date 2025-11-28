using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Enums.Laboratory;
using Shuryan.Core.Entities.System.Review;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.LaboratoryConfigurations
{
    public class LabOrderEntityConfiguration : AuditableEntityConfiguration<LabOrder>
    {
        public override void Configure(EntityTypeBuilder<LabOrder> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("LabOrders");

            // Properties
            builder.Property(lo => lo.LabPrescriptionId).IsRequired();
            builder.Property(lo => lo.LaboratoryId).IsRequired();
            builder.Property(lo => lo.PatientId).IsRequired();
            builder.Property(lo => lo.Status).IsRequired().HasConversion<int>().HasDefaultValue(LabOrderStatus.NewRequest);
            builder.Property(lo => lo.SampleCollectionType).IsRequired().HasConversion<int>().HasDefaultValue(SampleCollectionType.LabVisit);
            builder.Property(lo => lo.TestsTotalCost).IsRequired().HasPrecision(10, 2);
            builder.Property(lo => lo.SampleCollectionDeliveryCost).IsRequired().HasPrecision(10, 2).HasDefaultValue(0);
            builder.Property(lo => lo.ConfirmedByLabAt).IsRequired(false);
            builder.Property(lo => lo.PaidAt).IsRequired(false);
            builder.Property(lo => lo.SamplesCollectedAt).IsRequired(false);
            builder.Property(lo => lo.CancellationReason).IsRequired(false).HasMaxLength(500);
            builder.Property(lo => lo.CancelledAt).IsRequired(false);
            builder.Property(lo => lo.RejectionReason).IsRequired(false).HasMaxLength(500);
            builder.Property(lo => lo.RejectedAt).IsRequired(false);

            // Indexes
            builder.HasIndex(lo => lo.LabPrescriptionId)
                .IsUnique()
                .HasDatabaseName("IX_LabOrder_LabPrescriptionId");

            builder.HasIndex(lo => lo.LaboratoryId)
                .HasDatabaseName("IX_LabOrder_LaboratoryId");

            builder.HasIndex(lo => lo.PatientId)
                .HasDatabaseName("IX_LabOrder_PatientId");

            builder.HasIndex(lo => lo.Status)
                .HasDatabaseName("IX_LabOrder_Status");

            builder.HasIndex(lo => new { lo.LaboratoryId, lo.Status })
                .HasDatabaseName("IX_LabOrder_Laboratory_Status");

            builder.HasIndex(lo => new { lo.PatientId, lo.Status })
                .HasDatabaseName("IX_LabOrder_Patient_Status");

            // LabPrescription Relationship (One-to-One)
            builder.HasOne(lo => lo.LabPrescription)
                .WithOne(lp => lp.LabOrder)
                .HasForeignKey<LabOrder>(lo => lo.LabPrescriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Laboratory Relationship (Many-to-One)
            builder.HasOne(lo => lo.Laboratory)
                .WithMany(l => l.LabOrders)
                .HasForeignKey(lo => lo.LaboratoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient Relationship (Many-to-One)
            builder.HasOne(lo => lo.Patient)
                .WithMany(p => p.LabOrders)
                .HasForeignKey(lo => lo.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Lab Results Relationship (One-to-Many)
            builder.HasMany(lo => lo.LabResults)
                .WithOne(lr => lr.LabOrder)
                .HasForeignKey(lr => lr.LabOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Laboratory Review Relationship (One-to-One, Optional)
            builder.HasOne(lo => lo.LaboratoryReview)
                .WithOne(lr => lr.LabOrder)
                .HasForeignKey<LaboratoryReview>(lr => lr.LabOrderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Check Constraint - Ensure costs are non-negative
            builder.HasCheckConstraint("CK_LabOrder_Costs", "[TestsTotalCost] >= 0 AND [SampleCollectionDeliveryCost] >= 0");
        }
    }
}