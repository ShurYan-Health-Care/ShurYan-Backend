using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;

namespace Shuryan.Infrastructure.Data.Configurations.Identity
{
    public class UserEntityConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
            builder.Property(u => u.LastName).IsRequired().HasMaxLength(50);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
            builder.Property(u => u.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(u => u.IsActive).IsRequired().HasDefaultValue(true);

            builder.UseTptMappingStrategy();

            builder.HasIndex(u => u.Email)
                   .HasDatabaseName("IX_User_Email")
                   .IsUnique();

            builder.HasIndex(u => u.IsActive)
                   .HasDatabaseName("IX_User_IsActive");

            builder.HasIndex(u => new { u.FirstName, u.LastName })
                   .HasDatabaseName("IX_User_FullName");
        }
    }
}
