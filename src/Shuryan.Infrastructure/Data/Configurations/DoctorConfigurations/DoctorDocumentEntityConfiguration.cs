using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Enums;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.DoctorConfigurations
{
    public class DoctorDocumentEntityConfiguration : AuditableEntityConfiguration<DoctorDocument>
    {
        public override void Configure(EntityTypeBuilder<DoctorDocument> builder)
        {
            base.Configure(builder);

            builder.HasKey(dd => dd.Id);

			builder.Property(dd => dd.DocumentUrl)
				   .IsRequired()
				   .HasMaxLength(500);
			
            builder.Property(dd => dd.Type)
                   .HasConversion<int>()
                   .IsRequired();

			builder.Property(dd => dd.Status)
				   .HasConversion<int>()
				   .IsRequired()
				   .HasDefaultValue(VerificationDocumentStatus.Pending);

			builder.Property(dd => dd.RejectionReason)
				   .HasMaxLength(500);


			// Relationships
			builder.HasOne(dd => dd.Doctor)
                   .WithMany(d => d.VerificationDocuments)
                   .HasForeignKey(dd => dd.DoctorId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
