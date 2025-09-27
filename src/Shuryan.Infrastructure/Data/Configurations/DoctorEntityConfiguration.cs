using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.System;
using Shuryan.Core.Enums;

namespace Shuryan.Infrastructure.Data.Configurations
{
	public class DoctorEntityConfiguration : IEntityTypeConfiguration<Doctor>
	{
		public void Configure(EntityTypeBuilder<Doctor> builder)
		{
			builder.Property(d => d.YearsOfExperience).IsRequired();
			builder.Property(d => d.Biography).HasMaxLength(1000);
			builder.Property(d => d.MedicalSpecialty).HasConversion<int>().IsRequired();
			builder.Property(d => d.VerificationStatus).HasConversion<int>().IsRequired().HasDefaultValue(VerificationStatus.Unverified);

			builder.HasOne(d => d.Verifier)
			   .WithMany(v => v.VerifiedDoctors)
			   .HasForeignKey(d => d.VerifierId)
			   .IsRequired(false)
			   .OnDelete(DeleteBehavior.NoAction);

			builder.HasOne(d => d.Clinic)
				   .WithOne(c => c.DoctorClinic)
				   .HasForeignKey<Clinic>(c => c.DoctorId)
				   .OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(d => d.Services)
				   .WithOne(ds => ds.Doctor)
				   .HasForeignKey(ds => ds.DoctorId)
				   .OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(d => d.Overrides)
				   .WithOne(do_override => do_override.Doctor)
				   .HasForeignKey(do_override => do_override.DoctorId)
				   .OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(d => d.Availabilities)
				   .WithOne(da => da.Doctor)
				   .HasForeignKey(da => da.DoctorId)
				   .OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(d => d.VerificationDocuments)
				   .WithOne(vd => vd.Doctor)
				   .HasForeignKey(vd => vd.DoctorId)
				   .OnDelete(DeleteBehavior.Cascade);

			// Indexes
			builder.HasIndex(d => d.MedicalSpecialty)
				   .HasDatabaseName("IX_Doctor_MedicalSpecialty");

			builder.HasIndex(d => d.VerificationStatus)
				   .HasDatabaseName("IX_Doctor_VerificationStatus");

			builder.HasIndex(d => d.YearsOfExperience)
				   .HasDatabaseName("IX_Doctor_YearsOfExperience");

			// Constraints
			builder.HasCheckConstraint("CK_Doctor_YearsOfExperience", "[YearsOfExperience] >= 0 AND [YearsOfExperience] <= 60");
		}
	}
}
