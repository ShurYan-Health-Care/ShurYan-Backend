using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.System.Review;

namespace Shuryan.Infrastructure.Data.Configurations.Review
{
	public class LaboratoryReviewEntityConfiguration : IEntityTypeConfiguration<LaboratoryReview>
	{
		public void Configure(EntityTypeBuilder<LaboratoryReview> builder)
		{
			builder.HasKey(lr => lr.Id);

			// Properties
			builder.Property(lr => lr.OverallSatisfaction).IsRequired();
			builder.Property(lr => lr.ResultAccuracy).IsRequired();
			builder.Property(lr => lr.DeliverySpeed).IsRequired();
			builder.Property(lr => lr.ServiceQuality).IsRequired();
			builder.Property(lr => lr.ValueForMoney).IsRequired();

			builder.Property(lr => lr.IsEdited)
				.IsRequired()
				.HasDefaultValue(false);

			builder.Property(lr => lr.CreatedAt)
				.IsRequired()
				.HasDefaultValueSql("GETUTCDATE()");

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