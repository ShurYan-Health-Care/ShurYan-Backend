using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;

namespace Shuryan.Infrastructure.Data.Configurations.IdentityConfigurations
{
    public class UserEntityConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Primary Key
            builder.HasKey(u => u.Id);

            // TPT Mapping Strategy
            builder.UseTptMappingStrategy();

            // Basic Properties
            builder.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
            builder.Property(u => u.LastName).IsRequired().HasMaxLength(50);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
            builder.Property(u => u.PhoneNumber).HasMaxLength(20);

            // Auditable Properties
            builder.Property(u => u.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(u => u.CreatedBy).IsRequired(false);
            builder.Property(u => u.UpdatedAt).IsRequired(false);
            builder.Property(u => u.UpdatedBy).IsRequired(false);

            // Soft Delete Properties
            builder.Property(u => u.IsDeleted).IsRequired().HasDefaultValue(false);
            builder.Property(u => u.DeletedAt).IsRequired(false);
            builder.Property(u => u.DeletedBy).IsRequired(false);

            // Login Tracking Properties
            builder.Property(u => u.EmailVerifiedAt).IsRequired(false);
            builder.Property(u => u.LastLoginAt).IsRequired(false);
            builder.Property(u => u.LastLoginIp).HasMaxLength(50);

            // OAuth Properties
            builder.Property(u => u.OAuthProvider).HasMaxLength(50);
            builder.Property(u => u.OAuthProviderId).HasMaxLength(255);
            builder.Property(u => u.ProfilePictureUrl).HasMaxLength(500);
            builder.Property(u => u.IsOAuthAccount).IsRequired().HasDefaultValue(false);

            // Global Query Filter - Soft Delete
            builder.HasQueryFilter(u => !u.IsDeleted);

            // Indexes
            builder.HasIndex(u => u.Email)
                .HasDatabaseName("IX_User_Email")
                .IsUnique();

            builder.HasIndex(u => new { u.FirstName, u.LastName })
                .HasDatabaseName("IX_User_FullName");

            builder.HasIndex(u => u.IsDeleted)
                .HasDatabaseName("IX_User_IsDeleted");
        }
    }
}
