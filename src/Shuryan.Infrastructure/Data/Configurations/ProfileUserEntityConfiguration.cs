using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Infrastructure.Data.Configurations
{
	public class ProfileUserEntityConfiguration : IEntityTypeConfiguration<ProfileUser>
	{
		public void Configure(EntityTypeBuilder<ProfileUser> builder)
		{
			builder.Property(pu => pu.Gender)
				   .HasConversion<int>();

			builder.Property(pu => pu.ProfileImageUrl)
				   .HasMaxLength(500);
		}
	}
}
