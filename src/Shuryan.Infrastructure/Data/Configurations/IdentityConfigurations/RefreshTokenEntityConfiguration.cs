using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Infrastructure.Data.Configurations.IdentityConfigurations
{
    public class RefreshTokenEntityConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            // Table Mapping
            builder.ToTable("RefreshTokens");

            // Primary Key
            builder.HasKey(rt => rt.Id);

            // Properties
            builder.Property(rt => rt.Token).IsRequired().HasMaxLength(512);
            builder.Property(rt => rt.UserId).IsRequired();
            builder.Property(rt => rt.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(rt => rt.ExpiresAt).IsRequired();
            builder.Property(rt => rt.IsRevoked).IsRequired().HasDefaultValue(false);
            builder.Property(rt => rt.RevokedAt).IsRequired(false);
            builder.Property(rt => rt.CreatedByIp).HasMaxLength(50);
            builder.Property(rt => rt.RevokedByIp).HasMaxLength(50);
            builder.Property(rt => rt.RevocationReason).HasMaxLength(200);

            // Ignore Computed Properties
            builder.Ignore(rt => rt.IsActive);
            builder.Ignore(rt => rt.IsExpired);

            // Relationship with User
            builder.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Check Constraint - Ensure expiration is after creation
            builder.HasCheckConstraint("CK_RefreshToken_Expiration", "[ExpiresAt] > [CreatedAt]");
        }
    }
}