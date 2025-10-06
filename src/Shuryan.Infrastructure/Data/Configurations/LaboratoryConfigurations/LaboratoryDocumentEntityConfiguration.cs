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

            builder.HasKey(ld => ld.Id);

            builder.Property(ld => ld.DocumentUrl)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(ld => ld.Type)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(ld => ld.Status)
                   .HasConversion<int>()
                   .IsRequired()
                   .HasDefaultValue(VerificationDocumentStatus.Pending);

            builder.Property(ld => ld.RejectionReason)
                   .HasMaxLength(500);

			builder.HasOne(ld => ld.Laboratory)
                   .WithMany(l => l.VerificationDocuments)
                   .HasForeignKey(ld => ld.LaboratoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
