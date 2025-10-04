using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Data.Configurations.PharmacyConfigurations
{
    public class PharmacyOrderConfiguration : IEntityTypeConfiguration<PharmacyOrder>
    {
        public void Configure(EntityTypeBuilder<PharmacyOrder> builder)
        {
            builder.HasKey(po => po.Id);

            builder.Property(po => po.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(lpi => lpi.CreatedAt).IsRequired().
                HasDefaultValueSql("GETUTCDATE()");

            // تحديد نوع البيانات للسعر عشان الدقة
            builder.Property(po => po.TotalCost)
                .HasColumnType("decimal(18,2)");

            builder.Property(po => po.DeliveryFee)
                .HasColumnType("decimal(18,2)");

            // تحويل الـ enums إلى نص
            builder.Property(po => po.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(po => po.DeliveryType)
                .HasConversion<string>()
                .HasMaxLength(50);

            // تعريف العلاقات
            builder.HasOne(po => po.Patient)
                .WithMany(p => p.PharmacyOrders) // المريض له كذا طلب
                .HasForeignKey(po => po.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(po => po.Pharmacy)
                .WithMany(ph => ph.Orders) // الصيدلية ليها كذا طلب
                .HasForeignKey(po => po.PharmacyId)
                .OnDelete(DeleteBehavior.Restrict);

            // تعريف علاقة واحد لواحد بين الروشتة والطلب
            builder.HasOne(po => po.MedicationPrescription)
                .WithOne(p => p.PharmacyOrder)
                .HasForeignKey<PharmacyOrder>(po => po.MedicationPrescription) // الـ FK موجود في جدول الطلبات
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
