using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Enums;
using Shuryan.Core.Entities.External.Clinic;
using Shuryan.Core.Enums.Identity;

namespace Shuryan.Infrastructure.Data.Configurations.ClinicConfigurations
{
    public class ClinicEntityConfiguration : IEntityTypeConfiguration<Clinic>
    {
        public void Configure(EntityTypeBuilder<Clinic> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(c => c.ClinicStatus)
                   .HasConversion<int>()
                   .IsRequired()
                   .HasDefaultValue(Status.Active);

			builder.Property(c => c.FacilityVideoUrl)
				   .HasMaxLength(500);

			builder.Property(c => c.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

			// Relationships
			builder.HasOne(c => c.Doctor)
                   .WithOne(d => d.Clinic)
                   .HasForeignKey<Clinic>(c => c.DoctorId)
                   .OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(c => c.Address)
	               .WithOne()
	               .HasForeignKey<Clinic>(c => c.AddressId)
	               .OnDelete(DeleteBehavior.Cascade);

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

        }
    }
}
