using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.System.Review;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.ReviewConfigurations
{
	public class DoctorReviewEntityConfiguration : AuditableEntityConfiguration<DoctorReview>
	{
		public override void Configure(EntityTypeBuilder<DoctorReview> builder)
		{
			base.Configure(builder);

			builder.HasKey(dr => dr.Id);

			builder.Property(dr => dr.OverallSatisfaction)
				.IsRequired();

			builder.Property(dr => dr.WaitingTime)
				.IsRequired();

			builder.Property(dr => dr.CommunicationQuality)
				.IsRequired();

			builder.Property(dr => dr.ClinicCleanliness)
				.IsRequired();

			builder.Property(dr => dr.ValueForMoney)
				.IsRequired();

			builder.Property(dr => dr.Comment)
				.HasMaxLength(500);

			builder.Property(dr => dr.IsAnonymous)
				.IsRequired()
				.HasDefaultValue(false);

			builder.Property(dr => dr.IsEdited)
				.IsRequired()
				.HasDefaultValue(false);

			builder.Property(dr => dr.CreatedAt)
				.IsRequired()
				.HasDefaultValueSql("GETUTCDATE()");

			// For the future: Reply of Dr.
			builder.Property(dr => dr.DoctorReply)
				.HasMaxLength(300);

			// Relationships
			builder.HasOne(dr => dr.Appointment)
				.WithOne(a => a.DoctorReview)
				.HasForeignKey<DoctorReview>(dr => dr.AppointmentId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(dr => dr.Patient)
				.WithMany(p => p.DoctorReviews)
				.HasForeignKey(dr => dr.PatientId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(dr => dr.Doctor)
				.WithMany(d => d.DoctorReviews)
				.HasForeignKey(dr => dr.DoctorId)
				.OnDelete(DeleteBehavior.Restrict);

			// Check Constraints
			builder.HasCheckConstraint("CK_DoctorReview_OverallSatisfaction", "[OverallSatisfaction] >= 1 AND [OverallSatisfaction] <= 5");
			builder.HasCheckConstraint("CK_DoctorReview_WaitingTime", "[WaitingTime] >= 1 AND [WaitingTime] <= 5");
			builder.HasCheckConstraint("CK_DoctorReview_CommunicationQuality", "[CommunicationQuality] >= 1 AND [CommunicationQuality] <= 5");
			builder.HasCheckConstraint("CK_DoctorReview_ClinicCleanliness", "[ClinicCleanliness] >= 1 AND [ClinicCleanliness] <= 5");
			builder.HasCheckConstraint("CK_DoctorReview_ValueForMoney", "[ValueForMoney] >= 1 AND [ValueForMoney] <= 5");
		}
	}
}