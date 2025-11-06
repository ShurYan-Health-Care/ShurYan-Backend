using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.System.Review;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.ReviewConfigurations
{
	public class LaboratoryReviewEntityConfiguration : AuditableEntityConfiguration<LaboratoryReview>
	{
		public override void Configure(EntityTypeBuilder<LaboratoryReview> builder)
		{
			base.Configure(builder);

			// Table Mapping
			builder.ToTable("LaboratoryReviews");

			// Properties
			builder.Property(lr => lr.LabOrderId).IsRequired();
			builder.Property(lr => lr.PatientId).IsRequired();
			builder.Property(lr => lr.LaboratoryId).IsRequired();
			builder.Property(lr => lr.OverallSatisfaction).IsRequired();
			builder.Property(lr => lr.ResultAccuracy).IsRequired();
			builder.Property(lr => lr.DeliverySpeed).IsRequired();
			builder.Property(lr => lr.ServiceQuality).IsRequired();
			builder.Property(lr => lr.ValueForMoney).IsRequired();
			builder.Property(lr => lr.IsEdited).IsRequired().HasDefaultValue(false);

			// Indexes
			builder.HasIndex(lr => lr.LabOrderId)
				.IsUnique()
				.HasDatabaseName("IX_LaboratoryReview_LabOrderId");

			builder.HasIndex(lr => lr.PatientId)
				.HasDatabaseName("IX_LaboratoryReview_PatientId");

			builder.HasIndex(lr => lr.LaboratoryId)
				.HasDatabaseName("IX_LaboratoryReview_LaboratoryId");

			// Relationships
			builder.HasOne(lr => lr.LabOrder)
				.WithOne(lo => lo.LaboratoryReview)
				.HasForeignKey<LaboratoryReview>(lr => lr.LabOrderId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(lr => lr.Patient)
				.WithMany(p => p.LaboratoryReviews)
				.HasForeignKey(lr => lr.PatientId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(lr => lr.Laboratory)
				.WithMany(l => l.LaboratoryReviews)
				.HasForeignKey(lr => lr.LaboratoryId)
				.OnDelete(DeleteBehavior.Restrict);

			// Check Constraints
			builder.HasCheckConstraint("CK_LaboratoryReview_OverallSatisfaction", "[OverallSatisfaction] >= 1 AND [OverallSatisfaction] <= 5");
			builder.HasCheckConstraint("CK_LaboratoryReview_ResultAccuracy", "[ResultAccuracy] >= 1 AND [ResultAccuracy] <= 5");
			builder.HasCheckConstraint("CK_LaboratoryReview_DeliverySpeed", "[DeliverySpeed] >= 1 AND [DeliverySpeed] <= 5");
			builder.HasCheckConstraint("CK_LaboratoryReview_ServiceQuality", "[ServiceQuality] >= 1 AND [ServiceQuality] <= 5");
			builder.HasCheckConstraint("CK_LaboratoryReview_ValueForMoney", "[ValueForMoney] >= 1 AND [ValueForMoney] <= 5");
		}
	}
}