using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.PharmacyConfigurations
{
    public class PharmacyDocumentEntityConfiguration : AuditableEntityConfiguration<PharmacyDocument>
    {
        public override void Configure(EntityTypeBuilder<PharmacyDocument> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("PharmacyDocuments");

            // Properties
            builder.Property(pd => pd.PharmacyId).IsRequired();
            builder.Property(pd => pd.DocumentUrl).IsRequired().HasMaxLength(500);
            builder.Property(pd => pd.Type).IsRequired().HasConversion<int>();
            builder.Property(pd => pd.Status).IsRequired().HasConversion<int>().HasDefaultValue(VerificationDocumentStatus.UnderReview);
            builder.Property(pd => pd.RejectionReason).IsRequired(false).HasMaxLength(500);

            // Indexes
            builder.HasIndex(pd => pd.PharmacyId)
                .HasDatabaseName("IX_PharmacyDocument_PharmacyId");

            builder.HasIndex(pd => pd.Status)
                .HasDatabaseName("IX_PharmacyDocument_Status");

            builder.HasIndex(pd => new { pd.PharmacyId, pd.Type })
                .HasDatabaseName("IX_PharmacyDocument_Pharmacy_Type");

            // Relationships
            builder.HasOne(pd => pd.Pharmacy)
                .WithMany(p => p.VerificationDocuments)
                .HasForeignKey(pd => pd.PharmacyId)
                .OnDelete(DeleteBehavior.Cascade);
		}
    }
}
