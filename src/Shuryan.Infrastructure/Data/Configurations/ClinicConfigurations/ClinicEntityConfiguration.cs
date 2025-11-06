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
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.ClinicConfigurations
{
    public class ClinicEntityConfiguration : AuditableEntityConfiguration<Clinic>
    {
        public override void Configure(EntityTypeBuilder<Clinic> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("Clinics");

            // Properties
            builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
            builder.Property(c => c.ClinicStatus).IsRequired().HasConversion<int>().HasDefaultValue(Status.Active);
            builder.Property(c => c.FacilityVideoUrl).IsRequired(false).HasMaxLength(500);
            builder.Property(c => c.DoctorId).IsRequired();
            builder.Property(c => c.AddressId).IsRequired();

            // Indexes
            builder.HasIndex(c => c.DoctorId)
                .IsUnique()
                .HasDatabaseName("IX_Clinic_DoctorId");

            builder.HasIndex(c => c.ClinicStatus)
                .HasDatabaseName("IX_Clinic_Status");

            builder.HasIndex(c => c.AddressId)
                .HasDatabaseName("IX_Clinic_AddressId");

            // Doctor Relationship (One-to-One)
			builder.HasOne(c => c.Doctor)
                   .WithOne(d => d.Clinic)
                   .HasForeignKey<Clinic>(c => c.DoctorId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Address Relationship (One-to-One)
			builder.HasOne(c => c.Address)
	               .WithOne()
	               .HasForeignKey<Clinic>(c => c.AddressId)
	               .OnDelete(DeleteBehavior.Cascade);

            // Photos Relationship (One-to-Many)
			builder.HasMany(c => c.Photos)
                   .WithOne(p => p.Clinic)
                   .HasForeignKey(p => p.ClinicId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Phone Numbers Relationship (One-to-Many)
            builder.HasMany(c => c.PhoneNumbers)
                   .WithOne(pn => pn.Clinic)
                   .HasForeignKey(pn => pn.ClinicId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Offered Services Relationship (One-to-Many)
            builder.HasMany(c => c.OfferedServices)
                   .WithOne(os => os.Clinic)
                   .HasForeignKey(os => os.ClinicId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
