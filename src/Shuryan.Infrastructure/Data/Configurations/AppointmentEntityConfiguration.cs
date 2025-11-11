using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Entities.Medical.Consultations;
using Shuryan.Core.Entities.System.Review;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations
{
    public class AppointmentEntityConfiguration : AuditableEntityConfiguration<Appointment>
	{
		public override void Configure(EntityTypeBuilder<Appointment> builder)
		{
			base.Configure(builder);

			// Table Mapping
			builder.ToTable("Appointments");

			// Properties
			builder.Property(a => a.PatientId).IsRequired();
			builder.Property(a => a.DoctorId).IsRequired();
			builder.Property(a => a.PreviousAppointmentId).IsRequired(false);
			builder.Property(a => a.ScheduledStartTime).IsRequired();

			builder.Property(a => a.ScheduledEndTime).IsRequired();
			builder.Property(a => a.ConsultationType).IsRequired().HasConversion<int>();
			builder.Property(a => a.ConsultationFee).IsRequired().HasPrecision(10, 2);
			builder.Property(a => a.SessionDurationMinutes).IsRequired();
			builder.Property(a => a.Status).IsRequired().HasConversion<int>().HasDefaultValue(AppointmentStatus.Confirmed);
			builder.Property(a => a.CancellationReason).IsRequired(false).HasMaxLength(500);
			builder.Property(a => a.CancelledAt).IsRequired(false);
			builder.Property(a => a.ActualStartTime).IsRequired(false);
			builder.Property(a => a.ActualEndTime).IsRequired(false);

			// Indexes
			builder.HasIndex(a => a.PatientId)
				.HasDatabaseName("IX_Appointment_PatientId");

			builder.HasIndex(a => a.DoctorId)
				.HasDatabaseName("IX_Appointment_DoctorId");

			builder.HasIndex(a => a.Status)
				.HasDatabaseName("IX_Appointment_Status");

			builder.HasIndex(a => a.ScheduledStartTime)
				.HasDatabaseName("IX_Appointment_ScheduledStartTime");

			builder.HasIndex(a => new { a.DoctorId, a.ScheduledStartTime })
				.HasDatabaseName("IX_Appointment_Doctor_StartTime");

			builder.HasIndex(a => new { a.PatientId, a.Status })
				.HasDatabaseName("IX_Appointment_Patient_Status");

			// Composite index for GetDoctorPatientsAsync query performance
			builder.HasIndex(a => new { a.DoctorId, a.Status, a.ScheduledStartTime })
				.HasDatabaseName("IX_Appointment_Doctor_Status_StartTime");

			// Relationships
			builder.HasOne(a => a.Patient)
				.WithMany(p => p.Appointments)
				.HasForeignKey(a => a.PatientId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(a => a.Doctor)
				.WithMany(d => d.Appointments)
				.HasForeignKey(a => a.DoctorId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(a => a.PreviousAppointment)
				.WithMany(a => a.FollowUpAppointments)
				.HasForeignKey(a => a.PreviousAppointmentId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(a => a.Prescription)
				.WithOne(p => p.Appointment)
				.HasForeignKey<Prescription>(p => p.AppointmentId)
				.IsRequired(false)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(a => a.ConsultationRecord)
				.WithOne(cr => cr.Appointment)
				.HasForeignKey<ConsultationRecord>(cr => cr.AppointmentId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(a => a.LabPrescription)
				.WithOne(lp => lp.Appointment)
				.HasForeignKey(lp => lp.AppointmentId)
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
