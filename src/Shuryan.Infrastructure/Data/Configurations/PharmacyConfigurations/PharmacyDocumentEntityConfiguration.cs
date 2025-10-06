using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Data.Configurations.PharmacyConfigurations
{
    public class PharmacyDocumentEntityConfiguration : AuditableEntityConfiguration<PharmacyDocument>
    {
        public override void Configure(EntityTypeBuilder<PharmacyDocument> builder)
        {
            base.Configure(builder);

            builder.HasKey(pd => pd.Id);

            builder.Property(pd => pd.DocumentUrl)
                .IsRequired();

            builder.Property(pd => pd.Type)
                .HasConversion<int>()
                .HasMaxLength(60);

            builder.Property(pd => pd.Status)
                .HasConversion<int>()
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
