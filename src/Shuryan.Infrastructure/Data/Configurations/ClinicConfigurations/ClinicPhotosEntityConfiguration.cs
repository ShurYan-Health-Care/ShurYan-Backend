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
    public class ClinicPhotosEntityConfiguration : AuditableEntityConfiguration<ClinicPhoto>
	{
        public override void Configure(EntityTypeBuilder<ClinicPhoto> builder)
        {
			base.Configure(builder);

			builder.HasKey(cp => cp.Id);

			builder.Property(cp => cp.PhotoUrl)
				   .IsRequired()
				   .HasMaxLength(500);

			// Relationships
			builder.HasOne(cp => cp.Clinic)
                   .WithMany(c => c.Photos)
                   .HasForeignKey(cp => cp.ClinicId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
