using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.System.Review;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.ReviewConfigurations
{
	public class PharmacyReviewEntityConfiguration : AuditableEntityConfiguration<PharmacyReview>
	{
		public override void Configure(EntityTypeBuilder<PharmacyReview> builder)
		{
			base.Configure(builder);

			// Table Mapping
			builder.ToTable("PharmacyReviews");

			// Properties
			builder.Property(pr => pr.PharmacyOrderId).IsRequired();
			builder.Property(pr => pr.PatientId).IsRequired();
			builder.Property(pr => pr.PharmacyId).IsRequired();
			builder.Property(pr => pr.OverallSatisfaction).IsRequired();
			builder.Property(pr => pr.MedicationAvailability).IsRequired();
			builder.Property(pr => pr.ServiceQuality).IsRequired();
			builder.Property(pr => pr.DeliverySpeed).IsRequired();
			builder.Property(pr => pr.ValueForMoney).IsRequired();
			builder.Property(pr => pr.IsEdited).IsRequired().HasDefaultValue(false);

			// Indexes
			builder.HasIndex(pr => pr.PharmacyOrderId)
				.IsUnique()
				.HasDatabaseName("IX_PharmacyReview_PharmacyOrderId");

			builder.HasIndex(pr => pr.PatientId)
				.HasDatabaseName("IX_PharmacyReview_PatientId");

			builder.HasIndex(pr => pr.PharmacyId)
				.HasDatabaseName("IX_PharmacyReview_PharmacyId");

			// Relationships
			builder.HasOne(pr => pr.PharmacyOrder)
				.WithOne(po => po.PharmacyReview)
				.HasForeignKey<PharmacyReview>(pr => pr.PharmacyOrderId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(pr => pr.Patient)
				.WithMany(p => p.PharmacyReviews)
				.HasForeignKey(pr => pr.PatientId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(pr => pr.Pharmacy)
				.WithMany(ph => ph.PharmacyReviews)
				.HasForeignKey(pr => pr.PharmacyId)
				.OnDelete(DeleteBehavior.Restrict);

			// Check Constraints
			builder.HasCheckConstraint("CK_PharmacyReview_OverallSatisfaction", "[OverallSatisfaction] >= 1 AND [OverallSatisfaction] <= 5");
			builder.HasCheckConstraint("CK_PharmacyReview_MedicationAvailability", "[MedicationAvailability] >= 1 AND [MedicationAvailability] <= 5");
			builder.HasCheckConstraint("CK_PharmacyReview_ServiceQuality", "[ServiceQuality] >= 1 AND [ServiceQuality] <= 5");
			builder.HasCheckConstraint("CK_PharmacyReview_DeliverySpeed", "[DeliverySpeed] >= 1 AND [DeliverySpeed] <= 5");
			builder.HasCheckConstraint("CK_PharmacyReview_ValueForMoney", "[ValueForMoney] >= 1 AND [ValueForMoney] <= 5");
		}
	}
}