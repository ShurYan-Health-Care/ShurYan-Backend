using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External.Pharmacies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Data.Configurations.PrescriptionConfigurations
{
    public class PrescribedMedicationEntityConfiguration : IEntityTypeConfiguration<PrescribedMedication>
    {
        public void Configure(EntityTypeBuilder<PrescribedMedication> builder)
        {
            builder.HasKey(pm => new { pm.MedicationPrescriptionId, pm.MedicationId });

            // تحديد خصائص الحقول الإلزامية
            builder.Property(pm => pm.Dosage)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(pm => pm.Frequency)
                .IsRequired()
                .HasMaxLength(100);

            // تعريف العلاقة مع الروشتة (الروشتة تحتوي على أدوية موصوفة)
            builder.HasOne(pm => pm.MedicationPrescription)
                .WithMany(p => p.PrescribedMedications)
                .HasForeignKey(pm => pm.MedicationPrescriptionId)
                .OnDelete(DeleteBehavior.Cascade); // لو الروشتة اتمسحت، امسح الأدوية اللي جواها

            // تعريف العلاقة مع كتالوج الأدوية
            builder.HasOne(pm => pm.Medication)
                .WithMany(m => m.PrescribedMedications)
                .HasForeignKey(pm => pm.MedicationId)
                .OnDelete(DeleteBehavior.Restrict); // امنع مسح دواء من الكتالوج لو مكتوب في أي روشتة


        }
    }
}
