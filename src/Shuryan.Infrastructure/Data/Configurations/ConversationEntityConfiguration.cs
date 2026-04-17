using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.System;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations
{
    public class ConversationEntityConfiguration : AuditableEntityConfiguration<Conversation>
    {
        public override void Configure(EntityTypeBuilder<Conversation> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("Conversations");

            // Properties
            builder.Property(c => c.UserId).IsRequired();
            builder.Property(c => c.UserRole).IsRequired().HasConversion<int>();
            builder.Property(c => c.Title).IsRequired(false).HasMaxLength(200);
            builder.Property(c => c.LastMessage).IsRequired(false).HasMaxLength(500);
            builder.Property(c => c.LastMessageAt).IsRequired(false);
            builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);

            // Indexes
            builder.HasIndex(c => c.UserId)
                .HasDatabaseName("IX_Conversation_UserId");

            builder.HasIndex(c => c.IsActive)
                .HasDatabaseName("IX_Conversation_IsActive");

            builder.HasIndex(c => new { c.UserId, c.IsActive })
                .HasDatabaseName("IX_Conversation_User_IsActive");

            builder.HasIndex(c => c.LastMessageAt)
                .HasDatabaseName("IX_Conversation_LastMessageAt");

            // Relationships
            builder.HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
