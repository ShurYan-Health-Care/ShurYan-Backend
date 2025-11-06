using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External.Clinic;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.ClinicConfigurations
{
    public class ClinicPhoneNumberEntityConfiguration : AuditableEntityConfiguration<ClinicPhoneNumber>
	{
        public override void Configure(EntityTypeBuilder<ClinicPhoneNumber> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("ClinicPhoneNumbers");

            // Properties
            builder.Property(cpn => cpn.ClinicId).IsRequired();
            builder.Property(cpn => cpn.Number).IsRequired().HasMaxLength(20);
            builder.Property(cpn => cpn.Type).IsRequired().HasConversion<int>();

            // Indexes
            builder.HasIndex(cpn => cpn.ClinicId)
                .HasDatabaseName("IX_ClinicPhoneNumber_ClinicId");

            builder.HasIndex(cpn => new { cpn.ClinicId, cpn.Type })
                .HasDatabaseName("IX_ClinicPhoneNumber_Clinic_Type");

            builder.HasIndex(cpn => new { cpn.ClinicId, cpn.Number })
                .IsUnique()
                .HasDatabaseName("IX_ClinicPhoneNumber_Clinic_Number");

            // Clinic Relationship (Many-to-One)
            builder.HasOne(cpn => cpn.Clinic)
                .WithMany(c => c.PhoneNumbers)
                .HasForeignKey(cpn => cpn.ClinicId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
