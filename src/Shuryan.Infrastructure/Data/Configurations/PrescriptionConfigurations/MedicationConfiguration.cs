using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Data.Configurations.PrescriptionConfigurations
{
    public class MedicationConfiguration : IEntityTypeConfiguration<Medication>
    {
        public void Configure(EntityTypeBuilder<Medication> builder)
        {
            // تعريف المفتاح الأساسي
            builder.HasKey(m => m.Id);

            // تحديد خصائص اسم الدواء التجاري
            builder.Property(m => m.BrandName)
                .IsRequired()
                .HasMaxLength(250);

            // تحديد خصائص الاسم العلمي (اختياري)
            builder.Property(m => m.GenericName)
                .HasMaxLength(200);

            // تحديد خصائص التركيز (اختياري)
            builder.Property(m => m.Strength)
                .HasMaxLength(100);

            // تحويل الـ enum إلى نص في قاعدة البيانات
            builder.Property(m => m.DosageForm)
                .HasConversion<string>()
                .HasMaxLength(50);

            // تعريف علاقة واحد لكثير مع الأدوية الموصوفة
            builder.HasMany(m => m.PrescribedMedications)
                .WithOne(pm => pm.Medication)
                .HasForeignKey(pm => pm.MedicationId);

        }
    }
}
