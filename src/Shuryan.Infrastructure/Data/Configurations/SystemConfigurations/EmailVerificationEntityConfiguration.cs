using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.System;

namespace Shuryan.Infrastructure.Data.Configurations.SystemConfigurations
{
    public class EmailVerificationEntityConfiguration : IEntityTypeConfiguration<EmailVerification>
    {
        public void Configure(EntityTypeBuilder<EmailVerification> builder)
        {
            builder.ToTable("EmailVerifications");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.OtpCode)
                .IsRequired()
                .HasMaxLength(6)
                .IsFixedLength();

            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.ExpiresAt)
                .IsRequired();

            builder.Property(e => e.IsUsed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.AttemptCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(e => e.RequestedFromIp)
                .HasMaxLength(50);

            builder.Property(e => e.VerificationType)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("EmailVerification");

            // Relationship with User
            builder.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Check constraints
            builder.HasCheckConstraint(
                "CK_EmailVerification_ExpiresAt",
                "[ExpiresAt] > [CreatedAt]");

            builder.HasCheckConstraint(
                "CK_EmailVerification_AttemptCount",
                "[AttemptCount] >= 0 AND [AttemptCount] <= 10");
        }
    }
}