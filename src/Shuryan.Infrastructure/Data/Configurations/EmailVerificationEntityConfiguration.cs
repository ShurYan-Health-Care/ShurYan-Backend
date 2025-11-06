using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.System;

namespace Shuryan.Infrastructure.Data.Configurations
{
    public class EmailVerificationEntityConfiguration : IEntityTypeConfiguration<EmailVerification>
    {
        public void Configure(EntityTypeBuilder<EmailVerification> builder)
        {
            // Table Mapping
            builder.ToTable("EmailVerifications");

            // Primary Key
            builder.HasKey(ev => ev.Id);

            // Properties
            builder.Property(ev => ev.UserId).IsRequired();
            builder.Property(ev => ev.Email).IsRequired().HasMaxLength(255);
            builder.Property(ev => ev.OtpCode).IsRequired().HasMaxLength(6);
            builder.Property(ev => ev.CreatedAt).IsRequired();
            builder.Property(ev => ev.ExpiresAt).IsRequired();
            builder.Property(ev => ev.IsUsed).IsRequired().HasDefaultValue(false);
            builder.Property(ev => ev.VerifiedAt).IsRequired(false);
            builder.Property(ev => ev.AttemptCount).IsRequired().HasDefaultValue(0);
            builder.Property(ev => ev.RequestedFromIp).IsRequired(false).HasMaxLength(50);
            builder.Property(ev => ev.VerificationType).IsRequired().HasConversion<int>();

            // Ignore computed property
            builder.Ignore(ev => ev.IsValid);

            // Indexes
            builder.HasIndex(ev => ev.UserId)
                .HasDatabaseName("IX_EmailVerification_UserId");

            builder.HasIndex(ev => ev.Email)
                .HasDatabaseName("IX_EmailVerification_Email");

            builder.HasIndex(ev => new { ev.Email, ev.OtpCode })
                .HasDatabaseName("IX_EmailVerification_Email_OtpCode");

            builder.HasIndex(ev => ev.ExpiresAt)
                .HasDatabaseName("IX_EmailVerification_ExpiresAt");

            builder.HasIndex(ev => new { ev.IsUsed, ev.ExpiresAt })
                .HasDatabaseName("IX_EmailVerification_IsUsed_ExpiresAt");

            // Relationships
            builder.HasOne(ev => ev.User)
                .WithMany()
                .HasForeignKey(ev => ev.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Check Constraints
            builder.HasCheckConstraint("CK_EmailVerification_AttemptCount", "[AttemptCount] >= 0 AND [AttemptCount] <= 10");
        }
    }
}
