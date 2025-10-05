using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Entities.System.Review;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Data.Configurations.PharmacyConfigurations
{
    public class PharmacyOrderEntityConfiguration : IEntityTypeConfiguration<PharmacyOrder>
    {
        public void Configure(EntityTypeBuilder<PharmacyOrder> builder)
        {
            builder.HasKey(po => po.Id);

            builder.Property(po => po.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(po => po.CreatedAt).IsRequired().
                HasDefaultValueSql("GETUTCDATE()");

            builder.Property(po => po.TotalCost)
                .HasColumnType("decimal(18,2)");

            builder.Property(po => po.DeliveryFee)
                .HasColumnType("decimal(18,2)");

            builder.Property(po => po.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(po => po.DeliveryType)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.HasOne(po => po.Patient)
                .WithMany(p => p.PharmacyOrders)
                .HasForeignKey(po => po.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(po => po.Pharmacy)
                .WithMany(ph => ph.Orders) 
                .HasForeignKey(po => po.PharmacyId)
                .OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(po => po.Prescription)
				.WithOne(p => p.PharmacyOrder)
				.HasForeignKey<PharmacyOrder>(po => po.PrescriptionId)
				.IsRequired(false)
				.OnDelete(DeleteBehavior.SetNull);

			builder.HasOne(po => po.PharmacyReview)
	                .WithOne(pr => pr.PharmacyOrder)
	                .HasForeignKey<PharmacyReview>(pr => pr.PharmacyOrderId)
	                .IsRequired(false)
	                .OnDelete(DeleteBehavior.Restrict);
		}
    }
}
