using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;

namespace Shuryan.Infrastructure.Data.Configurations
{
	public class UserEntityConfiguration : IEntityTypeConfiguration<User>
	{
		public void Configure(EntityTypeBuilder<User> builder)
		{
			builder.HasKey(u => u.Id);
			builder.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
			builder.Property(u => u.LastName).IsRequired().HasMaxLength(50);
			builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
			builder.Property(u => u.CreatedAt).IsRequired();
			builder.Property(u => u.IsActive).IsRequired().HasDefaultValue(true);
			builder.Property(u => u.UserRole).HasConversion<int>().IsRequired();

			builder.HasDiscriminator(u => u.UserRole)
				.HasValue<Patient>(UserRole.Patient)
				.HasValue<Doctor>(UserRole.Doctor)
				.HasValue<DoctorVerifier>(UserRole.Verifier);

			builder.HasIndex(u => u.Email)
				   .HasDatabaseName("IX_User_Email")
				   .IsUnique();

			builder.HasIndex(u => u.UserRole)
				   .HasDatabaseName("IX_User_UserRole");

			builder.HasIndex(u => u.IsActive)
				   .HasDatabaseName("IX_User_IsActive");

			builder.HasIndex(u => new { u.FirstName, u.LastName })
				   .HasDatabaseName("IX_User_FullName");
		}
	}
}
