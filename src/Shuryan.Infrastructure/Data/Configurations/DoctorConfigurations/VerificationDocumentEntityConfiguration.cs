using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Enums;

namespace Shuryan.Infrastructure.Data.Configurations.DoctorConfigurations
{
    public class VerificationDocumentEntityConfiguration : IEntityTypeConfiguration<VerificationDocument>
    {
        public void Configure(EntityTypeBuilder<VerificationDocument> builder)
        {
            builder.HasKey(vd => vd.Id);

			builder.Property(vd => vd.DocumentUrl)
				   .IsRequired()
				   .HasMaxLength(500);
			
            builder.Property(vd => vd.Type)
                   .HasConversion<int>()
                   .IsRequired();

			builder.Property(vd => vd.Status)
				   .HasConversion<int>()
				   .IsRequired()
				   .HasDefaultValue(VerificationDocumentStatus.Pending);

			builder.Property(vd => vd.RejectionReason)
				   .HasMaxLength(500);

			builder.Property(vd => vd.UploadedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

			// Relationships
			builder.HasOne(vd => vd.Doctor)
                   .WithMany(d => d.VerificationDocuments)
                   .HasForeignKey(vd => vd.DoctorId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
