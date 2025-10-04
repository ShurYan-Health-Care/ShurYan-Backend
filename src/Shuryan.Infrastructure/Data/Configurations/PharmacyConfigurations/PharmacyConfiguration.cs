using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Data.Configurations.PharmacyConfigurations
{
    public class PharmacyConfiguration : IEntityTypeConfiguration<Pharmacy>
    {
        public void Configure(EntityTypeBuilder<Pharmacy> builder)
        {
            // خصائص الصيدلية نفسها
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(250);

            // تعريف علاقة واحد لواحد مع العنوان
            builder.HasOne(p => p.Address)
                .WithOne()
                .HasForeignKey<Pharmacy>(p => p.AddressId)
                .OnDelete(DeleteBehavior.Cascade); // نمنع مسح العنوان لو الصيدلية اتمسحت

            builder.HasOne(p => p.Verifier)
                .WithMany(v => v.VerifiedPharmacies)
                .HasForeignKey(p => p.VerifierId)
                .OnDelete(DeleteBehavior.SetNull); // لو الموثق اتمسح، خلي الموثق بتاع الصيدلية يبقى null
                                                   // تعريف علاقة واحد لكثير مع مستندات التوثيق

            builder.HasMany(p => p.VerificationDocuments)
                .WithOne(d => d.Pharmacy)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.Cascade); // لو الصيدلية اتمسحت، امسح ورقها معاها

            // تعريف علاقة واحد لكثير مع مواعيد العمل
            builder.HasMany(p => p.WorkingHours)
                .WithOne(wh => wh.Pharmacy)
                .HasForeignKey(wh => wh.PharmacyId)
                .OnDelete(DeleteBehavior.Cascade);

            // تعريف علاقة واحد لكثير مع الطلبات
            builder.HasMany(p => p.Orders)
                .WithOne(o => o.Pharmacy)
                .HasForeignKey(o => o.PharmacyId)
                .OnDelete(DeleteBehavior.Restrict); // نمنع مسح الصيدلية لو عليها طلبات
        }
    }
}
