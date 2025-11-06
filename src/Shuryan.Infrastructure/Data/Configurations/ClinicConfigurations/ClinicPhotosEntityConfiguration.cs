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

            // Table Mapping
            builder.ToTable("ClinicPhotos");

            // Properties
            builder.Property(cp => cp.ClinicId).IsRequired();
            builder.Property(cp => cp.PhotoUrl).IsRequired().HasMaxLength(500);
            builder.Property(cp => cp.DisplayOrder).IsRequired();

            // Indexes
            builder.HasIndex(cp => cp.ClinicId)
                .HasDatabaseName("IX_ClinicPhoto_ClinicId");

            builder.HasIndex(cp => new { cp.ClinicId, cp.DisplayOrder })
                .IsUnique()
                .HasDatabaseName("IX_ClinicPhoto_Clinic_Order");

            // Clinic Relationship (Many-to-One)
            builder.HasOne(cp => cp.Clinic)
                .WithMany(c => c.Photos)
                .HasForeignKey(cp => cp.ClinicId)
                .OnDelete(DeleteBehavior.Cascade);

            // Check Constraint - Ensure DisplayOrder is between 0 and 5 (max 6 photos)
            builder.HasCheckConstraint("CK_ClinicPhoto_DisplayOrder", "[DisplayOrder] >= 0 AND [DisplayOrder] <= 5");
        }
    }
}
