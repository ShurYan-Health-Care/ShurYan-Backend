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

            // Table Mapping
            builder.ToTable("DoctorDocuments");

            // Properties
            builder.Property(dd => dd.DoctorId).IsRequired();
            builder.Property(dd => dd.DocumentUrl).IsRequired().HasMaxLength(500);
            builder.Property(dd => dd.Type).IsRequired().HasConversion<int>();
            builder.Property(dd => dd.Status).IsRequired().HasConversion<int>().HasDefaultValue(VerificationDocumentStatus.UnderReview);
            builder.Property(dd => dd.RejectionReason).IsRequired(false).HasMaxLength(500);

            // Indexes
            builder.HasIndex(dd => dd.DoctorId)
                .HasDatabaseName("IX_DoctorDocument_DoctorId");

            builder.HasIndex(dd => dd.Status)
                .HasDatabaseName("IX_DoctorDocument_Status");

            builder.HasIndex(dd => new { dd.DoctorId, dd.Type })
                .HasDatabaseName("IX_DoctorDocument_Doctor_Type");

            // Doctor Relationship (Many-to-One)
            builder.HasOne(dd => dd.Doctor)
                .WithMany(d => d.VerificationDocuments)
                .HasForeignKey(dd => dd.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
