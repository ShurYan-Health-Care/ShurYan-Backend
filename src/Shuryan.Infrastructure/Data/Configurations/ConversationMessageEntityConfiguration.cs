using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.System;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations
{
    public class ConversationMessageEntityConfiguration : AuditableEntityConfiguration<ConversationMessage>
    {
        public override void Configure(EntityTypeBuilder<ConversationMessage> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("ConversationMessages");

            // Properties
            builder.Property(cm => cm.ConversationId).IsRequired();
            builder.Property(cm => cm.Role).IsRequired().HasConversion<int>();
            builder.Property(cm => cm.Content).IsRequired().HasMaxLength(4000);
            builder.Property(cm => cm.SuggestionsJson).IsRequired(false);
            builder.Property(cm => cm.ActionsJson).IsRequired(false);
            builder.Property(cm => cm.ContextJson).IsRequired(false);
            builder.Property(cm => cm.TokenCount).IsRequired(false);
            builder.Property(cm => cm.ResponseTimeMs).IsRequired(false);

            // Indexes
            builder.HasIndex(cm => cm.ConversationId)
                .HasDatabaseName("IX_ConversationMessage_ConversationId");

            builder.HasIndex(cm => cm.Role)
                .HasDatabaseName("IX_ConversationMessage_Role");

            builder.HasIndex(cm => new { cm.ConversationId, cm.CreatedAt })
                .HasDatabaseName("IX_ConversationMessage_Conversation_CreatedAt");

            // Relationships
            builder.HasOne(cm => cm.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(cm => cm.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
