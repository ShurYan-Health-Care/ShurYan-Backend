using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Data.Configurations.PharmacyConfigurations
{
    public class PharmacyDocumentEntityConfiguration : IEntityTypeConfiguration<PharmacyDocument>
    {
        public void Configure(EntityTypeBuilder<PharmacyDocument> builder)
        {
            builder.HasKey(pd => pd.Id);

            builder.Property(pd => pd.UploadedAt).IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(pd => pd.DocumentUrl)
                .IsRequired();

            builder.Property(pd => pd.Type)
                .HasConversion<string>()
                .HasMaxLength(60);

            builder.Property(pd => pd.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.HasOne(pd => pd.Pharmacy)
                .WithMany(p => p.VerificationDocuments)
                .HasForeignKey(pd => pd.PharmacyId)
                .OnDelete(DeleteBehavior.Cascade);

			builder.Property(pd => pd.Status)
				   .HasConversion<int>()
				   .IsRequired()
				   .HasDefaultValue(VerificationDocumentStatus.Pending);
		}
    }
}
