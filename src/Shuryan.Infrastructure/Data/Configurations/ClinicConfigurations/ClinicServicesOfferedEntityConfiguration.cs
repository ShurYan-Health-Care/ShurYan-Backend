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

			builder.HasKey(cso => cso.Id);

            builder.Property(cso => cso.ServiceType)
                   .HasConversion<int>()
                   .IsRequired();

            // Relationships
            builder.HasOne(cso => cso.Clinic)
                   .WithMany(c => c.OfferedServices)
                   .HasForeignKey(cso => cso.ClinicId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
