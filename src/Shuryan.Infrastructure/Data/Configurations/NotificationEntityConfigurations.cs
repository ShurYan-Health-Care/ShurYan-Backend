using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.System;
using Shuryan.Core.Enums.Notifications;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations
{
	public class NotificationEntityConfiguration : AuditableEntityConfiguration<Notification>
	{
		public override void Configure(EntityTypeBuilder<Notification> builder)
		{
			base.Configure(builder);

			// Table Mapping
			builder.ToTable("Notifications");

			// Properties
			builder.Property(n => n.UserId).IsRequired();
			builder.Property(n => n.Type).IsRequired().HasConversion<int>();
			builder.Property(n => n.Title).IsRequired().HasMaxLength(100);
			builder.Property(n => n.Message).IsRequired().HasMaxLength(500);
			builder.Property(n => n.RelatedEntityType).IsRequired(false).HasMaxLength(50);
			builder.Property(n => n.RelatedEntityId).IsRequired(false);
			builder.Property(n => n.IsRead).IsRequired().HasDefaultValue(false);
			builder.Property(n => n.ReadAt).IsRequired(false);
			builder.Property(n => n.Priority).IsRequired().HasConversion<int>().HasDefaultValue(NotificationPriority.Normal);
			builder.Property(n => n.DeliveryMethod).IsRequired().HasConversion<int>().HasDefaultValue(NotificationDeliveryMethod.InApp);
			builder.Property(n => n.IsSent).IsRequired().HasDefaultValue(false);
			builder.Property(n => n.SentAt).IsRequired(false);
			builder.Property(n => n.FailureReason).IsRequired(false).HasMaxLength(500);

			// Indexes
			builder.HasIndex(n => n.UserId)
				.HasDatabaseName("IX_Notification_UserId");

			builder.HasIndex(n => n.IsRead)
				.HasDatabaseName("IX_Notification_IsRead");

			builder.HasIndex(n => new { n.UserId, n.IsRead })
				.HasDatabaseName("IX_Notification_User_IsRead");

			builder.HasIndex(n => n.Type)
				.HasDatabaseName("IX_Notification_Type");

			builder.HasIndex(n => new { n.RelatedEntityType, n.RelatedEntityId })
				.HasDatabaseName("IX_Notification_RelatedEntity");


			// Relationships
			builder.HasOne(n => n.User)
				.WithMany()
				.HasForeignKey(n => n.UserId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
