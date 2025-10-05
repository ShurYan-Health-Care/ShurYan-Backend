using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical.Appointments;

namespace Shuryan.Infrastructure.Data.Configurations.DoctorConfigurations
{
    public class ConsultationRecordEntityConfiguration : IEntityTypeConfiguration<ConsultationRecord>
    {
        public void Configure(EntityTypeBuilder<ConsultationRecord> builder)
        {
            builder.HasKey(cr => cr.Id);

            builder.Property(cr => cr.ChiefComplaint)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(cr => cr.HistoryOfPresentIllness)
                   .IsRequired()
                   .HasMaxLength(2000);

            builder.Property(cr => cr.PhysicalExamination)
                   .IsRequired()
                   .HasMaxLength(2000);

            builder.Property(cr => cr.Diagnosis)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(cr => cr.ManagementPlan)
                   .IsRequired()
                   .HasMaxLength(2000);

			builder.Property(cr => cr.RecordedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

			// Relationships
			builder.HasOne(cr => cr.Appointment)
                   .WithOne(a => a.ConsultationRecord)
                   .HasForeignKey<ConsultationRecord>(cr => cr.AppointmentId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}