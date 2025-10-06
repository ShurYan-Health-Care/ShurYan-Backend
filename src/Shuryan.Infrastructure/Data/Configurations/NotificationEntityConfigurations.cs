using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
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

			builder.HasKey(n => n.Id);

			// Properties
			builder.Property(n => n.Title)
                .IsRequired()
				.HasMaxLength(100);

			builder.Property(n => n.Message)
				.IsRequired()
				.HasMaxLength(500);

			builder.Property(n => n.RelatedEntityType)
				.HasMaxLength(50);

			builder.Property(n => n.Type)
				.HasConversion<int>()
				.IsRequired();

			builder.Property(n => n.Priority)
				.HasConversion<int>()
				.IsRequired()
				.HasDefaultValue(NotificationPriority.Normal);

			builder.Property(n => n.DeliveryMethod)
				.HasConversion<int>()
				.IsRequired()
				.HasDefaultValue(NotificationDeliveryMethod.InApp);

			builder.Property(n => n.IsRead)
				.IsRequired()
				.HasDefaultValue(false);

			builder.Property(n => n.IsSent)
				.IsRequired()
				.HasDefaultValue(false);

			builder.Property(n => n.FailureReason)
				.HasMaxLength(500);


			// Relationships
			builder.HasOne(n => n.User)
				.WithMany()
				.HasForeignKey(n => n.UserId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
