using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.LaboratoryConfigurations
{
    public class LabServiceEntityConfiguration : AuditableEntityConfiguration<LabService>
    {
        public override void Configure(EntityTypeBuilder<LabService> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("LabServices");

            // Properties
            builder.Property(ls => ls.LaboratoryId).IsRequired();
            builder.Property(ls => ls.LabTestId).IsRequired();
            builder.Property(ls => ls.Price).IsRequired().HasPrecision(10, 2);
            builder.Property(ls => ls.IsAvailable).IsRequired().HasDefaultValue(true);
            builder.Property(ls => ls.LabSpecificNotes).IsRequired(false).HasMaxLength(500);

            // Indexes
            builder.HasIndex(ls => ls.LaboratoryId)
                .HasDatabaseName("IX_LabService_LaboratoryId");

            builder.HasIndex(ls => ls.LabTestId)
                .HasDatabaseName("IX_LabService_LabTestId");

            builder.HasIndex(ls => new { ls.LaboratoryId, ls.LabTestId })
                .IsUnique()
                .HasDatabaseName("IX_LabService_Laboratory_Test");

            builder.HasIndex(ls => new { ls.LaboratoryId, ls.IsAvailable })
                .HasDatabaseName("IX_LabService_Laboratory_Available");

            // Laboratory Relationship (Many-to-One)
            builder.HasOne(ls => ls.Laboratory)
                .WithMany(l => l.LabServices)
                .HasForeignKey(ls => ls.LaboratoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // LabTest Relationship (Many-to-One)
            builder.HasOne(ls => ls.LabTest)
                .WithMany(lt => lt.LabServices)
                .HasForeignKey(ls => ls.LabTestId)
                .OnDelete(DeleteBehavior.Restrict);

            // Check Constraint - Ensure price is non-negative
            builder.HasCheckConstraint("CK_LabService_Price", "[Price] >= 0");
        }
    }
}