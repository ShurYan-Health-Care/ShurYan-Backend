using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums;

namespace Shuryan.Infrastructure.Data.Configurations
{
	public class AppointmentEntityConfiguration : IEntityTypeConfiguration<Appointment>
	{
		public void Configure(EntityTypeBuilder<Appointment> builder)
		{
			builder.HasKey(a => a.Id);

			// Properties Configuration
			builder.Property(a => a.Rating)
				   .HasDefaultValue(0);

			builder.Property(a => a.ConsultationFee)
				   .IsRequired()
				   .HasPrecision(10, 2);

			builder.Property(a => a.SessionDurationMinutes)
				   .IsRequired();

			builder.Property(a => a.ConsultationType)
				   .HasConversion<int>()
				   .IsRequired();

			builder.Property(a => a.Status)
				   .HasConversion<int>()
				   .IsRequired()
				   .HasDefaultValue(AppointmentStatus.Confirmed);

			builder.Property(a => a.CancellationReason)
				   .HasMaxLength(500);

			// Relationships
			builder.HasOne(a => a.Patient)
				   .WithMany(p => p.Appointments)
				   .HasForeignKey(a => a.PatientId)
				   .OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(a => a.Doctor)
				   .WithMany(d => d.Appointments)
				   .HasForeignKey(a => a.DoctorId)
				   .OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(a => a.ConsultationRecord)
				   .WithOne(cr => cr.Appointment)
				   .HasForeignKey<ConsultationRecord>(cr => cr.AppointmentId)
				   .OnDelete(DeleteBehavior.Cascade);

			// Check Constraints
			builder.HasCheckConstraint("CK_Appointment_TimeValidation", "[ScheduledStartTime] < [ScheduledEndTime]");

			builder.HasCheckConstraint("CK_Appointment_ConsultationFee", "[ConsultationFee] >= 0");

			builder.HasCheckConstraint("CK_Appointment_SessionDuration", "[SessionDurationMinutes] > 0 AND [SessionDurationMinutes] <= 480");

			builder.HasCheckConstraint("CK_Appointment_Rating", "[Rating] >= 0 AND [Rating] <= 5");
		}
	}
}
