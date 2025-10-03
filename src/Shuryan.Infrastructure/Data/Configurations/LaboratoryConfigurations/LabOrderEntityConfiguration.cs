using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums;

namespace Shuryan.Infrastructure.Data.Configurations.LaboratoryConfigurations
{
    public class LabOrderEntityConfiguration : IEntityTypeConfiguration<LabOrder>
    {
        public void Configure(EntityTypeBuilder<LabOrder> builder)
        {
            builder.HasKey(lo => lo.Id);

            builder.Property(lo => lo.Status)
                   .HasConversion<int>()
                   .IsRequired()
                   .HasDefaultValue(LabOrderStatus.PendingPayment);

			builder.Property(lo => lo.SampleCollectionType)
                   .HasConversion<int>()
                   .IsRequired()
				   .HasDefaultValue(SampleCollectionType.LabVisit);

			builder.Property(lo => lo.TestsTotalCost)
                   .IsRequired()
                   .HasPrecision(10, 2);

			builder.Property(lo => lo.SampleCollectionDeliveryCost)
                   .HasPrecision(10, 2)
				   .HasDefaultValue(0);

			builder.Property(lo => lo.CancellationReason)
				   .HasMaxLength(500);

			builder.HasOne(lo => lo.LabPrescription)
				   .WithOne(lp => lp.LabOrder)
				   .HasForeignKey<LabOrder>(lo => lo.LabPrescriptionId)
				   .OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(lo => lo.Laboratory)
				   .WithMany(l => l.LabOrders)
				   .HasForeignKey(lo => lo.LaboratoryId)
				   .OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(lo => lo.Patient)
				   .WithMany(p => p.LabOrders)
				   .HasForeignKey(lo => lo.PatientId)
				   .OnDelete(DeleteBehavior.Restrict);

			builder.HasMany(lo => lo.LabResults)
				   .WithOne(lr => lr.LabOrder)
				   .HasForeignKey(lr => lr.LabOrderId)
				   .OnDelete(DeleteBehavior.Cascade);

			builder.HasCheckConstraint("CK_LabOrder_TotalCost", "[TotalCost] >= [TestsTotalCost]");
			builder.HasCheckConstraint("CK_LabOrder_Costs", "[TestsTotalCost] >= 0 AND [SampleCollectionDeliveryCost] >= 0");
		}
	}
}