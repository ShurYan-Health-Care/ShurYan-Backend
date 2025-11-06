using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.System.Review;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.ReviewConfigurations
{
	public class DoctorReviewEntityConfiguration : AuditableEntityConfiguration<DoctorReview>
	{
		public override void Configure(EntityTypeBuilder<DoctorReview> builder)
		{
			base.Configure(builder);

			// Table Mapping
			builder.ToTable("DoctorReviews");

			// Properties
			builder.Property(dr => dr.AppointmentId).IsRequired();
			builder.Property(dr => dr.PatientId).IsRequired();
			builder.Property(dr => dr.DoctorId).IsRequired();

			builder.Property(dr => dr.OverallSatisfaction).IsRequired();
			builder.Property(dr => dr.WaitingTime).IsRequired();
			builder.Property(dr => dr.CommunicationQuality).IsRequired();
			builder.Property(dr => dr.ClinicCleanliness).IsRequired();
			builder.Property(dr => dr.ValueForMoney).IsRequired();
			builder.Property(dr => dr.Comment).IsRequired(false).HasMaxLength(500);
			builder.Property(dr => dr.IsAnonymous).IsRequired().HasDefaultValue(false);
			builder.Property(dr => dr.IsEdited).IsRequired().HasDefaultValue(false);
			builder.Property(dr => dr.DoctorReply).IsRequired(false).HasMaxLength(300);
			builder.Property(dr => dr.DoctorRepliedAt).IsRequired(false);

			// Indexes
			builder.HasIndex(dr => dr.AppointmentId)
				.IsUnique()
				.HasDatabaseName("IX_DoctorReview_AppointmentId");

			builder.HasIndex(dr => dr.PatientId)
				.HasDatabaseName("IX_DoctorReview_PatientId");

			builder.HasIndex(dr => dr.DoctorId)
				.HasDatabaseName("IX_DoctorReview_DoctorId");

			builder.HasIndex(dr => dr.IsAnonymous)
				.HasDatabaseName("IX_DoctorReview_IsAnonymous");

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