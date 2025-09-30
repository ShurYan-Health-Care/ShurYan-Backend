using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Enums;
using Shuryan.Core.Entities.External;

namespace Shuryan.Infrastructure.Data.Configurations
{
    public class ClinicEntityConfiguration : IEntityTypeConfiguration<Clinic>
	{
		public void Configure(EntityTypeBuilder<Clinic> builder)
		{
			builder.HasKey(c => c.Id);

			builder.Property(c => c.Name)
				   .IsRequired()
				   .HasMaxLength(200);

			builder.Property(c => c.Status)
				   .HasConversion<int>()
				   .IsRequired()
				   .HasDefaultValue(ClinicStatus.Active);

			// Relationships
			builder.HasOne(c => c.DoctorClinic)
				   .WithOne(d => d.Clinic)
				   .HasForeignKey<Clinic>(c => c.DoctorId)
				   .OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(c => c.Address)
				   .WithOne(a => a.Clinic)
				   .HasForeignKey<Clinic>(c => c.AddressId)
				   .OnDelete(DeleteBehavior.Restrict);

			builder.HasMany(c => c.Photos)
				   .WithOne(p => p.Clinic)
				   .HasForeignKey(p => p.ClinicId)
				   .OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(c => c.PhoneNumbers)
				   .WithOne(pn => pn.Clinic)
				   .HasForeignKey(pn => pn.ClinicId)
				   .OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(c => c.OfferedServices)
				   .WithOne(os => os.Clinic)
				   .HasForeignKey(os => os.ClinicId)
				   .OnDelete(DeleteBehavior.Cascade);

			// Indexes
			builder.HasIndex(c => c.Name)
				   .HasDatabaseName("IX_Clinic_Name");

			builder.HasIndex(c => c.Status)
				   .HasDatabaseName("IX_Clinic_Status");

			builder.HasIndex(c => c.DoctorId)
				   .HasDatabaseName("IX_Clinic_DoctorId")
				   .IsUnique();
		}
	}
}
