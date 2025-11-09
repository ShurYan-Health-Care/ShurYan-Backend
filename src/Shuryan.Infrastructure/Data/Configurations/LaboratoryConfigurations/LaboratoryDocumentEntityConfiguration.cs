using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Enums;
using Shuryan.Core.Entities.Shared;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.LaboratoryConfigurations
{
    public class LaboratoryDocumentEntityConfiguration : AuditableEntityConfiguration<LaboratoryDocument>
    {
        public override void Configure(EntityTypeBuilder<LaboratoryDocument> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("LaboratoryDocuments");

            // Properties
            builder.Property(ld => ld.LaboratoryId).IsRequired();
            builder.Property(ld => ld.DocumentUrl).IsRequired().HasMaxLength(500);
            builder.Property(ld => ld.Type).IsRequired().HasConversion<int>();
            builder.Property(ld => ld.Status).IsRequired().HasConversion<int>().HasDefaultValue(VerificationDocumentStatus.UnderReview);
            builder.Property(ld => ld.RejectionReason).IsRequired(false).HasMaxLength(500);

            // Indexes
            builder.HasIndex(ld => ld.LaboratoryId)
                .HasDatabaseName("IX_LaboratoryDocument_LaboratoryId");

            builder.HasIndex(ld => ld.Status)
                .HasDatabaseName("IX_LaboratoryDocument_Status");

            builder.HasIndex(ld => new { ld.LaboratoryId, ld.Type })
                .HasDatabaseName("IX_LaboratoryDocument_Laboratory_Type");

            // Laboratory Relationship (Many-to-One)
            builder.HasOne(ld => ld.Laboratory)
                .WithMany(l => l.VerificationDocuments)
                .HasForeignKey(ld => ld.LaboratoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
