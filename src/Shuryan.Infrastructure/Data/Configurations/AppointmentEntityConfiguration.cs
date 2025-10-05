using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Entities.Medical.Appointments;
using Shuryan.Core.Entities.System.Review;
using Shuryan.Core.Enums.Appointments;

namespace Shuryan.Infrastructure.Data.Configurations
{
    public class AppointmentEntityConfiguration : IEntityTypeConfiguration<Appointment>
	{
		public void Configure(EntityTypeBuilder<Appointment> builder)
		{
			builder.HasKey(a => a.Id);

			// Properties Configuration
			builder.Property(a => a.ScheduledStartTime)
				   .IsRequired();

			builder.Property(a => a.ScheduledEndTime)
				   .IsRequired();

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

			builder.Property(a => a.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

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

			builder.HasOne(a => a.PreviousAppointment)
				   .WithMany(a => a.FollowUpAppointments)
				   .HasForeignKey(a => a.PreviousAppointmentId)
				   .OnDelete(DeleteBehavior.Restrict);

			builder.HasMany(a => a.LabPrescription)
				   .WithOne(lp => lp.Appointment)
				   .HasForeignKey(lp => lp.AppointmentId)
				   .OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(a => a.Prescription)
				   .WithOne(p => p.Appointment)
				   .HasForeignKey<Prescription>(p => p.AppointmentId)
				   .IsRequired(false)
				   .OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(a => a.DoctorReview)
					.WithOne(dr => dr.Appointment)
					.HasForeignKey<DoctorReview>(dr => dr.AppointmentId)
					.IsRequired(false)
					.OnDelete(DeleteBehavior.Restrict);

			// Check Constraints
			builder.HasCheckConstraint("CK_Appointment_TimeValidation", "[ScheduledStartTime] < [ScheduledEndTime]");

			builder.HasCheckConstraint("CK_Appointment_ConsultationFee", "[ConsultationFee] >= 0");

			builder.HasCheckConstraint("CK_Appointment_SessionDuration", "[SessionDurationMinutes] > 0 AND [SessionDurationMinutes] <= 480");
		}
	}
}
