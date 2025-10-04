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
    public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
    {
        public void Configure(EntityTypeBuilder<Prescription> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.PrescriptionNumber)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(p => p.DigitalSignature)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.GeneralInstructions)
                .HasMaxLength(1000);

            builder.Property(p => p.FollowUpInstructions)
                .HasMaxLength(1000);


            builder.Property(p => p.IssuedDate).IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // تعريف العلاقات مع الدكتور والمريض والموعد
            builder.HasOne(p => p.Doctor)
                .WithMany() // الدكتور الواحد له كذا روشتة
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Patient)
                .WithMany() // المريض الواحد له كذا روشتة
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // الكود ده بيعرف Entity Framework إن العلاقة One-to-One
            builder.HasOne(p => p.Appointment)       // الروشتة الواحدة ليها موعد واحد
                   .WithOne(a => a.Prescription)      // والموعد الواحد له روشتة واحدة
                   .HasForeignKey<Prescription>(p => p.AppointmentId) // والمفتاح الأجنبي موجود في جدول الروشتة
                   .OnDelete(DeleteBehavior.Cascade); // لو الموعد اتمسح، امسح الروشتة بتاعته معاه

            builder.HasOne(p => p.PharmacyOrder)
                .WithOne(po => po.Prescription)
                .HasForeignKey<Prescription>(p => p.PharmacyOrderId)
                .OnDelete(DeleteBehavior.Cascade); // لو طلب الصيدلية اتمسح، امسح الروشتة بتاعته معاه

            // تعريف علاقة واحد لكثير مع الأدوية الموصوفة
            builder.HasMany(p => p.PrescribedMedications)
                .WithOne(pm => pm.MedicationPrescription)
                .HasForeignKey(pm => pm.MedicationPrescriptionId)
                .OnDelete(DeleteBehavior.Cascade); // لو الروشتة اتمسحت، امسح الأدوية اللي جواها
        }
    }
}
