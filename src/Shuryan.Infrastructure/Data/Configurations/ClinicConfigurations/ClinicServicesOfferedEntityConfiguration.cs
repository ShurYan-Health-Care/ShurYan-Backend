using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Clinic;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.ClinicConfigurations
{
    public class ClinicServicesOfferedEntityConfiguration : AuditableEntityConfiguration<ClinicService>
	{
        public override void Configure(EntityTypeBuilder<ClinicService> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("ClinicServices");

            // Properties
            builder.Property(cso => cso.ClinicId).IsRequired();
            builder.Property(cso => cso.ServiceType).IsRequired().HasConversion<int>();

            // Indexes
            builder.HasIndex(cso => cso.ClinicId)
                .HasDatabaseName("IX_ClinicService_ClinicId");

            builder.HasIndex(cso => cso.ServiceType)
                .HasDatabaseName("IX_ClinicService_ServiceType");

            builder.HasIndex(cso => new { cso.ClinicId, cso.ServiceType })
                .IsUnique()
                .HasDatabaseName("IX_ClinicService_Clinic_Service");

            // Clinic Relationship (Many-to-One)
            builder.HasOne(cso => cso.Clinic)
                .WithMany(c => c.OfferedServices)
                .HasForeignKey(cso => cso.ClinicId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
