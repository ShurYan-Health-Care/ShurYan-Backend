using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Infrastructure.Data.Configurations.IdentityConfigurations
{
    public class ProfileUserEntityConfiguration : IEntityTypeConfiguration<ProfileUser>
    {
        public void Configure(EntityTypeBuilder<ProfileUser> builder)
        {
            // Table Mapping
            builder.ToTable("ProfileUsers");

            // Properties
            builder.Property(pu => pu.BirthDate).IsRequired(false);
            builder.Property(pu => pu.Gender).IsRequired(false).HasConversion<int>();
            builder.Property(pu => pu.ProfileImageUrl).IsRequired(false).HasMaxLength(500);

            // Indexes
            builder.HasIndex(pu => pu.Gender)
                .HasDatabaseName("IX_ProfileUser_Gender");
        }
    }
}
